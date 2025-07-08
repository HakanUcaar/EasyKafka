using EasyKafka.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EasyKafka;

public static class KafkaRegistrationExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection collection, Action<IKafkaRegistrationConfigurator, IKafkaOption> configure = null)
    {
        var configurator = new KafkaRegistrationConfigurator(collection);
        var option = new KafkaOption();
        configure?.Invoke(configurator, option);
        configurator.Build();

        return collection;
    }
}
