namespace CQRSBookingKata.Sales;

// HHHHRRRR

public class UniqueRoomId(int Urid)
{
    public int Value => Urid;

    public UniqueRoomId(int hotelId, int floorNum, int roomNum) 
        : this(
            hotelId * 1_0000 +
            roomNum
            )
    {
        // if (hotelId is <= 0 or >= 1_0000)
        // {
        //     throw new ArgumentException("value must be at least 1 and at most 9999", nameof(hotelId));
        // }
        //
        // if (roomNum is <= 0 or >= 1_0000)
        // {
        //     throw new ArgumentException("value must be at least 1 and at most 9999", nameof(roomNum));
        // }

        var floorNum2 = RoomNum / 1_00;

        if (floorNum2 != floorNum)
        {
            throw new ArgumentException("floorNum={floorNum} value must be consistent with roomNum floorNum={floorNum2}", nameof(floorNum));
        }
    }

    private readonly bool Valid = Validate(Urid);

    private static bool Validate(int urid)
    {

        if (urid is <= 0 or 1_0000 or >= 1_0000_0000)
        {
            throw new ArgumentException("urid={urid} value must be at least 1 and at most 9999 9999, and not 1 0000", nameof(Urid));
        }

        return true;
    }


    public int FloorNum { get; } = (Urid % 1_0000) % 1_00;

    public int RoomNum { get; } = Urid % 1_0000;

    public int HotelId { get; } = Urid / 1_0000;
}