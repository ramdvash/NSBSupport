using NServiceBus;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NServiceBus.Persistence.Sql;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NSBOutboxExceptionNet5
{
    public static class HostBuilderExtensions
    {
        private static NServiceBusSettingsManager _nServiceBusSettingsManager;

        public static IHostBuilder UseInfrastructureServiceBus(this IHostBuilder builder)
        {
            return builder
                .UseNServiceBus(ctx =>
                {
                    _nServiceBusSettingsManager = ctx.Configuration.Get<NServiceBusSettingsManager>();

                    var endpointConfiguration = new EndpointConfiguration(ctx.HostingEnvironment.ApplicationName);

                    ConfigureEndpoint(endpointConfiguration);

                    return endpointConfiguration;

                });
        }

        private static void ConfigureEndpoint(EndpointConfiguration endpointConfiguration)
        {
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>().UseConventionalRoutingTopology();
            transport.ConnectionString(_nServiceBusSettingsManager.NsbSettings.RabbitMqConnectionStrings);
            transport.Transactions(TransportTransactionMode.ReceiveOnly);

            endpointConfiguration.AuditProcessedMessagesTo(_nServiceBusSettingsManager.NsbSettings.AuditQueueAddress);
            endpointConfiguration.SendFailedMessagesTo(_nServiceBusSettingsManager.NsbSettings.ErrorQueueAddress);
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.EnableInstallers();


            endpointConfiguration.UseMsSqlPersistence();


            endpointConfiguration.EnableOutbox().UsePessimisticConcurrencyControl();

            endpointConfiguration.RegisterComponents(c =>
            {
                c.ConfigureComponent(b =>
                {
                    var session = b.Build<ISqlStorageSession>();

                    var context = new MonitorContext(new DbContextOptionsBuilder<MonitorContext>()
                        .UseSqlServer(session.Connection)
                        .Options);

                    //Use the same underlying ADO.NET transaction
                    context.Database.UseTransaction(session.Transaction);

                    //Ensure context is flushed before the transaction is committed
                    session.OnSaveChanges(s => context.SaveChangesAsync());

                    return context;
                }, DependencyLifecycle.InstancePerUnitOfWork);
            });

            ConventionsBuilder conventionsBuilder = endpointConfiguration.Conventions();
            UseConventions(conventionsBuilder, _nServiceBusSettingsManager.NsbSettings.NamespacesToExcludeFromConvention);

        }


        private static void UseMsSqlPersistence(this EndpointConfiguration endpointConfiguration)
        {
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(connectionBuilder: () => new SqlConnection(_nServiceBusSettingsManager.NsbSettings.PersistenceConnectionString));
            var subscriptions = persistence.SubscriptionSettings();
            if (!int.TryParse(_nServiceBusSettingsManager.NsbSettings.SubscriptionsCache, out int timeSpan))
            {
                timeSpan = 1;
            }
            subscriptions.CacheFor(TimeSpan.FromMinutes(timeSpan));
        }



        private static void UseConventions(ConventionsBuilder conventionsBuilder, List<string> namespacesToExclue)
        {
            conventionsBuilder.DefiningCommandsAs(
                type =>
                {
                    return type.Namespace != null &&
                           type.Namespace.EndsWith(ConventionNamespaceSuffix.COMMANDS) &&
                           !type.GetInterfaces().Any(x =>
                               namespacesToExclue.Any(y => x.Namespace.StartsWith(y)));
                });
            conventionsBuilder.DefiningEventsAs(
                type =>
                {
                    return type.Namespace != null &&
                           type.Namespace.EndsWith(ConventionNamespaceSuffix.EVENTS) &&
                           !type.GetInterfaces().Any(x =>
                               namespacesToExclue.Any(y => x.Namespace.StartsWith(y)));
                });
            conventionsBuilder.DefiningMessagesAs(
                type => type.Namespace != null &&
                        type.Namespace.EndsWith(ConventionNamespaceSuffix.MESSAGES));
        }

    }
    public static class ConventionNamespaceSuffix
    {
        public const string MESSAGES = ".Messages";
        public const string EVENTS = ".Events";
        public const string COMMANDS = ".Commands";

    }
}
