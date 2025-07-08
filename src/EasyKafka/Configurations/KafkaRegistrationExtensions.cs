using Microsoft.Extensions.DependencyInjection;

namespace EasyKafka;

public static class KafkaRegistrationExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection collection, Action<IKafkaRegistrationConfigurator> configure = null)
    {
        var configurator = new KafkaRegistrationConfigurator(collection);
        configure?.Invoke(configurator);
        configurator.Build();

        return collection;
    }
}
