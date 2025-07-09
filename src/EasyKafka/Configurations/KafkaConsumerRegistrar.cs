using EasyKafka.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyKafka;


public static class KafkaConsumerRegistrar
{
    public static void Register(IServiceCollection collection)
    {
        var consumerDescriptors = collection
            .Where(descriptor =>
                descriptor.ServiceType.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() == typeof(IKafkaConsumer<>)))
            .ToList();

        foreach (var descriptor in consumerDescriptors)
        {
            var consumerType = descriptor.ImplementationType ?? descriptor.ServiceType;

            if (consumerType != null)
            {
                var kafkaConsumerInterface = consumerType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IKafkaConsumer<>));

                if (kafkaConsumerInterface != null)
                {
                    var messageType = kafkaConsumerInterface.GetGenericArguments()[0];
                    var hostedServiceType = typeof(KafkaHostService<>).MakeGenericType(messageType);

                    collection.AddSingleton(typeof(IHostedService), provider =>
                    {
                        var consumer = Activator.CreateInstance(hostedServiceType, provider, consumerType);

                        if (consumer is null) throw new NullReferenceException("The provided consumer instance is null. Make sure to initialize the consumer before adding it.");
                        return consumer;
                    });
                }
            }
        }
    }
}