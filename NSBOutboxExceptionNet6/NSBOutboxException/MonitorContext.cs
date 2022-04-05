using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace NSBOutboxException
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
