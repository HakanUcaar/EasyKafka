using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EasyKafka.Abstractions;

public interface IKafkaConsumerManager
{
    Task Add(IKafkaHostService consumer);
    Task Remove(IKafkaHostService consumer);
    Task<HealthCheckResult> CheckHealth();
    List<IKafkaHostService> Consumers { get; }
}
