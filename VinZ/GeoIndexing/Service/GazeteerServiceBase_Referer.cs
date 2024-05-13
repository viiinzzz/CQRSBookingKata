using System;

namespace VinZ.GeoIndexing;

public abstract partial class GazetteerServiceBase
{
    public TReferer IncludeGeoIndex<TReferer>(TReferer referer, byte maxLevel)
        where TReferer : IHavePrimaryKeyAndPosition
    {
        //
        //
        referer.Cells = RefererAllGeoIndex(referer)
            .Where(cell => cell.S2Level <= maxLevel)
            .ToList();
        //
        //

        referer.geoIndex = ToGeoIndexString(referer.Cells);

        return referer;
    }

    private static readonly int[] S2Levels = Enumerable
        .Range(S2GeometryHelper.S2MinLevel, S2GeometryHelper.S2MaxLevel - S2GeometryHelper.S2MinLevel + 1)
        .Reverse()
        .ToArray();

    private static readonly Regex EndingZeroRx = new (@"0+$");



    public static string ToGeoIndexString(IList<IGeoIndexCell> cells)
    {
        var previousLevel = byte.MaxValue;

        foreach (var cell in cells)
        {
            if (cell.S2Level >= previousLevel)
            {
                throw new ArgumentException("must be ordered by descending S2Level", nameof(cells));
            }

            previousLevel = cell.S2Level;
        }

        var cellsStr = S2Levels
            .Select(level => cells.FirstOrDefault(cell => cell.S2Level == level))
            .Select(c => c == default ? "" : $"{c.S2CellIdSigned:x16}")
            .Select(c => EndingZeroRx.Replace(c, ""));

        return string.Join(":", cellsStr);
    }


    public static string ToCompressedGeoIndexString(IList<IGeoIndexCell> cells)
    {
        var str = ToGeoIndexString(cells);

        using var buffer = new MemoryStream();
        using var compress = new DeflateStream(buffer, CompressionMode.Compress);

        {
            using var writer = new StreamWriter(compress, Encoding.ASCII);
            writer.Write(str);
        }

        var b58 = SimpleBase.Base58.Bitcoin.Encode(buffer.ToArray());
        return b58;
    }

    public static IList<IGeoIndexCell> FromCompressedGeoIndexString(string str)
    {
        var bytes = SimpleBase.Base58.Bitcoin.Decode(str);

        using var inputStream = new MemoryStream(bytes);
        using var compress = new DeflateStream(inputStream, CompressionMode.Decompress);

        string cellsStr;
        {
            using var reader = new StreamReader(compress, Encoding.ASCII);
            cellsStr = reader.ReadToEnd();
        }

        var cells = cellsStr
            .Split([':'])
            .Select(c => c + new string('0', 16 - c.Length))
            .Select(c => long.Parse(c, System.Globalization.NumberStyles.HexNumber))
            .Select((c, i) => (IGeoIndexCell)new GeoIndexCell(c, (byte)(S2GeometryHelper.S2MaxLevel - i)))
            .ToList();

        var previousLevel = byte.MaxValue;

        foreach (var cell in cells)
        {
            if (cell.S2Level >= previousLevel)
            {
                throw new ArgumentException("must be ordered by descending S2Level", nameof(cells));
            }

            previousLevel = cell.S2Level;
        }

        return cells;
    }

    public IEnumerable<TReferer> IncludeGeoIndex<TReferer>(IEnumerable<TReferer> referers, double precisionMaxKm)
        where TReferer : IHavePrimaryKeyAndPosition
    {
        var (_, maxLevel) = S2GeometryHelper.S2MinMaxLevelForKm(precisionMaxKm, default);

        return referers.Select(referer => IncludeGeoIndex(referer, maxLevel));
    }

    public void AddReferer<TReferer>(TReferer referer, double? minKm, double? maxKm)
        where TReferer : IHavePrimaryKeyAndPosition

    {
        var indexes = S2GeometryHelper.GetGeoIndexes(referer, minKm, maxKm);

        AddIndexes(indexes);
    }

    public void RemoveReferer<TReferer>(TReferer referer)
        where TReferer : IHavePrimaryKeyAndPosition
    {
        RemoveIndexes(referer);
    }

    public void CopyToReferers<TReferer, TReferer2>(TReferer referer, IEnumerable<TReferer2> referers2) 
        where TReferer : IHavePrimaryKeyAndPosition
        where TReferer2 : IHavePrimaryKey
    {
        foreach(var referer2 in referers2)
        {
            CopyIndexes(referer, referer2);
        }
    }

    public void CopyToReferer<TReferer, TReferer2>(TReferer referer, TReferer2 referer2) 
        where TReferer : IHavePrimaryKeyAndPosition
        where TReferer2 : IHavePrimaryKey
    {
        CopyIndexes(referer, referer2);
    }

    public IGeoIndexCell? RefererGeoIndex<TReferer>(TReferer referer)
        where TReferer : IHavePrimaryKey
    {
        var cells = RefererAllGeoIndex(referer);

        return cells.FirstOrDefault();
    }

    public IList<IGeoIndexCell> RefererAllGeoIndex<TReferer>(TReferer referer)
        where TReferer : IHavePrimaryKey
    {
        var (refererTypeHash, refererHash) = referer.GetRefererHashes();

        var cells =

            from index in Indexes

            where
                index.RefererHash == refererHash
                &&
                index.RefererId == referer.PrimaryKey &&
                index.RefererTypeHash == refererTypeHash

            orderby index.S2Level descending

            select new GeoIndexCell(index.S2CellIdSigned, index.S2Level);

        return cells
            .AsEnumerable()
            .Select(cell => (IGeoIndexCell)cell)
            .ToList();
    }


    public IQueryable<long> GetMatchingRefererLongIds<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
        where TReferer : IHavePrimaryKey
    {
        var (refererTypeHash, _) = default(TReferer).GetRefererHashes();

        var searchIds = searchCells
            .Select(cell => cell.S2CellIdSigned);

        var refererIds =

            from index in Indexes

            where index.RefererTypeHash == refererTypeHash &&
                  searchIds.Contains(index.S2CellIdSigned)

            select index.RefererId;

        return refererIds.Distinct();
    }

    public IQueryable<int> GetMatchingRefererIntIds<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
        where TReferer : IHavePrimaryKey
    {
        return GetMatchingRefererLongIds<TReferer>(searchCells)
            .Select(id => (int)id);
    }

}
