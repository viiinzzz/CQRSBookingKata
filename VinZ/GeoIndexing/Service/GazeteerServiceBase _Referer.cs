namespace VinZ.GeoIndexing;

public abstract partial class GazeteerServiceBase
{
    public void AddReferer<TReferer>(TReferer referer, double? minKm, double? maxKm, bool scoped)
        where TReferer : IHavePosition, IHavePrimaryKey

    {
        var indexes = S2GeometryHelper.GetGeoIndexes(referer, minKm, maxKm);

        AddIndexes(indexes, scoped);
    }

    public void RemoveReferer<TReferer>(TReferer referer, bool scoped)
        where TReferer : IHavePosition, IHavePrimaryKey
    {
        RemoveIndexes(referer, scoped);
    }

    public void CopyToReferers<TReferer, TReferer2>(TReferer referer, IEnumerable<TReferer2> referers2, bool scoped) 
        where TReferer : IHavePosition, IHavePrimaryKey
        where TReferer2 : IHavePrimaryKey
    {
        foreach(var referer2 in referers2)
        {
            CopyIndexes(referer, referer2, scoped);
        }
    }

    public IGeoIndexCell? RefererGeoIndex<TReferer>(TReferer referer)
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

        return cells.FirstOrDefault();
    }

    // public bool IsMatch<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
    //     where TReferer : IHavePrimaryKey 
    //
    //     => QueryMatch<TReferer>(searchCells)
    //         .Any();

    public IQueryable<int> GetMatchingRefererIntIds<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
        where TReferer : IHavePrimaryKey 

        => GetMatchingRefererLongIds<TReferer>(searchCells)
            .Select(id => (int)id);

    public IQueryable<long> GetMatchingRefererLongIds<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
        where TReferer : IHavePrimaryKey 

        => QueryMatch<TReferer>(searchCells)
            .Distinct();

    private IQueryable<long> QueryMatch<TReferer>(IEnumerable<IGeoIndexCell> searchCells) 
        where TReferer : IHavePrimaryKey
    {
        var (refererTypeHash, _) = default(TReferer).GetRefererHashes();

        var refererIds =

            from index in Indexes

            join search in searchCells
                on index.S2CellIdSigned equals search.S2CellIdSigned

            where index.RefererTypeHash == refererTypeHash

            select index.RefererId;

        return refererIds;
    }
}
