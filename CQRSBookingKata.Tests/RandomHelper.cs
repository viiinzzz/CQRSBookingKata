
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CQRSBookingKata.Tests;

public static class RandomHelper
{
    private static readonly string AssetsDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "assets");

    public static JToken GetJsonAsset(string nameWithoutExt)
    {
        var assetPath = Path.Combine(
            AssetsDir,
            $"{nameWithoutExt}.json");

        using var file = File.OpenText(assetPath);

        using var reader = new JsonTextReader(file);
        
        return JToken.ReadFrom(reader);
    }

    private static string[] GetJsonStringArray(string nameWithoutExt)

        => GetJsonAsset(nameWithoutExt)
            .Values<string>()
            .Where(str => str != null)
            .Select(str => $"{str}")
            .ToArray();

    private static readonly string[] FirstNames = GetJsonStringArray("firstnames1000");

    private static readonly string[] LastNames = GetJsonStringArray("lastnames1000");

    record City(string name, string lat, string lng, string country, string admin1, string admin2)
    {
        double? Latitude { get {

            if (!double.TryParse(lat, out var value))
            {
                return default;
            }

            return value;
        }}

        double? Longitude { get {

            if (!double.TryParse(lng, out var value))
            {
                return default;
            }

            return value;
        }}
    }

    private static readonly City[] Cities =

        GetJsonAsset("cities")
            .Values<City>()
            .Where(city => city != null)
            .Select(city => city ?? throw new Exception("shall not happen"))
            .ToArray();


    public record FakeHotel(
        bool Valid,
        string HotelName,
        double Latitude,
        double Longitude,
        string EmailAddress,
        string ReceptionTelephoneNumber,
        string LocationAddress,
        string Url
    );

    private static readonly FakeHotel[] HotelsIDF =

        ((JArray)GetJsonAsset("hotels_idf")).Select(hotel =>
        {
            var hotelName = hotel.SelectToken("fields.nom_commercial")?.Value<string>();
            var latitude = hotel.SelectToken("fields.geo[0]")?.Value<double>();
            var longitude = hotel.SelectToken("fields.geo[1]")?.Value<double>();
            var roomCount = hotel.SelectToken("fields.nombre_de_chambres")?.Value<int>();
            var emailAddress = hotel.SelectToken("fields.courriel")?.Value<string>();
            var receptionTelephoneNumber = hotel.SelectToken("fields.telephone")?.Value<string>();
            var url = hotel.SelectToken("fields.nom_commercial")?.Value<string>();
            var address = hotel.SelectToken("fields.adresse")?.Value<string>();
            var zipCode = hotel.SelectToken("fields.code_postal")?.Value<int>();
            var city = hotel.SelectToken("fields.site_internet")?.Value<string>();

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
                valid, hotelName ?? string.Empty, 
                latitude ?? 0, longitude ?? 0,
                emailAddress ?? string.Empty, receptionTelephoneNumber ?? string.Empty,
                locationAddress, url ?? string.Empty);
        })
        
        .ToArray();



    private static readonly Random random = new();

    public record FakeCustomer(
        string LastName,
        string FirstName,
        long DebitCardNumber,
        string DebitCardOwnerName,
        int Expire,
        int CCV
        );

    public static FakeCustomer[] GenerateFakeCustomers(int count)
    {
        return Enumerable.Range(1, count)

            .Select(i =>
            {
                var lastName = LastNames[random.Next(LastNames.Length)];
                var firstName = LastNames[random.Next(LastNames.Length)];
                var debitCardNumber = random.NextInt64(1_0000_0000_0000_0000);
                var debitCardOwnerName = $"MX {lastName} {firstName}".ToUpper();
                var expireMonth = random.Next(12) + 1;
                var expireYear = (DateTime.Now.Year + random.Next(5)) % 1_00;
                var expire = expireMonth * 1_00 + expireYear;
                var ccv = random.Next(1_000);

                return new FakeCustomer(
                    lastName, firstName,
                    debitCardNumber, debitCardOwnerName,
                    expire, ccv);
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
                var lastName = LastNames[random.Next(LastNames.Length)];
                var firstName = LastNames[random.Next(LastNames.Length)];
                var socialSecurityNumber = random.NextInt64(1_0_00_00_00_000_000_00);

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
                    var j = random.Next(bag.Count);
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