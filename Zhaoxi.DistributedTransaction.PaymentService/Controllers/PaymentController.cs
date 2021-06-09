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

namespace Zhaoxi.DistributedTransaction.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private static string PublishName = "RabbitMQ.SQLServer.PaymentService1";
        private readonly IMongoClient _iMongoClient;
        private readonly IConfiguration _iConfiguration;
        private readonly ICapPublisher _iCapPublisher;
        private readonly CommonServiceDbContext _UserServiceDbContext;
        private readonly ILogger<PaymentController> _Logger;
        public PaymentController(IMongoClient mongoClient, ICapPublisher capPublisher, IConfiguration configuration, CommonServiceDbContext userServiceDbContext, ILogger<PaymentController> logger)
        {
            this._iMongoClient = mongoClient;
            this._iCapPublisher = capPublisher;
            this._iConfiguration = configuration;
            this._UserServiceDbContext = userServiceDbContext;
            this._Logger = logger;

            //注意：MongoDB 不能在事务中创建数据库和集合，所以你需要单独创建它们，模拟一条记录插入则会自动创建
            var mycollection = _iMongoClient.GetDatabase("test1").GetCollection<BsonDocument>("test1.collection1");
            mycollection.InsertOne(new BsonDocument { { "test", "test" } });
        }

        [NonAction]
        [CapSubscribe("RabbitMQ.SQLServer.PaymentService")]
        public void Subscriber(User u)
        {
            Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {u}");
            //throw new Exception("Subscriber failed custom!!!!!");
        }

        [NonAction]
        [CapSubscribe("RabbitMQ.SQLServer.PaymentService", Group = "PaymentService.Group.Queue2")]
        public void Subscriber2(User u, [FromCap] CapHeader header)
        {
            Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {u}");

            #region bussiness+发布
            {
                var userNew = new User()
                {
                    Name = "Eleven" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"),
                    CompanyId = 1,
                    CompanyName = "朝夕教育",
                    CreateTime = DateTime.Now,
                    CreatorId = 1,
                    LastLoginTime = DateTime.Now,
                    LastModifierId = 1,
                    LastModifyTime = DateTime.Now,
                    Password = "123456",
                    State = 1,
                    Account = "Administrator",
                    Email = "57265177@qq.com",
                    Mobile = "18664876677",
                    UserType = 1
                };

                using (var session = this._iMongoClient.StartTransaction(this._iCapPublisher, autoCommit: false))
                {
                    var collection = this._iMongoClient.GetDatabase("test1").GetCollection<BsonDocument>("test1.collection1");
                    collection.InsertOne(session, new BsonDocument { { "hello", "world" } });
                    Console.WriteLine("写入MongoDB数据");

                    this._iCapPublisher.Publish(PublishName, userNew);
                    session.CommitTransaction();
                }

                this._Logger.LogWarning($"This is Subscriber2 Invoke");
            }
            #endregion
        }

        //[NonAction]
        //[CapSubscribe("RabbitMQ.SQLServer.PaymentService", Group = "PaymentService.Group.Queue3")]
        //public void Subscriber3(User u, [FromCap] CapHeader header)
        //{
        //    Console.WriteLine($@"{DateTime.Now} Subscriber3 invoked, Info: {u}");
          

        //    //#region bussiness+发布
        //    //{
        //    //    using (var session = this._iMongoClient.StartTransaction(this._iCapPublisher, autoCommit: false))
        //    //    {
        //    //        var collection = this._iMongoClient.GetDatabase("test").GetCollection<BsonDocument>("test.collection");
        //    //        collection.InsertOne(session, new BsonDocument { { "hello", "world" } });
        //    //        Console.WriteLine("只写入MongoDB数据，不做再次publish");

        //    //        session.CommitTransaction();
        //    //    }
        //    //    this._Logger.LogWarning($"This is Subscriber2 Invoke");
        //    //}
        //    //#endregion

            
        //}
    }
}