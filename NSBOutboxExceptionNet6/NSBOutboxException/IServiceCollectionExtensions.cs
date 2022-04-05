using Microsoft.EntityFrameworkCore;

namespace NSBOutboxException
{
    public class SqlServerSettings
    {
        public string ConnectionString { get; init; }
    }
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration conf)
        {
            var sqlServerSettings = conf.GetSection(nameof(SqlServerSettings)).Get<SqlServerSettings>();

            services.AddDbContext<MonitorContext>(options =>
                options.UseSqlServer(
                    sqlServerSettings.ConnectionString,
                    b => b.MigrationsAssembly(typeof(MonitorContext).Assembly.FullName)));

            return services;
        }
        public static void MigrateDatabase<TDbContext>(this IServiceCollection services, int? migrationTimeout = null) where TDbContext : DbContext
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<TDbContext>();            
        }
    }
}
