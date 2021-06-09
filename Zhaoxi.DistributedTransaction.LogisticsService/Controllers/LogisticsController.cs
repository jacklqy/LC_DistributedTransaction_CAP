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

namespace Zhaoxi.DistributedTransaction.LogisticsService.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class LogisticsController : ControllerBase
    {
        private static string PublishName = "RabbitMQ.SQLServer.OrderService";

        private readonly IConfiguration _iConfiguration;
        private readonly ICapPublisher _iCapPublisher;
        private readonly CommonServiceDbContext _UserServiceDbContext;
        private readonly ILogger<LogisticsController> _Logger;
        public LogisticsController(ICapPublisher capPublisher, IConfiguration configuration, CommonServiceDbContext userServiceDbContext, ILogger<LogisticsController> logger)
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
    }
}