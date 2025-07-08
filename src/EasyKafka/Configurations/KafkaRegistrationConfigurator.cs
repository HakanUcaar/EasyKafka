using EasyKafka.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyKafka;

public class KafkaRegistrationConfigurator : IKafkaRegistrationConfigurator
{
    private readonly IServiceCollection _services;

    public KafkaRegistrationConfigurator(IServiceCollection services)
    {
        _services = services;
    }

    public IKafkaRegistrationConfigurator AddConsumer<T>() where T : class, IKafkaConsumer
    {
        _services!.TryAddScoped<T>();
        return this;
    }

    public IKafkaRegistrationConfigurator AddConsumer<T>(KafkaConsumerOption option) where T : class, IKafkaConsumer
    {
        _services!.TryAddScoped<T>();
        return this;
    }

    public void Build()
    {
        KafkaConsumerRegistrar.Register(_services);
    }
}