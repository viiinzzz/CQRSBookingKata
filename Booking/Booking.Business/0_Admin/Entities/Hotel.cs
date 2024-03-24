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

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public IList<HotelCell> Cells { get; }

    public string CellsArray 
        => string.Join(", ", Cells.Select(c => $"{c.S2CellId:x16}".Substring(0, 8)));

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Cells12? Cells12 => new (
        Cells.FirstOrDefault(c => c.S2Level == 12)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 11)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 10)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 9)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 8)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 7)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 6)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 5)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 4)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 3)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 2)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 1)?.S2CellId,
        Cells.FirstOrDefault(c => c.S2Level == 0)?.S2CellId);
}