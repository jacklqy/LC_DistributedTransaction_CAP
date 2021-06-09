using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using DotNetCore.CAP.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zhaoxi.DistributedTransaction.EFModel;
using Zhaoxi.DistributedTransaction.Utility;

namespace Zhaoxi.DistributedTransaction.StorageService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string conn = this.Configuration.GetConnectionString("StorageServiceConnection");
            string rabbitMQ = this.Configuration.GetConnectionString("RabbitMQ");

            services.Replace(ServiceDescriptor.Singleton<IStorageInitializer, CustomTableInitializer>());
            services.AddOptions<SqlServerOptions>().Configure(o =>
            {
                o.ConnectionString = conn;
                o.Schema = "ccap";//Ĭ��
            });

            services.AddCap(x =>
            {
                x.UseSqlServer(conn);
                x.UseRabbitMQ(rabbitMQ);
                x.FailedRetryCount = 10;
                x.FailedRetryInterval = 60;
                x.FailedThresholdCallback = failed =>
                {
                    var logger = failed.ServiceProvider.GetService<ILogger<Startup>>();
                    logger.LogError($@"MessageType {failed.MessageType} ʧ���ˣ� ������ {x.FailedRetryCount} ��, 
                        ��Ϣ����: {failed.Message.GetName()}");//do anything
                };

                #region ע��Consul���ӻ�
                x.UseDashboard();
                DiscoveryOptions discoveryOptions = new DiscoveryOptions();
                this.Configuration.Bind(discoveryOptions);
                x.UseDiscovery(d =>
                {
                    d.DiscoveryServerHostName = discoveryOptions.DiscoveryServerHostName;
                    d.DiscoveryServerPort = discoveryOptions.DiscoveryServerPort;
                    d.CurrentNodeHostName = discoveryOptions.CurrentNodeHostName;
                    d.CurrentNodePort = discoveryOptions.CurrentNodePort;
                    d.NodeId = discoveryOptions.NodeId;
                    d.NodeName = discoveryOptions.NodeName;
                    d.MatchPath = discoveryOptions.MatchPath;
                });
                #endregion
            });

            //DotNetCore.CAP.Diagnostics.CapDiagnosticListenerNames

            #region ��չ������  ����
            //services.Replace(ServiceDescriptor.Singleton<IStorageInitializer, CustomTableInitializer>());
            //services.AddOptions<SqlServerOptions>().Configure(o =>
            //{
            //    o.ConnectionString = conn;
            //    o.Schema = "ccap";//Ĭ��
            //});
            #endregion

            #region ��չ���л���
            services.Replace(ServiceDescriptor.Singleton<ISerializer, CustomSerializer>());
            #endregion

            #region �Զ���Dispatcher
            services.Replace(ServiceDescriptor.Singleton<IDispatcher, CustomDispatcher>());
            #endregion

            #region EFCore
            services.AddDbContext<CommonServiceDbContext>(options =>
            {
                options.UseSqlServer(conn);
            });
            #endregion

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
