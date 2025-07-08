namespace EasyKafka;

public class KafkaOption
{
    public required string Host { get; set; }
    public required string Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required int TimeoutAsMs { get; set; }

    public required short NumPartitions { get; set; }

    public required short ReplicationFactor { get; set; }

    public required short MinSyncReplicas { get; set; }

    public required string GroupId { get; set; }

    public required int ConsumeItemCount { get; set; }
    public Dictionary<string, string>? Queues { get; set; }
}