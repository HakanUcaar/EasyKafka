using EasyKafka.Abstractions;
using System.Reflection;

namespace EasyKafka;

public static class KafkaTopicNameRegistrar
{
    public static async Task Register(IKafkaServiceBus kafkaServiceBus, object consumer)
    {
        // Tüketici tipinden topic bilgisi çekmek için refleksiyon
        var topicProperty = consumer.GetType().GetProperty("Topic", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (topicProperty == null) throw new InvalidOperationException($"Consumer type {consumer.GetType().Name} must define a 'TopicName' property.");

        // Topic adını al
        var topic = (string)topicProperty.GetValue(consumer)!;
        if (string.IsNullOrEmpty(topic)) throw new InvalidOperationException($"Topic property of {consumer.GetType().Name} cannot be null or empty.");

        // Topic oluştur
        var result = await kafkaServiceBus.CreateTopicAsync(topic);
        if (!result)
        {
            throw new InvalidOperationException($"Topic {topic} creation failed");
        }
    }
}
