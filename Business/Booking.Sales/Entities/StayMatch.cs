namespace BookingKata.Sales;

public record StayMatch(
    int PersonCount,

    double Price,
    string Currency,

    DateTime ArrivalDate,
    DateTime DepartureDate,

    int Urid
)
{
    public override int GetHashCode()
    {
        return (ArrivalDate.DayStart().Ticks, DepartureDate.DayStart().Ticks, Urid).GetHashCode();
    }

    public virtual bool Equals(StayMatch? other)
    {
        if (other == null)
        {
            return false;
        }

        return other.GetHashCode() == GetHashCode();
    }
}