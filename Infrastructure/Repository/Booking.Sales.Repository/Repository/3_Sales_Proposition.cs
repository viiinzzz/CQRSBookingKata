using Business.Common;

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

    public bool HasActiveProposition(DateTime now, int urid, DateTime arrivalDate, DateTime departureDate)
    {
        var nowDayNum = OvernightStay.From(now).DayNum;
        var arrivalDayNum = OvernightStay.From(arrivalDate).DayNum;
        var departureDayNum = OvernightStay.From(departureDate).DayNum;

        return _sales.Propositions
            .AsNoTracking()
            .Any(prop =>
                prop.Urid == urid &&

                nowDayNum >= prop.OptionStartDayNum &&
                nowDayNum < prop.OptionEndDayNum &&
                (
                    (prop.ArrivalDayNum >= arrivalDayNum && prop.DepartureDayNum <= departureDayNum) ||
                    (prop.ArrivalDayNum <= arrivalDayNum && prop.DepartureDayNum >= departureDayNum) ||
                    (prop.ArrivalDayNum <= arrivalDayNum && prop.DepartureDayNum >= arrivalDayNum) ||
                    (prop.ArrivalDayNum <= departureDayNum && prop.DepartureDayNum >= departureDayNum)
                )
            );
    }
}