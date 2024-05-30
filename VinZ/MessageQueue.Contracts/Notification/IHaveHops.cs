namespace VinZ.MessageQueue;

public interface IHaveHops
{
    int _hops { get; }
    string[] _steps { get; }
}