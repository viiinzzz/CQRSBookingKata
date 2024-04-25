namespace VinZ.MessageQueue;

public interface IHaveSerializedMessage
{
    string? Verb { get; }
    string? MessageType { get; }
    string? Message { get; }
}