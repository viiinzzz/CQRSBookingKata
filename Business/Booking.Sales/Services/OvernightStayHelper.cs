using BookingKata.Sales;

namespace Booking.Sales.Services;

public static class OvernightStayHelper
{

    public static IEnumerable<Vacancy> StayUntil(this OvernightStay firstNight,
        OvernightStay lastNight, int personMaxCount,
        double latitude, double longitude,
        string hotelName, string cityName, int urid)
    {

        return firstNight.DayNum.RangeTo(lastNight.DayNum)

            .Select(dayNum => new Vacancy(dayNum, personMaxCount, latitude, longitude,
                hotelName, cityName, false, urid));
    }

    public static long[] StayUntil(this OvernightStay firstNight,
        OvernightStay lastStay, int urid)
    {
        return firstNight.StayUntil(lastStay, 0,
                0, 0,
                string.Empty, string.Empty, urid)
            .Select(vacancy => vacancy.VacancyId)
            .ToArray();
    }
}