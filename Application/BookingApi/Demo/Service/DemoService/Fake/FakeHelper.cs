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

namespace BookingKata.API.Demo;

public static class FakeHelper
{


    private static readonly string[] FirstNames ="firstnames1000".GetJsonStringArray("assets");

    private static readonly string[] LastNames = "lastnames1000".GetJsonStringArray("assets");

    public record FakeHotel(
        bool Valid,
        string HotelName,
        int? ranking,
        double Latitude,
        double Longitude,
        string EmailAddress,
        string ReceptionTelephoneNumber,
        string LocationAddress,
        string Url
    );

    private static Regex RankinkRx = new Regex(@"^\D*(\d+)\D*$");

    private static readonly FakeHotel[] HotelsIDF =

        ((JArray)"hotels_idf".GetJsonAsset("assets")).Select(hotel =>
        {
            var hotelName = hotel.SelectToken("fields.nom_commercial")?.Value<string>();
            var latitude = hotel.SelectToken("fields.geo[0]")?.Value<double>();
            var longitude = hotel.SelectToken("fields.geo[1]")?.Value<double>();
            var roomCount = hotel.SelectToken("fields.nombre_de_chambres")?.Value<int>();
            var emailAddress = hotel.SelectToken("fields.courriel")?.Value<string>();
            var receptionTelephoneNumber = hotel.SelectToken("fields.telephone")?.Value<string>();
            var url = hotel.SelectToken("fields.site_internet")?.Value<string>();
            var address = hotel.SelectToken("fields.adresse")?.Value<string>();
            var zipCode = hotel.SelectToken("fields.code_postal")?.Value<int>();
            var city = hotel.SelectToken("fields.commune")?.Value<string>();
            var ranking = hotel.SelectToken("fields.classement")?.Value<string>();
            
            int? rankingNum = default;
            var rankingMatch = RankinkRx.Match(ranking);
            if (rankingMatch.Success && int.TryParse(rankingMatch.Result("$1"), out var rankingNumValue))
            {
                rankingNum = rankingNumValue;
            }

            var valid =
                hotelName != default &&
                latitude.HasValue &&
                longitude.HasValue &&
                roomCount.HasValue &&
                emailAddress != default &&
                receptionTelephoneNumber != default &&
                url != default &&
                address != default &&
                zipCode.HasValue &&
                city != default;

            var locationAddress = !valid ? string.Empty :
            $@"{address}
{zipCode.Value} {city}";

            return new FakeHotel(
                valid, hotelName ?? string.Empty, rankingNum,
                latitude ?? 0, longitude ?? 0,
                emailAddress ?? string.Empty, receptionTelephoneNumber ?? string.Empty,
                locationAddress, url ?? string.Empty);
        })
        
        .ToArray();




    public record FakeCustomer(
        string EmailAddress,
        string LastName,
        string FirstName,
        long DebitCardNumber,
        DebitCardSecrets DebitCardSecrets
        );

    private static Regex NonAlphanumDash = new ("[^a-zA-Z0-9-]");
    public static FakeCustomer[] GenerateFakeCustomers(int count)
    {
        var norm = (string name) => NonAlphanumDash.Replace(name.Normalize(NormalizationForm.FormD), "_");

        return Enumerable.Range(1, count)

            .Select(i =>
            {
                var lastName = LastNames[LastNames.Length.Rand()];
                var firstName = FirstNames[FirstNames.Length.Rand()];
                var emailAddress = $"{norm(firstName)}.{norm(lastName)}{1000.Rand()}@mail.box";
                var debitCardNumber = 1_0000_0000_0000_0000.Rand();
                var debitCardOwnerName = $"MX {lastName} {firstName}".ToUpper();
                var expireMonth = 12.Rand() + 1;
                var expireYear = (DateTime.UtcNow.Year + 5.Rand()) % 1_00;
                var expire = expireMonth * 1_00 + expireYear;
                var ccv = 1_000.Rand();

                var secrets = new DebitCardSecrets(debitCardOwnerName, expire, ccv);

                return new FakeCustomer(emailAddress, lastName, firstName, debitCardNumber, secrets);
            })

            .ToArray();
    }

    public record FakeEmployee(
        string LastName,
        string FirstName,
        long SocialSecurityNumber,
        double MonthlyIncome,
        string Currency
    );

    public static FakeEmployee[] GenerateFakeEmployees(int count)
    {
        return Enumerable.Range(1, count)

            .Select(i =>
            {
                var lastName = LastNames[LastNames.Length.Rand()];
                var firstName = FirstNames[FirstNames.Length.Rand()];
                var socialSecurityNumber = 1_0_00_00_00_000_000_00.Rand();

                return new FakeEmployee(lastName, firstName, socialSecurityNumber, 1800, "EUR");
            })

            .ToArray();
    }


    public static FakeHotel[] GenerateFakeHotels(int count)
    {
        var bag = HotelsIDF.ToList();

        return Enumerable.Range(1, count)

            .Select(i =>
            {
                FakeHotel? hotel = default;

                while (bag.Count > 0)
                {
                    var j = bag.Count.Rand();
                    hotel = bag[j];
                    bag.RemoveAt(j);

                    if (hotel.Valid)
                    {
                        break;
                    }
                }

                if (hotel == default)
                {
                    throw new Exception("not enough valid hotels");
                }
                
                return hotel;
            })

            .ToArray();
    }

}