using Google.Common.Geometry;

namespace CQRSBookingKata.Admin;

public record HotelCell(long S2CellId, byte S2Level, int HotelId, int HotelCellId = 0)
{
    public S2CellId Id { get; } = new CellId(S2CellId, S2Level).S2;
}