# EasyKafka
confluent-kafka-dotnet wrapper


## Event Tanımı
``` csharp
namespace KafkaLibTest.Consumers;

public class TestTaskEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Value { get; set; }
}
```

## Consumer Tanımı
``` csharp
using EasyKafka.Abstractions;

namespace KafkaLibTest.Consumers;

public sealed class TestConsumer(ILogger<TestConsumer> logger) : IKafkaConsumer<TestTaskEvent>
{
    public string Topic => "TestTopic";
    public string GroupId => "TestGroupId";

    public async Task Consume(IKafkaConsumerContext<TestTaskEvent> context, CancellationToken cancellationToken)
    {
        var testTaskEvent = context.Message;
        logger.LogInformation($"Received message on topic {context.Topic} with value {testTaskEvent.Value}");
    }
}
```

## DI ServiceCollectiona Ekleme
``` csharp
builder.Services.AddKafka((conf) =>
{
    conf.AddConsumer<TestConsumer>();
});
```

## Publish 
``` csharp
using EasyKafka.Abstractions;
using KafkaLibTest.Consumers;
using Microsoft.AspNetCore.Mvc;

namespace KafkaLibTest.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsumerTestController(IKafkaServiceBus kafkaServiceBus) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> TestMessage()
    {
        await kafkaServiceBus.PublishAsync("TestTopic", new TestTaskEvent
        {
            Value = "Test message from ConsumerController",
        });

        return Ok();
    }
}
```

