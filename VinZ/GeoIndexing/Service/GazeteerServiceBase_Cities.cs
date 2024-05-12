namespace VinZ.GeoIndexing;

public abstract partial class GazetteerServiceBase
{
    private static readonly City[] Cities = "cities"
        .GetJsonObjectArray<City>("assets")
        .Where(city => city != null)
        .Select(city => city!)
        .ToArray();

    public IQueryable<City> QueryCities(string cityName, bool? approximateNameMatch, string? countryCode)
    {
        var cities = Cities
            .Where(city => city.name.EqualsIgnoreCaseAndAccents(cityName))
            .AsQueryable();

        if (!cities.Any() && (approximateNameMatch ?? false))
        {
            cities = Cities
                .Where(city => city.name.EqualsApprox(cityName))
                .AsQueryable();
        }

        if (countryCode != default && countryCode.Trim().Length > 0)
        {
            cities = cities
                .Where(city => city.country.EqualsIgnoreCaseAndAccents(countryCode));
        }

        return cities;
    }

    public (City?, double) NearestCity(IGeoIndexCell searchCell)
    {
        var top = Cities

            .Select(city =>
            {
                var cityCell = CacheGeoIndex(city);

                var km = double.MaxValue;

                if (city.Position != default)
                {
                    var distance = EarthArcDist(searchCell, cityCell);

                    km = distance.Km;
                }

                return (City: city, Km: km);
            })

            .Where(c =>
            {
                return c.Km < 1000;
            })
            .OrderBy(c => c.Km)

            .Take(10)
            .ToArray();

        if (top.Length == 0)
        {
            return (null, double.MaxValue);
        }

        return top.MinBy(c => c.Km);
    }

}