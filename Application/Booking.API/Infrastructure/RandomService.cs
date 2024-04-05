namespace BookingKata.API.Infrastructure;

public class RandomService : IRandomService
{
    public int Int() => RandomHelper.Int();
    public long Long() => RandomHelper.Long();

    public (long, long) Guid()
    {
        var guid = System.Guid.NewGuid();

        return guid.ToLong2();
    }
}