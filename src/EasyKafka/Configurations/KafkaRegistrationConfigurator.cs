using EasyKafka.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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

    internal void Build()
    {
        _services.TryAddSingleton<IKafkaServiceBus, KafkaServiceBus>();
        _services.AddOptions<KafkaOption>().BindConfiguration("KafkaOption").ValidateDataAnnotations().ValidateOnStart();
        _services.AddSingleton((IServiceProvider sp) => sp.GetRequiredService<IOptions<KafkaOption>>().Value);

        KafkaConsumerRegistrar.Register(_services);
    }
}