using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace EasyKafka.Converters;

public class CustomValueSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, typeof(T)));
    }
}