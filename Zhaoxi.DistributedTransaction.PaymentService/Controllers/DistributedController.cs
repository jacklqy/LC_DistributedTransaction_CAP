using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Zhaoxi.DistributedTransaction.EFModel;

namespace Zhaoxi.DistributedTransaction.LogisticsService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedController : ControllerBase
    {
        private readonly IConfiguration _iConfiguration;
        private readonly ICapPublisher _iCapPublisher;
        private readonly CommonServiceDbContext _UserServiceDbContext;
        private readonly ILogger<DistributedController> _Logger;
        private readonly IMongoClient _iMongoClient;
        public DistributedController(IConfiguration configuration, CommonServiceDbContext userServiceDbContext, ILogger<DistributedController> logger
            , ICapPublisher capPublisher
            , IMongoClient mongoClient
            )
        {
            this._iCapPublisher = capPublisher;
            this._iConfiguration = configuration;
            this._UserServiceDbContext = userServiceDbContext;
            this._Logger = logger;
            this._iMongoClient = mongoClient;
            if (!IsInit)//
            {
                lock (IsInit_Lock)
                {
                    if (!IsInit)
                    {
                        Console.WriteLine("初始化数据库");
                        var myCollection = _iMongoClient.GetDatabase("test").GetCollection<User>("test.collection");//初始化表
                        var result = myCollection.AsQueryable().ToList();
                        if (result==null)
                            myCollection.InsertOne(new EFModel.User() { });
                        IsInit = true;
                    }
                }
            }
        }

        private static bool IsInit = false;
        private static object IsInit_Lock = new object();


        #region 多节点贯穿协作
        [NonAction]
        [CapSubscribe("RabbitMQ.SQLServer.DistributedDemo.Logistics-Payment")]
        public void Distributed(User u, [FromCap] CapHeader header)
        {
            try
            {
                Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {Newtonsoft.Json.JsonConvert.SerializeObject(u)}");
                if (header != null)
                {
                    Console.WriteLine("message header Teacher:" + header["Teacher"]);
                    Console.WriteLine("message header Student:" + header["Student"]);
                    Console.WriteLine("message header Version:" + header["Version"]);
                    Console.WriteLine("message header   Index:" + header["Index"]);
                }

                int index = int.Parse(header["Index"]);
                index++;
                string publishName = "RabbitMQ.SQLServer.DistributedDemo.Payment-Other";

                var user = this._UserServiceDbContext.User.Find(1);
                var userNew = new User()
                {
                    Id = DateTime.Now.Millisecond,//ID要求唯一
                    Name = "Eleven" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"),
                    CompanyId = 1,
                    CompanyName = "朝夕教育" + index,
                    CreateTime = DateTime.Now,
                    CreatorId = 1,
                    LastLoginTime = DateTime.Now,
                    LastModifierId = 1,
                    LastModifyTime = DateTime.Now,
                    Password = "123456" + index,
                    State = 1,
                    Account = "Administrator" + index,
                    Email = "57265177@qq.com",
                    Mobile = "18664876677",
                    UserType = 1
                };

                #region bussiness+发布
                {
                    IDictionary<string, string> dicHeader = new Dictionary<string, string>();
                    dicHeader.Add("Teacher", header["Teacher"]);
                    dicHeader.Add("Student", header["Student"]);
                    dicHeader.Add("Version", header["Version"]);
                    dicHeader.Add("Index", index.ToString());
                    using (var session = this._iMongoClient.StartTransaction(this._iCapPublisher, autoCommit: false))
                    {
                        var collection = this._iMongoClient.GetDatabase("test").GetCollection<User>("test.collection");
                        collection.InsertOne(session, userNew);
                        Console.WriteLine("只写入MongoDB数据，不做再次publish");
                        this._iCapPublisher.Publish(publishName, user, dicHeader);//带header
                        session.CommitTransaction();
                    }
                    this._Logger.LogWarning($"This is Subscriber2 Invoke");
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("****************************************************");
                Console.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}