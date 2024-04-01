namespace VinZ.MessageQueue;

public interface IMessageQueueServer : IMessageBus
{
    public int BusRefreshSeconds { get; set; }
}