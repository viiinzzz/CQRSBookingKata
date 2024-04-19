namespace BookingKata.Sales;

public partial class SalesQueryService
{
    public long[] CloseBooking(UniqueRoomId urid, DateTime openingDate, DateTime closingDate)
    {
        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var vacancyIds = firstNight.VacancyIdsUntil(lastNight, urid.Value);

        //
        //
        sales.RemoveVacancies(vacancyIds);
        //
        //

        return vacancyIds;
    }
}