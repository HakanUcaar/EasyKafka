using EasyKafka.Abstractions;

namespace EasyKafka;

public class KafkaOption : IKafkaOption
{
    public string[] Host { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int TimeoutAsMs { get; set; }

    public short NumPartitions { get; set; }

    public short ReplicationFactor { get; set; }

    public short MinSyncReplicas { get; set; }

    public string? GroupId { get; set; }

    public int ConsumeItemCount { get; set; }
}