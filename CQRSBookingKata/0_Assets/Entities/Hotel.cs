using CQRSBookingKata.Common;

namespace CQRSBookingKata.Assets;

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
    }

    private int CheckInH => EarliestCheckInTime / 60;
    private int CheckInM => EarliestCheckInTime % 60;
    private int CheckOutH => LatestCheckOutTime / 60;
    private int CheckOutM => LatestCheckOutTime % 60;

    public double EarliestCheckInHours => CheckInH + CheckInM / 60d;
    public double LatestCheckOutHours => CheckOutH + CheckOutM / 60d;
}
