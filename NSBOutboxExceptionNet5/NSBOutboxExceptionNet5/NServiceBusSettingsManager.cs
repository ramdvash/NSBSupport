using System.Collections.Generic;

namespace NSBOutboxExceptionNet5
{
    public class NServiceBusSettingsManager
    {
        public NServiceBusSettings NsbSettings { get; set; }
    }

    public class NServiceBusSettings
    {
        public Recoverability Recoverability { get; set; }
        public string License { get; set; }
        public string RabbitMqConnectionStrings { get; set; }
        public string ErrorQueueAddress { get; set; }
        public bool IsAuditQueueActive { get; set; }
        public bool IsOutBoxEnable { get; set; } = true;
        public string AuditQueueAddress { get; set; }
        public HeartBeat HeartBeat { get; set; }
        public Metrics Metrics { get; set; }
        public BridgeConfig BridgeConfig { get; set; }
        public string PersistenceConnectionString { get; set; }
        public string SubscriptionsCache { get; set; }
        public List<string> NamespacesToExcludeFromConvention { get; set; }

    }

    public class BridgeConfig
    {
        public bool UseBridge { get; set; }
        public string BridgeEndpointName { get; set; }
        public string BridgeSendEndpointName { get; set; }
    }

    public class Recoverability
    {
        public int ImmediateRetries { get; set; }
        public int DelayRetries { get; set; }
        public int DelayTimeIncrease { get; set; }
    }

    public class HeartBeat
    {
        public bool UseHeartBeat { get; set; }
        public string ServiceControlQueue { get; set; }
        public int Frequency { get; set; }
        public int TimeToLive { get; set; }
    }

    public class Metrics
    {
        public bool UseMetrics { get; set; }
        public string ServiceControlMetricsAddress { get; set; }
        public int Interval { get; set; }
    }
}
