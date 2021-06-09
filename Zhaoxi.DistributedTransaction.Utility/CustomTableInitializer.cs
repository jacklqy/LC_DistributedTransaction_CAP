using System;
using System.Collections.Generic;
using System.Text;
using DotNetCore.CAP;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.SqlServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zhaoxi.DistributedTransaction.Utility
{
    public class CustomTableInitializer : SqlServerStorageInitializer
    {
        private IOptions<SqlServerOptions> _optionsCustom = null;
        public CustomTableInitializer(ILogger<SqlServerStorageInitializer> logger, IOptions<SqlServerOptions> options) : base(logger, options)
        {
            this._optionsCustom = options;
        }

        public override string GetPublishedTableName()
        {
            return $"{this._optionsCustom.Value.Schema}.EPublished";
        }

        public override string GetReceivedTableName()
        {
            return $"{this._optionsCustom.Value.Schema}.EReceived";
        }
    }
}
