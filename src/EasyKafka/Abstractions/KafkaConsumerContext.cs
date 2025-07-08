using Confluent.Kafka;

namespace EasyKafka.Abstractions;

public class KafkaConsumerContext<TMessage> : IKafkaConsumerContext<TMessage> where TMessage : class
{
    private TMessage _message;
    public TMessage Message => _message;
    public Headers Headers { get; }
    public string Topic { get; }
    public int Partition { get; }
    public long Offset { get; }
    public long PartitionOffset { get; }
    public string GroupId { get; }
    public DateTime MessageTimestamp { get; }
    public CancellationToken CancellationToken { get; }

    public KafkaConsumerContext(TMessage message, Headers headers, string topic , int partition, long offset, long partitionOffset, string groupId, DateTime messageTimestamp, CancellationToken cancellationToken = default)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
        Headers = headers ?? new Headers();
        CancellationToken = cancellationToken;
        Topic = topic;
        Partition = partition;
        Offset = offset;
        GroupId = groupId;
        MessageTimestamp = messageTimestamp;
        PartitionOffset = partitionOffset;
    }   

    public IKafkaConsumerContext<TMessage> SetMessage(object key, object value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));
        if (value is not TMessage) throw new InvalidCastException($"The value must be of type {typeof(TMessage).Name}, but it is of type {value?.GetType().Name ?? "null"}.");

        if (value is TMessage message)
        {
            _message = message;
        }
        return this;
    }
}
