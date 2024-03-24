using System.Diagnostics;
using Google.Common.Geometry;

namespace VinZ.ToolBox;


public record struct CellId(long Id, byte Level = 0xff)
{
    public CellId(S2CellId cid) : this
    (
        NumberHelper.MapUnsignedToSigned<ulong, long>(cid.Id),
        (byte)cid.Level
    ) {}

    public S2CellId S2 => new S2CellId(Id.MapSignedToUnsigned<long, ulong>());
}

public record Cells12
(
    long? Level12,
    long? Level11,
    long? Level10,
    long? Level9,
    long? Level8,
    long? Level7,
    long? Level6,
    long? Level5,
    long? Level4,
    long? Level3,
    long? Level2,
    long? Level1,
    long? Level0
);

public static partial class PositionHelper
{
    public static CellId CellId(this Position p)
    {
        var l = S2LatLng.FromDegrees(p.Latitude, p.Longitude);
        var c = new S2Cell(l);
        var cid = c.Id;

        return new CellId(cid);
    }

    public static CellId[] CellIds(this Position p)
        => GetCellIdsEnumerable(p).ToArray();

    public static Cells12 CellIdsLevel12(this Position p, int kmMax)
    {
        var cellIds = p.CellIds();

        return new Cells12(
            EarthKmMax(12) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 12).Id,
            EarthKmMax(11) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 11).Id,
            EarthKmMax(10) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 10).Id,
            EarthKmMax(9) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 9).Id,
            EarthKmMax(8) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 8).Id,
            EarthKmMax(7) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 7).Id,
            EarthKmMax(6) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 6).Id,
            EarthKmMax(5) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 5).Id,
            EarthKmMax(4) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 4).Id,
            EarthKmMax(3) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 3).Id,
            EarthKmMax(2) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 2).Id,
            EarthKmMax(1) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 1).Id,
            EarthKmMax(0) > kmMax ? null : cellIds.FirstOrDefault(c => c.Level == 0).Id
        );
    }



//http://s2geometry.io/resources/s2cell_statistics
    private const byte S2MaxLevel = 12;

    private static IEnumerable<CellId> GetCellIdsEnumerable(Position p)
    {
        var l = S2LatLng.FromDegrees(p.Latitude, p.Longitude);
        var c = new S2Cell(l);
        var cid = c.Id;

        var id = new CellId(cid);
        if (id.Level <= S2MaxLevel) yield return id;


        while (id.Level > 0)
        {
            cid = cid.Parent;

            id = new CellId(cid);
            if (id.Level <= S2MaxLevel) yield return id;
        }
    }

    public static S2CellId ToCellId(this long cellId)
    {
        var id = cellId.MapSignedToUnsigned<long, ulong>();

        return new S2CellId(id);
    }

    public static string ToS2Hex(this S2CellId cellId)
    {
        return $"s2:{cellId.Id:x16}";
    }

    public static double EarthKm(this long cellId1, long cellId2)
    {
        var cid1 = new CellId(cellId1).S2;
        var cid2 = new CellId(cellId2).S2;

        var a = new S1Angle(cid1.ToPoint(), cid2.ToPoint());
        var km = a.Radians * S2LatLng.EarthRadiusMeters / 1000;

        return km;
    }

    public static double EarthKmMax(this byte s2Level)
    {
        return s2Level switch
        {
            00 => 7842,
            01 => 5000,
            02 => 2500,
            03 => 1310,
            04 =>  636,
            05 =>  315,
            06 =>  156,
            07 =>   78,
            08 =>   40,
            09 =>   20,
            10 =>   10,
            11 =>    5,
            12 =>    2,
            _  =>    1
        };
    }
}