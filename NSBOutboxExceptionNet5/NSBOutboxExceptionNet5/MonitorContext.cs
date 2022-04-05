using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace NSBOutboxExceptionNet5
{
    public class MonitorContext : DbContext
    {
        public virtual DbSet<SomeEntity> Merchants { get; set; }

        public MonitorContext(DbContextOptions<MonitorContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }

    public class SomeEntity
    {
        public Guid Id { get; set; }
    }
}
