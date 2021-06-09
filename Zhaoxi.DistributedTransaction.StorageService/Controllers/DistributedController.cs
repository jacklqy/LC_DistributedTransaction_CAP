using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhaoxi.DistributedTransaction.EFModel;

namespace Zhaoxi.DistributedTransaction.StorageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedController : ControllerBase
    {
        private readonly IConfiguration _iConfiguration;
        private readonly ICapPublisher _iCapPublisher;
        private readonly CommonServiceDbContext _UserServiceDbContext;
        private readonly ILogger<DistributedController> _Logger;
        public DistributedController(IConfiguration configuration, CommonServiceDbContext userServiceDbContext, ILogger<DistributedController> logger
            , ICapPublisher capPublisher
            )
        {
            this._iCapPublisher = capPublisher;
            this._iConfiguration = configuration;
            this._UserServiceDbContext = userServiceDbContext;
            this._Logger = logger;
        }

        #region 多节点贯穿协作
        [NonAction]
        [CapSubscribe("RabbitMQ.SQLServer.DistributedDemo.Order-Storage")]
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
                string publishName = "RabbitMQ.SQLServer.DistributedDemo.Storage-Logistics";

                var user = this._UserServiceDbContext.User.Find(1);
                var userNew = new User()
                {
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

                IDictionary<string, string> dicHeader = new Dictionary<string, string>();
                dicHeader.Add("Teacher", header["Teacher"]);
                dicHeader.Add("Student", header["Student"]);
                dicHeader.Add("Version", header["Version"]);
                dicHeader.Add("Index", index.ToString());
                using (var trans = this._UserServiceDbContext.Database.BeginTransaction(this._iCapPublisher, autoCommit: false))
                {
                    this._UserServiceDbContext.User.Add(userNew);
                    this._iCapPublisher.Publish(publishName, user, dicHeader);//带header
                    this._UserServiceDbContext.SaveChanges();
                    Console.WriteLine("数据库业务数据已经插入");
                    trans.Commit();
                }
                this._Logger.LogWarning($"This is EFCoreTransaction Invoke");
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