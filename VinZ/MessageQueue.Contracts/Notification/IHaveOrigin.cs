namespace VinZ.MessageQueue;

public interface IHaveOrigin
{
    string? Originator { get; }
}