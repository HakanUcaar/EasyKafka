namespace EasyKafka.Abstractions;

public interface IKafkaConsumer<TMessage> : IKafkaConsumer where TMessage : class
{
    public Task Consume(IKafkaConsumerContext<TMessage> context, CancellationToken cancellationToken);
}

public interface IKafkaConsumer
{
    public abstract string Topic { get; }
    public abstract string GroupId { get; }
}

