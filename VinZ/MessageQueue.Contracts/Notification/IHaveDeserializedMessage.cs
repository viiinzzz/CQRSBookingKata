namespace VinZ.MessageQueue;

public interface IHaveDeserializedMessage
{
    string? Verb { get; }
    object? MessageObj { get; }
}