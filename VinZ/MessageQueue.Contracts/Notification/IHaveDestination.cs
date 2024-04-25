namespace VinZ.MessageQueue;

public interface IHaveDestination
{
    string? Recipient { get; }
}