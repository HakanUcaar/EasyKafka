using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EasyKafka.Constants;
using EasyKafka.Converters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace EasyKafka.Abstractions;
public class KafkaServiceBus(KafkaOption kafkaOption, ILogger<KafkaServiceBus> logger) : IKafkaServiceBus
{
    private static ProducerConfig? _producerConfig;
    private static readonly object ProducerLockObject = new();
    private static readonly Dictionary<string, ConsumerConfig> ConsumerConfigs = new();
    private static readonly object ConsumerLockObject = new();

    public IConsumer<TKey, TMessage> GetConsumer<TMessage, TKey>(string topicName, string groupId)
    {
        var consumer = new ConsumerBuilder<TKey, TMessage>(GetConsumerConfig(groupId))
            .SetValueDeserializer(new CustomValueDeserializer<TMessage>()).Build();
        consumer.Subscribe(topicName);
        return consumer;
    }


    public async Task<CustomResult> CreateTopicAsync(string topicName, int expireAsDay = 7)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = string.Join(",", kafkaOption.Host),
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = kafkaOption.Username,
            SaslPassword = kafkaOption.Password,
            SocketTimeoutMs = 120000
        }).Build();


        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var topicExists = metadata.Topics.Exists(t => t.Topic == topicName);

        var config = new Dictionary<string, string>
        {
            { "min.insync.replicas", kafkaOption.MinSyncReplicas.ToString() }
        };


        if (expireAsDay == -1)
        {
            config["retention.ms"] = "-1";
        }
        else if (expireAsDay > 0)
        {
            var retentionMs = (expireAsDay * 24 * 60 * 60 * 1000).ToString(); // gün → ms
            config["retention.ms"] = retentionMs;
        }


        if (!topicExists)
        {
            await adminClient.CreateTopicsAsync([
                new TopicSpecification
                {
                    Name = topicName, NumPartitions = kafkaOption.NumPartitions,
                    ReplicationFactor = kafkaOption.ReplicationFactor, Configs = config
                }
            ]);


            logger.LogInformation($"Topic({topicName} has created)");
        }

        return CustomResult.Success();
    }


    public async Task<DeliveryResult<TKey, TMessage>> PublishWithKeyAsync<TKey, TMessage>(string topicName, TKey key,
        TMessage message)
    {
        using var producer = new ProducerBuilder<TKey, TMessage>(GetProducerConfig())
            .SetValueSerializer(new CustomValueSerializer<TMessage>()).Build();

        var headers = new Headers { new Header(KafkaConstant.MessageId, Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())) };
        var newMessage = new Message<TKey, TMessage>
        {
            Value = message,
            Key = key,
            Headers = headers
        };
        var response = await producer.ProduceAsync(topicName, newMessage);
        return response;
    }

    public async Task<DeliveryResult<TKey, TMessage>> PublishWithKeyAsync<TKey, TMessage>(string topicName, TKey key,
        TMessage message, Dictionary<string, string> headers)
    {
        using var producer = new ProducerBuilder<TKey, TMessage>(GetProducerConfig())
            .SetValueSerializer(new CustomValueSerializer<TMessage>()).Build();

        var messageheaders = new Headers { new Header(KafkaConstant.MessageId, Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())) };


        foreach (var header in headers) messageheaders.Add(new Header(header.Key, Encoding.UTF8.GetBytes(header.Value)));


        var newMessage = new Message<TKey, TMessage>
        {
            Value = message,
            Key = key,
            Headers = messageheaders
        };
        var response = await producer.ProduceAsync(topicName, newMessage);
        return response;
    }


    public async Task<DeliveryResult<Null, TMessage>> PublishAsync<TMessage>(string topicName, TMessage message)
    {
        using var producer = new ProducerBuilder<Null, TMessage>(GetProducerConfig())
            .SetValueSerializer(new CustomValueSerializer<TMessage>()).Build();

        var headers = new Headers
        {
            new Header(KafkaConstant.MessageId, Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))
        };


        var newMessage = new Message<Null, TMessage>
        {
            Value = message,
            Headers = headers
        };

        var response = await producer.ProduceAsync(topicName, newMessage);
        return response;
    }

    public async Task<DeliveryResult<Null, TMessage>> PublishAsync<TMessage>(string topicName, TMessage message, Dictionary<string, string> headers)
    {
        using var producer = new ProducerBuilder<Null, TMessage>(GetProducerConfig())
            .SetValueSerializer(new CustomValueSerializer<TMessage>()).Build();

        var messageheaders = new Headers
        {
            new Header(KafkaConstant.MessageId, Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())),
            new Header("TraceId", Encoding.UTF8.GetBytes(Activity.Current?.Id ?? string.Empty))
        };


        foreach (var header in headers) messageheaders.Add(new Header(header.Key, Encoding.UTF8.GetBytes(header.Value)));

        var newMessage = new Message<Null, TMessage>
        {
            Value = message,
            Headers = messageheaders
        };

        return await producer.ProduceAsync(topicName, newMessage);
    }

    private ProducerConfig GetProducerConfig()
    {
        if (_producerConfig is not null)
            return _producerConfig;

        lock (ProducerLockObject)
        {
            if (_producerConfig is null)
                _producerConfig = new ProducerConfig
                {
                    BootstrapServers = string.Join(",", kafkaOption.Host),
                    SecurityProtocol = SecurityProtocol.SaslPlaintext,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = kafkaOption.Username,
                    SaslPassword = kafkaOption.Password,
                    SocketTimeoutMs = 120000, // 120 saniye
                    Acks = Acks.All
                };
        }

        return _producerConfig;
    }

    private ConsumerConfig GetConsumerConfig(string groupId)
    {
        if (ConsumerConfigs.TryGetValue(groupId, out var config))
            return config;

        lock (ConsumerLockObject)
        {
            if (!ConsumerConfigs.TryGetValue(groupId, out config))
            {
                config = new ConsumerConfig
                {
                    BootstrapServers = string.Join(",", kafkaOption.Host),
                    SecurityProtocol = SecurityProtocol.SaslPlaintext,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = kafkaOption.Username,
                    SaslPassword = kafkaOption.Password,
                    // 4. Zaman Aşımı Optimizasyonu
                    SocketTimeoutMs = 120000, // 120 saniye
                    SessionTimeoutMs = 60000, // 60 saniye
                    MaxPollIntervalMs = 300000, // 5 dakika
                    GroupId = groupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                ConsumerConfigs[groupId] = config;
            }
        }

        return config;
    }

}
