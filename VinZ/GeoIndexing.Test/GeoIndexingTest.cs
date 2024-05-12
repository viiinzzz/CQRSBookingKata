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

            var dist = geo.EarthArcDist(Paris.Cells.First(), Marseille.Cells.First());

            Check.That(dist.Km).IsCloseTo(660.1, 0.5);

            var cellsParis = positionParis.AllGeoIndexCell().Select(cell => (IGeoIndexCell)cell).ToList();

            var parisGeoIndexString1 = Paris.geoIndex;
            var parisGeoIndexString2 = GazetteerServiceBase.ToGeoIndexString(cellsParis);
            Check.That(Paris.Cells.First().S2CellIdSigned).Equals(cellsParis.First().S2CellIdSigned);

            var (nearestCity, km) = geo.NearestCity(cellsParis.First());

            Check.That(nearestCity).IsNotNull();
            Check.That(nearestCity.name).Equals("Paris");
            Check.That(km).IsCloseTo(0, 2);
        }
    }
}