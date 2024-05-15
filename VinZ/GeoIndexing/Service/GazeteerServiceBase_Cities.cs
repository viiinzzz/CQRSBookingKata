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

    public (City?, double) NearestCity(IGeoIndexCell searchCell, double maxKm)
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
                return c.Km < maxKm;
            })
            .OrderBy(c => c.Km);

        return top.FirstOrDefault(_ => true, (null, double.MaxValue)!);
        //
        // var top10 = top
        //
        //     .Take(10) //if we had population data in the city, it would maybe better to propose the nearest biggest city
        //     .ToArray();
        //
        // if (top10.Length == 0)
        // {
        //     return (null, double.MaxValue);
        // }
        //
        // return top10.MinBy(c => c.Km);
    }

}