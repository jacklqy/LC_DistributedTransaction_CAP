using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhaoxi.DistributedTransaction.EFModel;

namespace Zhaoxi.DistributedTransaction.UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static string PublishName = "RabbitMQ.SQLServer.UserService";

        private readonly IConfiguration _iConfiguration;
        private readonly ICapPublisher _iCapPublisher;
        private readonly CommonServiceDbContext _UserServiceDbContext;
        private readonly ILogger<UserController> _Logger;
        public UserController(ICapPublisher capPublisher, IConfiguration configuration, CommonServiceDbContext userServiceDbContext, ILogger<UserController> logger)
        {
            this._iCapPublisher = capPublisher;
            this._iConfiguration = configuration;
            this._UserServiceDbContext = userServiceDbContext;
            this._Logger = logger;
        }
        [Route("/without/transaction")]//根目录
        public async Task<IActionResult> WithoutTransaction()
        {
            var user = this._UserServiceDbContext.User.Find(1);
            this._Logger.LogWarning($"This is WithoutTransaction Invoke");
            await _iCapPublisher.PublishAsync(PublishName, user);
            return Ok();
        }

        [Route("/adotransaction/sync")]//根目录
        public IActionResult AdoTransaction()
        {
            var user = this._UserServiceDbContext.User.Find(1);
            IDictionary<string, string> dicHeader = new Dictionary<string, string>();
            dicHeader.Add("Teacher", "Eleven");
            dicHeader.Add("Student", "Seven");
            dicHeader.Add("Version", "1.2");

            using (var connection = new SqlConnection(this._iConfiguration.GetConnectionString("UserServiceConnection")))
            {
                using (var transaction = connection.BeginTransaction(this._iCapPublisher, true))
                {
                    //Something

                    _iCapPublisher.Publish(PublishName, user, dicHeader);//带header
                }
            }
            this._Logger.LogWarning($"This is AdoTransaction Invoke");
            return Ok();
        }

        [Route("/efcoretransaction/async")]//根目录
        public IActionResult EFCoreTransaction()
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

            IDictionary<string, string> dicHeader = new Dictionary<string, string>();
            dicHeader.Add("Teacher", "Eleven");
            dicHeader.Add("Student", "Seven");
            dicHeader.Add("Version", "1.2");

            using (var trans = this._UserServiceDbContext.Database.BeginTransaction(this._iCapPublisher, autoCommit: false))
            {
                this._UserServiceDbContext.User.Add(userNew);


                _iCapPublisher.Publish(PublishName, user, dicHeader);//带header

                this._UserServiceDbContext.SaveChanges();
                Console.WriteLine("数据库业务数据已经插入");
                trans.Commit();
            }
            this._Logger.LogWarning($"This is EFCoreTransaction Invoke");
            return Ok("Done");
        }

        #region 多节点贯穿协作
        [Route("/Distributed/Demo/{id}")]//根目录
        public IActionResult Distributed(int? id)
        {
            int index = id ?? 11;
            string publishName = "RabbitMQ.SQLServer.DistributedDemo.User-Order";

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
            dicHeader.Add("Teacher", "Eleven");
            dicHeader.Add("Student", "Seven");
            dicHeader.Add("Version", "1.2");
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
            return Ok("Done");
        }

        #endregion

    }
}