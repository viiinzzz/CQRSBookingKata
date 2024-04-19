namespace BookingKata.Sales;

public partial class SalesQueryService
{
    public IQueryable<StayMatch> FindStay(StayRequest request, int customerId)
    {
        var firstNight = OvernightStay.From(request.ArrivalDate);
        var lastNight = OvernightStay.FromCheckOutDate(request.DepartureDate);


        var maxKm =
            !request.MaxKm.HasValue ? FindMaxKm
            : request.MaxKm < FindMinKm ? FindMinKm
            : request.MaxKm > FindMaxKm ? FindMaxKm
            : request.MaxKm.Value;


        var nightsCount = firstNight.To(lastNight);


        var requestCells = new List<IGeoIndexCell>();

        if (request is { Latitude: not null, Longitude: not null })
        {
            var positionCells = geo.NewGeoIndex(request, bconf.PrecisionMaxKm, maxKm);

            requestCells.AddRange(positionCells);
        }

        ;

        if (request.CityName != default)
        {
            var citiesCells = geo
                .QueryCities(request.CityName, request.ApproximateNameMatch, request.CountryCode)
                .Where(city => city.Position != default)
                .SelectMany(city => geo.CacheGeoIndex(city, bconf.PrecisionMaxKm))
                .AsEnumerable();

            requestCells.AddRange(citiesCells);
        }

        if (requestCells.Count == 0)
        {
            throw new ArgumentException(
                $"must specify either known {request.CityName} or {request.Latitude} and {request.Longitude}",
                nameof(request.CityName));
        }

        var matchingVacancyIds = geo.GetMatchingRefererLongIds<Vacancy>(requestCells).ToHashSet();


        var vacancies =
            from vacancy in sales.Vacancies
            where vacancy.PersonMaxCount >= request.PersonCount &&
                  vacancy.DayNum >= firstNight.DayNum &&
                  vacancy.DayNum <= lastNight.DayNum
            select vacancy;


        if (request.HotelName != default)
        {
            vacancies =
                from vacancy in vacancies
                where !(request.ApproximateNameMatch ?? false)
                    ? vacancy.HotelName.EqualsIgnoreCaseAndAccents(request.HotelName)
                    : vacancy.HotelName.EqualsApprox(request.HotelName)
                select vacancy;
        }


        {
            vacancies = vacancies
                .Where(vacancy => matchingVacancyIds.Contains(vacancy.VacancyId));
        }


        var urids =
            from stay in
                from vacancy in vacancies
                group vacancy by vacancy.UniqueRoomId
                into stay
                select new
                {
                    urid = stay.Key,
                    nightsCount = stay.Count(),
                    personMaxCount = stay.First().PersonMaxCount
                }
            where stay.nightsCount == nightsCount
            orderby stay.personMaxCount
            select stay.urid;


        var originator = GetType().FullName
                         ?? throw new Exception("invalid originator");

        var roomDetails = bus.AskResult<RoomDetails[]>(
            originator, Recipient.Admin, Verb.Admin.RequestManyRoomDetails,
            new RoomDetailsRequest
            {
                onlyRoomNumbers =
                    urids.ToArray() //fetch into db only good room size, geo-localization and availability timing
            });

        if (roomDetails is null or { Length: 0 })
        {
            throw new RoomNotFoundException();
        }


        var customerProfile = GetCustomerProfile(customerId);


        var stays = roomDetails
            .Select(room =>
            {
                var price = pricing.GetPrice(
                    //room
                    room.PersonMaxCount, room.FloorNum, room.FloorNumMax, room.HotelRank, room.Latitude, room.Longitude,

                    //booking
                    request.PersonCount, request.ArrivalDate, request.DepartureDate, request.Currency, customerProfile);

                var match = new StayMatch(
                    request.PersonCount,
                    request.ArrivalDate, request.DepartureDate,
                    price.Amount, price.Currency,
                    urid);

                return match;
            })
            .Where(stay => //now filter with dynamic pricing
                (!request.PriceMax.HasValue || stay.Price <= request.PriceMax) &&
                (!request.PriceMin.HasValue || request.PriceMax.HasValue && request.PriceMax <= request.PriceMin ||
                 stay.Price >= request.PriceMin) &&
                (request.Currency == null || string.Equals(stay.Currency, request.Currency,
                    StringComparison.InvariantCultureIgnoreCase)))
            .AsQueryable();

        if (request.PriceMax is > 0)
        {
            return stays.OrderBy(stay => stay.Price);
        }

        if (request.PriceMin is > 0)
        {
            return stays.OrderByDescending(stay => stay.Price);
        }

        return stays;
    }
}