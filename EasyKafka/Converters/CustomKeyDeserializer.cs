using Confluent.Kafka;
using System.Text.Json;

namespace EasyKafka.Converters;

public class CustomKeyDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<T>(data)!;
    }
}