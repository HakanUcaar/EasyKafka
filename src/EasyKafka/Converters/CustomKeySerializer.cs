﻿using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace EasyKafka.Converters;

public class CustomKeySerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, typeof(T)));
    }
}