using System.Text.RegularExpressions;

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
        var cellsStr = S2Levels
            .Select(level => cells.FirstOrDefault(cell => cell.S2Level == level))
            .Select(c => c == default ? "" : $"{c.S2CellIdSigned:x16}")
            .Select(c => EndingZeroRx.Replace(c, ""));

        return string.Join(":", cellsStr);
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
