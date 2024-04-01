using BookingKata.Admin;

namespace BookingKata.API;

public class GazetteerService(
    IDbContextFactory factory
) : GazetteerServiceBase, ITransactionable
{
    private readonly BookingGazeteerContext _geo = factory.CreateDbContext<BookingGazeteerContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _geo;


    public override IQueryable<GeoIndex> Indexes => _geo.Indexes;

    public override void AddIndexes(IEnumerable<GeoIndex> indexes, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            _geo.Indexes.AddRange(indexes);

            foreach (var index in indexes)
            {
                _geo.Entry(index).State = EntityState.Detached;
            }

            _geo.SaveChanges();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public override void CopyIndexes<TReferer, TReferer2>(TReferer referer, TReferer2 referer2, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var (refererTypeHash, refererHash) = referer.GetRefererHashes();
            var (referer2TypeHash, referer2Hash) = referer2.GetRefererHashes();

            var copyIndexes =

                from index in _geo.Indexes

                where index.RefererId == referer.PrimaryKey &&
                      index.RefererTypeHash == refererTypeHash

                select new GeoIndex(index.S2CellIdSigned, index.S2Level,
                    referer2.PrimaryKey, referer2TypeHash, referer2Hash, default);

            _geo.Indexes.AddRange(copyIndexes);

            foreach (var index in copyIndexes)
            {
                _geo.Entry(index).State = EntityState.Detached;
            }

            _geo.SaveChanges();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public override void RemoveIndexes<TReferer>(TReferer referer, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var (refererTypeHash, refererHash) = referer.GetRefererHashes();

            var removedCount =
                _geo.Indexes
                    .Where(index => 
                        // index.RefererId == referer.PrimaryKey &&
                        // index.RefererTypeHash == refererTypeHash &&
                        index.RefererHash == refererHash
                        )
                    .ExecuteDelete();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }
}