using System.ComponentModel;

namespace EasyKafka.Constants;
public enum ConsumerState
{
    [Description("The service is starting.")]
    Starting,

    [Description("The service is currently running.")]
    Running,

    [Description("The service is stopping.")]
    Stopping,

    [Description("The service has stopped or has not started yet.")]
    Stopped,

    [Description("The service has encountered an error and failed.")]
    Failed
}
