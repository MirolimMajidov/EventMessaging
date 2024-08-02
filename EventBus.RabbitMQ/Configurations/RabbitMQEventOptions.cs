namespace EventBus.RabbitMQ.Configurations;

public class RabbitMQEventOptions : BaseEventOptions
{
    /// <summary>
    /// Clone/Copying settings
    /// </summary>
    /// <returns>Returns a new copy of settings</returns>
    internal RabbitMQEventOptions Clone()
    {
        var options = new RabbitMQEventOptions();
        options.OverwriteSettings(this);

        return options;
    }
}