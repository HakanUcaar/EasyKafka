using Confluent.Kafka;

namespace EasyKafka.Abstractions;

public interface IKafkaConsumerContext
{
    Headers Headers { get; }
    string Topic { get; }
    int Partition { get; }
    long Offset { get; }
    long PartitionOffset { get; }
    string GroupId { get; }
    DateTime MessageTimestamp { get; }
    CancellationToken CancellationToken { get; }
}

public interface IKafkaConsumerContext<out T> : IKafkaConsumerContext where T : class
{
    IKafkaConsumerContext<T> SetMessage(object key, object value);
    T Message { get; }
}
