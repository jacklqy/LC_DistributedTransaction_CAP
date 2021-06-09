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

namespace Zhaoxi.DistributedTransaction.OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private static string PublishName = "RabbitMQ.SQLServer.PaymentService";

        private readonly IConfiguration _iConfiguration;
        private readonly ICapPublisher _iCapPublisher;
        private readonly CommonServiceDbContext _UserServiceDbContext;
        private readonly ILogger<OrderController> _Logger;
        public OrderController(IConfiguration configuration, CommonServiceDbContext userServiceDbContext, ILogger<OrderController> logger
            , ICapPublisher capPublisher
            )
        {
            this._iCapPublisher = capPublisher;
            this._iConfiguration = configuration;
            this._UserServiceDbContext = userServiceDbContext;
            this._Logger = logger;
        }

        [NonAction]
        [CapSubscribe("RabbitMQ.SQLServer.OrderService")]//注册-监听某个队列
        public void Subscriber(User u)
        {
            Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {u}");
            //throw new Exception("Subscriber failed");
        }

        [NonAction]
        [CapSubscribe("RabbitMQ.SQLServer.OrderService", Group = "Group.Queue2")]
        public void Subscriber2(User u, [FromCap] CapHeader header)
        {
            Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {u}");
            #region bussiness
            {
                var user = this._UserServiceDbContext.User.Find(1);
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
                this._UserServiceDbContext.User.Add(userNew);
                this._UserServiceDbContext.SaveChanges();
                Console.WriteLine("数据库业务数据已经插入");
            }
            #endregion
        }


        [NonAction]
        [CapSubscribe("RabbitMQ.SQLServer.OrderService", Group = "Group.Queue3")]
        public void Subscriber3(User u, [FromCap] CapHeader header)
        {
            Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {u}");
            //if (header != null)
            //{
            //    Console.WriteLine("message header Teacher:" + header["Teacher"]);
            //    Console.WriteLine("message header Student:" + header["Student"]);
            //    Console.WriteLine("message header Version:" + header["Version"]);
            //}
            #region bussiness

            var user = this._UserServiceDbContext.User.Find(1);
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

            using (var trans = this._UserServiceDbContext.Database.BeginTransaction(this._iCapPublisher, autoCommit: false))
            {
                this._UserServiceDbContext.User.Add(userNew);
                this._UserServiceDbContext.SaveChanges();//本地的业务操作

                this._iCapPublisher.Publish(PublishName, user);//发布任务的操作  也是sql
                trans.Commit();
                Console.WriteLine("数据库业务数据已经插入");
            }
            #endregion
        }
    }
}