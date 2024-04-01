namespace VinZ.MessageQueue;

public record struct DeliveryCount(int Delivered = 0, int Failed = 0)
{
    public int Total => Delivered + Failed;

    public static DeliveryCount operator +(DeliveryCount a, DeliveryCount b)

        => new(a.Delivered + b.Delivered, a.Failed + b.Failed);
}