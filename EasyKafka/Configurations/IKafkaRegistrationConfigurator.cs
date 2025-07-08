using EasyKafka.Abstractions;

namespace EasyKafka;

public interface IKafkaRegistrationConfigurator
{
    IKafkaRegistrationConfigurator AddConsumer<T>() where T : class, IKafkaConsumer;
    void Build();
}
