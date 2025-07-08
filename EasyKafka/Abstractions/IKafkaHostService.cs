using EasyKafka.Constants;

namespace EasyKafka.Abstractions;

public interface IKafkaHostService
{
    //ConsumerState Status { get; }
    //Type ConsumerType { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
