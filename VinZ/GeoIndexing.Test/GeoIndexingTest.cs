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

using NFluent;
using VinZ.Common;
using VinZ.GeoIndexing;

namespace GeoIndexing.Test
{
    public class TestReferer : IHavePrimaryKeyAndPosition
    {
        public long PrimaryKey { get; set; }
        public Position? Position { get; set; }


        public IList<IGeoIndexCell> Cells { get; set; }
        public string geoIndex { get; set; }
    }

    public class GazetteerTestService : GazetteerServiceBase
    {
        private List<GeoIndex> _geoIndexes = new();

        private int _geoIndexId = 0;

        public override IQueryable<IGeoIndex> Indexes => _geoIndexes.AsQueryable();

        public override void AddIndexes(IEnumerable<GeoIndex> indexes)
        {
            foreach (var index in indexes)
            {
                    _geoIndexes.Add(index with
                    {
                        GeoIndexId = ++_geoIndexId
                    });
            }
        }

        public override void RemoveIndexes<TReferer>(TReferer referer)
        {
            throw new NotImplementedException();
        
        }

        public override void CopyIndexes<TReferer, TReferer2>(TReferer referer, TReferer2 referer2)
        {
            if (referer is not TestReferer testReferer)
            {
                throw new NotImplementedException();
            }

            if (referer2 is not TestReferer testReferer2)
            {
                throw new NotImplementedException();
            }

            _geoIndexes.Add(_geoIndexes.Single(index => index.RefererId == referer.PrimaryKey) with
            {
                RefererId = testReferer2.PrimaryKey
            });
        }
    }


    public class GeoIndexingTest
    {
        [Fact]
        public void Test0()
        {
            var geo = new GazetteerTestService();

            var positionParis = new Position(48.86472, 2.34901);
            var Paris = new TestReferer
            {
                PrimaryKey = 75,
                Position = positionParis,
            };

            geo.AddReferer(Paris, 0, 40000);
            geo.IncludeGeoIndex(Paris, S2GeometryHelper.S2MaxLevel);

            var geoIndexStr = GazetteerServiceBase.ToGeoIndexString(Paris.Cells);
            var compressedGeoIndexStr = GazetteerServiceBase.ToCompressedGeoIndexString(Paris.Cells);

            var decompressedCells = GazetteerServiceBase.FromCompressedGeoIndexString(compressedGeoIndexStr);

            Check.That(decompressedCells).Equals(Paris.Cells);
        }


        [Fact]
        public void Test1()
        {
            var geo = new GazetteerTestService();

            var positionParis = new Position(48.86472, 2.34901);
            var Paris = new TestReferer
            {
                PrimaryKey = 75,
                Position = positionParis,
            };

            var Marseille = new TestReferer
            {
                PrimaryKey = 13,
                Position = new Position(43.31856, 5.40836),
            };

            geo.AddReferer(Paris, 0, 40000);
            geo.AddReferer(Marseille, 0, 40000);

            geo.IncludeGeoIndex(Paris, S2GeometryHelper.S2MaxLevel);
            geo.IncludeGeoIndex(Marseille, S2GeometryHelper.S2MaxLevel);

            var distKm = geo.EarthArcDist(Paris.Cells.First(), Marseille.Cells.First());
            var expectedDistKm = 659.731;
            var diffDistMeter = 1000 * Math.Abs(distKm.Km - expectedDistKm);
            var worseToleranceMeter = distKm.WorseTolerance * 1000;
            var bestToleranceMeter = distKm.BestTolerance * 1000;
            Check.That(diffDistMeter).IsCloseTo(0, 500);
            Check.That(diffDistMeter).IsCloseTo(0, worseToleranceMeter);
            Check.That(diffDistMeter).IsCloseTo(0, bestToleranceMeter);

            var cellsParis = positionParis.AllGeoIndexCell().Select(cell => (IGeoIndexCell)cell).ToList();

            var parisGeoIndexString1 = Paris.geoIndex;
            var parisGeoIndexString2 = GazetteerServiceBase.ToGeoIndexString(cellsParis);
            Check.That(Paris.Cells.First().S2CellIdSigned).Equals(cellsParis.First().S2CellIdSigned);

            var (nearestCity, km) = geo.NearestCity(cellsParis.First(), 50);

            Check.That(nearestCity).IsNotNull();
            Check.That(nearestCity.name).Equals("Paris");
            Check.That(km).IsCloseTo(0, 2);
        }
    }
}