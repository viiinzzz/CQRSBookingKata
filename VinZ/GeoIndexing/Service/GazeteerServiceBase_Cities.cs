namespace VinZ.GeoIndexing;

public abstract partial class GazeteerServiceBase
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
        var ret = Cities

            .Select(city =>
            {
                var cityCell = GeoIndex(city);

                var km = double.MaxValue;

                if (city.Position != default)
                {
                    var distance = EarthArcDist(searchCell, cityCell);

                    km = distance.Km;
                }

                return (City: city, Km: km);
            })

            .MinBy(c => c.Km) ;

            return ret;
    }

}