namespace CQRSBookingKata.Admin;

public record Hotel(
    string HotelName,

    double Latitude,
    double Longitude,

    int EarliestCheckInTime = 16_00,
    int LatestCheckOutTime = 10_00,

    string LocationAddress = "",
    string ReceptionPhoneNumber = "",
    string url = "",
    int ranking = 2,

    int? ManagerId = default,

    int HotelId = 0,
    bool Disabled = false
) 
    : RecordWithValidation
{
    protected override void Validate()
    {
        if (CheckInH is < 0 or > 23)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(EarliestCheckInTime));
        }
        if (CheckOutH is < 0 or > 23)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(LatestCheckOutTime));
        }
        if (CheckInM is < 0 or > 59)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(EarliestCheckInTime));
        }
        if (CheckOutM is < 0 or > 59)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(LatestCheckOutTime));
        }

        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);

    }

    public Position? Position;

    private int CheckInH => EarliestCheckInTime / 100;
    private int CheckInM => EarliestCheckInTime % 100;
    private int CheckOutH => LatestCheckOutTime / 100;
    private int CheckOutM => LatestCheckOutTime % 100;

    public double EarliestCheckInHours => CheckInH + CheckInM / 60d;
    public double LatestCheckOutHours => CheckOutH + CheckOutM / 60d;
}
