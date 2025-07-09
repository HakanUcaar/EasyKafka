using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyKafka.Abstractions;

public class KafkaHostService<TMessage>(IServiceProvider serviceProvider, Type consumerType) : IKafkaHostService where TMessage : class
{
    private IConsumer<string, TMessage>? _consumer;
    private IKafkaConsumer<TMessage>? _virtualConsumer;
    private IKafkaServiceBus? _kafkaServiceBus;
    private ILogger<IKafkaHostService>? _logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(()=> Consume(cancellationToken)).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _virtualConsumer = null;
        _consumer?.Close();
        _consumer = null;

        return Task.CompletedTask;
    }

    private async Task Consume(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        _logger = scope.ServiceProvider.GetRequiredService<ILogger<IKafkaHostService>>();
        _kafkaServiceBus = scope.ServiceProvider.GetRequiredService<IKafkaServiceBus>();
        _virtualConsumer ??= (IKafkaConsumer<TMessage>)scope.ServiceProvider.GetRequiredService(consumerType);
        await KafkaTopicNameRegistrar.Register(_kafkaServiceBus!, _virtualConsumer);
        _consumer ??= _kafkaServiceBus?.GetConsumer<TMessage, string>(_virtualConsumer.Topic, _virtualConsumer.GroupId);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                this.EnsureConsumer();

                var consumeResult = _consumer!.Consume(3000);

                if (consumeResult is not null)
                {
                    await ConsumeMessage(consumeResult, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.Local_MaxPollExceeded)
            {
                _logger.LogWarning($"Max Poll Interval Exceeded {nameof(consumerType)}");
            }
            catch (KafkaException ex) when (ex.Error.IsFatal)
            {
                _logger.LogError(ex, "Kafka Consumer fatal error occurred. Recreating consumer in 5 seconds");

                await StopAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TopicName={topicName}, Message={@message}", _virtualConsumer.Topic, ex.Message);
            }
        }
    }

    private async Task ConsumeMessage<TKey>(ConsumeResult<TKey, TMessage> consumeResult, CancellationToken cancellationToken)
    {
        if (consumeResult is null)
        {
            return;
        }

        try
        {
            var context = new KafkaConsumerContext<TMessage>(
                consumeResult.Message.Value,
                consumeResult.Message.Headers,
                consumeResult.Topic,
                consumeResult.Partition,
                consumeResult.Offset,
                consumeResult.TopicPartitionOffset.Offset,
                _virtualConsumer!.GroupId,
                consumeResult.Message.Timestamp.UtcDateTime,
                cancellationToken);

            var finalConsumDelegate = new KafkaMessagePipelineDelegate<IKafkaConsumerContext<TMessage>>(async (context, cancellationToken) =>
            {
                await _virtualConsumer.Consume(context, cancellationToken);
            });

            var pipeline = serviceProvider
                .GetServices<IKafkaMessagePipeline<IKafkaConsumerContext<TMessage>>>()
                .Reverse()
                .Aggregate(
                    finalConsumDelegate,
                    (next, pipeline) => async (context, cancellationToken) =>
                    {
                        try
                        {
                            await pipeline.InvokeAsync(context, next, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            HandleException(consumeResult, ex).Wait(cancellationToken);
                        }
                    });

            await pipeline(context, cancellationToken);
        }
        catch (Exception ex)
        {
            HandleException(consumeResult, ex).Wait(cancellationToken);
        }
    }

    private Task HandleException<TKey>(ConsumeResult<TKey, TMessage> consumeResult, Exception ex)
    {
        _logger!.LogError(ex, "Kafka Consumer Error, TopicName={topicName}, Message={@message}", _virtualConsumer!.Topic, consumeResult.Message);
        return Task.CompletedTask;
    }

    private void EnsureConsumer()
    {
        if (_consumer != null)
        {
            return;
        }

        _consumer ??= _kafkaServiceBus?.GetConsumer<TMessage, string>(_virtualConsumer!.Topic, _virtualConsumer.GroupId);
    }
}