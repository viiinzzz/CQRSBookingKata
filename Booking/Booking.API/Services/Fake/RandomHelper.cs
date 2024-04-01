namespace BookingKata.API.Demo;

public static class RandomHelper
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



    private static readonly Random random = new();
    public static int Rand(int max) => random.Next(max);
    public static long Rand(long max) => random.NextInt64(max);
    public static double Rand(double max) => random.NextDouble() * max;

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
                var lastName = LastNames[random.Next(LastNames.Length)];
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var emailAddress = $"{norm(firstName)}.{norm(lastName)}{random.Next(1000)}@mail.box";
                var debitCardNumber = random.NextInt64(1_0000_0000_0000_0000);
                var debitCardOwnerName = $"MX {lastName} {firstName}".ToUpper();
                var expireMonth = random.Next(12) + 1;
                var expireYear = (DateTime.UtcNow.Year + random.Next(5)) % 1_00;
                var expire = expireMonth * 1_00 + expireYear;
                var ccv = random.Next(1_000);

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
                var lastName = LastNames[random.Next(LastNames.Length)];
                var firstName = FirstNames[random.Next(FirstNames.Length)];
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