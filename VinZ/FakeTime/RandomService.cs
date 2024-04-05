namespace VinZ.Random;

public class RandomService : IRandomService
{
    private System.Random random = new System.Random();

    public int Int() => random.Next();
    public long Long() => random.NextInt64();
    public (long, long) Guid()
    {
        var guid = System.Guid.NewGuid();

        return guid.ToLong2();
    }
}