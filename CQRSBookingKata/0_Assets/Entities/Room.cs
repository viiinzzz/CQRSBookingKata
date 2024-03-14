using CQRSBookingKata.Common;
using CQRSBookingKata.Sales;

namespace CQRSBookingKata.Assets;

public record Room(
    int Urid,

    int PersonMaxCount
)
    : RecordWithValidation
{
    protected override void Validate()
    {
        if (PersonMaxCount is < 0 or > 5)
        {
            throw new ArgumentException("value must be at least 0 and at most 5", nameof(PersonMaxCount));
        }
    }

    private readonly UniqueRoomId _urid = new(Urid);

    public int HotelId => _urid.HotelId;
    public int FloorNum  => _urid.FloorNum;
    public int RoomNum  => _urid.RoomNum;
 
}