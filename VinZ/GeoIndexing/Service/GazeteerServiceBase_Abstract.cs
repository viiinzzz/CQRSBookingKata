namespace VinZ.GeoIndexing;

public abstract partial class GazetteerServiceBase : IGazetteerService
{
    public abstract IQueryable<GeoIndex> Indexes { get; }
    public abstract void AddIndexes(IEnumerable<GeoIndex> indexes);
    public abstract void RemoveIndexes<TReferer>(TReferer referer) 
        where TReferer : IHavePrimaryKey;

    public abstract void CopyIndexes<TReferer, TReferer2>(TReferer referer, TReferer2 referer2)
        where TReferer : IHavePrimaryKey
        where TReferer2 : IHavePrimaryKey;
}


