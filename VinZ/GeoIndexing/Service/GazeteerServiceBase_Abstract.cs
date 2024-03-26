namespace VinZ.GeoIndexing;

public abstract partial class GazeteerServiceBase : IGazeteerService
{
    public abstract IQueryable<GeoIndex> Indexes { get; }
    public abstract void AddIndexes(IEnumerable<GeoIndex> indexes, bool scoped);
    public abstract void RemoveIndexes<TReferer>(TReferer referer, bool scoped) 
        where TReferer : IHavePrimaryKey;

    public abstract void CopyIndexes<TReferer, TReferer2>(TReferer referer, TReferer2 referer2, bool scoped)
        where TReferer : IHavePrimaryKey
        where TReferer2 : IHavePrimaryKey;
}


