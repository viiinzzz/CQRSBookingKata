namespace BookingKata.Sales;

public class OvernightStay
{
    private const int YearStart = 2024;
    private const int Evening = 18;
    private const int Morning = 8;

    private static readonly DateTime BaseCheckInDate = new (
        YearStart, 1, 1, 
        Evening, 0, 0);

    private static readonly DateTime BaseCheckOutDate = new(
        YearStart, 1, 2,
        Morning, 0, 0);

    public int DayNum { get; }

    public DateTime CheckInDate => BaseCheckInDate.AddDays(DayNum);

    public DateTime CheckOutDate => BaseCheckOutDate.AddDays(DayNum);

    public OvernightStay(int dayNum)
    {
        DayNum = dayNum;
    }

    public static OvernightStay From(DateTime checkInDate)
    {
        var evening = new DateTime(
            checkInDate.Year, checkInDate.Month, checkInDate.Day,
            Evening, 0, 0);

        var dayNum = (int)(evening - BaseCheckInDate).TotalDays;

        return new OvernightStay(dayNum);
    }

    public static OvernightStay FromCheckOutDate(DateTime checkOutDate)
    {
        var morning = new DateTime(
            checkOutDate.Year, checkOutDate.Month, checkOutDate.Day,
            Morning, 0, 0);

        var dayNum = (int)(morning - BaseCheckInDate).TotalDays;

        return new OvernightStay(dayNum - 1);
    }

    public int To (OvernightStay lastNight)
    {
        var firstNight = this;

        return lastNight.DayNum - firstNight.DayNum + 1;
    }

    public int To(DateTime checkOutDate) => To(OvernightStay.FromCheckOutDate(checkOutDate));

    public IEnumerable<Vacancy> StayUntil(OvernightStay lastNight, int personMaxCount,
        double latitude, double longitude,
        string hotelName, string cityName, int urid)
    {
        var firstNight = this;

        return Enumerable.Range(firstNight.DayNum, lastNight.DayNum)
            
            .Select(dayNum => new Vacancy(DayNum, personMaxCount, latitude, longitude,
                hotelName, cityName, false, urid));
    }

    public long[] StayUntil(OvernightStay lastStay, int urid) 
        
        => StayUntil(lastStay, 0, 
                0, 0,
                string.Empty, string.Empty, urid)
            .Select(vacancy => vacancy.VacancyId)
            .ToArray();
}