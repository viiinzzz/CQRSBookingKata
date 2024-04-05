namespace VinZ.Random;

public interface IRandomService
{
    int Int();
    long Long();
    (long, long) Guid();
}