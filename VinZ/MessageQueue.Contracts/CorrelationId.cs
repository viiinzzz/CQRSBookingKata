namespace VinZ.MessageQueue;

public record struct CorrelationId(long Id1, long Id2) : ICorrelationId
{
    public static CorrelationId New()
    {
        return From(System.Guid.NewGuid());
    }

    public static CorrelationId From(Guid guid)
    {
        var bytes = guid.ToByteArray();

        long id1 = 0;
        long id2 = 0;

        for (var i = 0; i < 8; i++)
        {
            id1 |= (long)bytes[i] << (8 * i);
        }

        for (var i = 8; i < 16; i++)
        {
            id2 |= (long)bytes[i] << (8 * i);
        }

        return new CorrelationId(id1, id2);
    }

    public string Guid => ToGuid().ToString("B");

    public override string ToString() => Guid;

    public Guid ToGuid()
    {
        return new Guid(
            (int)Id1, (short)(Id1 >> 32), (short)(Id1 >> 48),
            (byte)Id2, (byte)(Id2 >> 8), (byte)(Id2 >> 16), (byte)(Id2 >> 24),
            (byte)(Id2 >> 32), (byte)(Id2 >> 40), (byte)(Id2 >> 48), (byte)(Id2 >> 56)
        );
    }
}
