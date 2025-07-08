using Confluent.Kafka;

namespace EasyKafka.Abstractions;

public interface IKafkaServiceBus
{
    IConsumer<TKey, TMessage> GetConsumer<TMessage, TKey>(string topicName, string groupId);
    Task<bool> CreateTopicAsync(string topicName);

    Task<DeliveryResult<TKey, TMessage>> PublishWithKeyAsync<TKey,TMessage>(string topicName, TKey key,TMessage message);
    Task<DeliveryResult<TKey, TMessage>> PublishWithKeyAsync<TKey,TMessage>(string topicName, TKey key, TMessage message, Dictionary<string, string> headers);

    Task<DeliveryResult<Null, TMessage>> PublishAsync<TMessage>(string topicName, TMessage message);
    Task<DeliveryResult<Null, TMessage>> PublishAsync<TMessage>(string topicName, TMessage message, Dictionary<string, string> headers);
}