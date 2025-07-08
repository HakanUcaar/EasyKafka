namespace EasyKafka.Abstractions;

public delegate Task KafkaMessagePipelineDelegate<TContext>(
    TContext context,
    CancellationToken cancellationToken = default
) where TContext : IKafkaConsumerContext<object>;

public interface IKafkaMessagePipeline<TContext> where TContext : IKafkaConsumerContext<object>
{
    Task InvokeAsync(TContext context, KafkaMessagePipelineDelegate<TContext> next, CancellationToken cancellationToken);
}
