namespace VinZ.MessageQueue;

public interface IHaveDeserializedMessage : IHaveMessageObj
{
    string? Verb { get; }
    object? MessageObj { get; }
}