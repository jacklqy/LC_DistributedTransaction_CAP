namespace Zhaoxi.DistributedTransaction.EFModel
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CommonServiceDbContext : DbContext
    {
        public CommonServiceDbContext(DbContextOptions<CommonServiceDbContext> options) : base(options)
        {
            Console.WriteLine($"This is {nameof(CommonServiceDbContext)}  DbContextOptions");
        }

        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
