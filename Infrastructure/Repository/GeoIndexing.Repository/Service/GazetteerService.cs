/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace Support.Infrastructure.Storage;

public class GazetteerService(
    IDbContextFactory factory
) : GazetteerServiceBase, ITransactionable
{
    private readonly GazeteerContext _geo = factory.CreateDbContext<GazeteerContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _geo;


    public override IQueryable<GeoIndex> Indexes => _geo.Indexes;

    public override void AddIndexes(IEnumerable<GeoIndex> indexes)
    {
        _geo.Indexes.AddRange(indexes);

        foreach (var index in indexes)
        {
            _geo.Entry(index).State = EntityState.Detached;
        }

        _geo.SaveChanges();
    }

    public override void CopyIndexes<TReferer, TReferer2>(TReferer referer, TReferer2 referer2)
    {
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
    }

    public override void RemoveIndexes<TReferer>(TReferer referer)
    {
            var (refererTypeHash, refererHash) = referer.GetRefererHashes();

            var removedCount =
                _geo.Indexes
                    .Where(index =>
                        // index.RefererId == referer.PrimaryKey &&
                        // index.RefererTypeHash == refererTypeHash &&
                        index.RefererHash == refererHash
                        )
                    .ExecuteDelete();
    }
}