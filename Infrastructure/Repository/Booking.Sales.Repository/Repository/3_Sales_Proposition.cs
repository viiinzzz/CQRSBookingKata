namespace BookingKata.Infrastructure.Storage;

public partial class SalesRepository
{
    public IQueryable<StayProposition> Propositions

        => _sales.Propositions
            .AsNoTracking();


    public void AddStayProposition(StayProposition proposition)
    {
        var entity = _sales.Propositions.Add(proposition);
        _sales.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public bool HasActiveProposition(DateTime now, int urid, DateTime arrival, DateTime departure)
    {
        return _sales.Propositions
            .AsNoTracking()
            .Any(prop =>
                // prop.IsValid(now) &&
                now >= prop.OptionStartsUtc && now < prop.OptionEndsUtc &&

                prop.Urid == urid &&

                (
                    (prop.ArrivalDate >= arrival && prop.DepartureDate <= departure) ||
                    (prop.ArrivalDate <= arrival && prop.DepartureDate >= departure) ||
                    (prop.ArrivalDate <= arrival && prop.DepartureDate >= arrival) ||
                    (prop.ArrivalDate <= departure && prop.DepartureDate >= departure)
                ));
    }
}