namespace CQRSBookingKata.Assets;

public record Room(
    int Urid,

    int HotelId,
    int RoomNum,
    int FloorNum,

    int PersonMaxCount
)
    : RecordWithValidation
{
    protected override void Validate()
    {
        var urid = new UniqueRoomId(Urid);

        if (urid.HotelId != HotelId || urid.RoomNum != RoomNum || urid.FloorNum != FloorNum)
        {
            throw new ArgumentException("value is not consistent", nameof(Urid));
        }

        if (PersonMaxCount is < 0 or > 5)
        {
            throw new ArgumentException("value must be at least 0 and at most 5", nameof(PersonMaxCount));
        }
    }
}