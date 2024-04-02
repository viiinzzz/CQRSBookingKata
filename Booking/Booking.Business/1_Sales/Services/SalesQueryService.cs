namespace BookingKata.Sales;


public class SalesQueryService
(
    ISalesRepository sales, 
    IAdminRepository admin,
   
    PricingQueryService pricing, 
    // BookingCommandService booking,

    IGazetteerService geo,
    ITimeService DateTime
)
{
    public const double PrecisionMaxKm = 0.5;
    private const int FindMinKm = 1;
    private const int FindMaxKm = 200;



    public IQueryable<StayMatch> Find(StayRequest request)
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
            var positionCells = geo.NewGeoIndex(request, PrecisionMaxKm, maxKm);

            requestCells.AddRange(positionCells);
        };

        if (request.CityName != default)
        {
            var citiesCells = geo
                .QueryCities(request.CityName, request.ApproximateNameMatch, request.CountryCode)
                .Where(city => city.Position != default)
                .SelectMany(city => geo.CacheGeoIndex(city, PrecisionMaxKm))
                .AsEnumerable();

            requestCells.AddRange(citiesCells);
        }

        if (requestCells.Count == 0)
        {
            throw new ArgumentException($"must specify either known {request.CityName} or {request.Latitude} and {request.Longitude}", nameof(request.CityName));
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
            
            from stay in (
                from vacancy in vacancies
                group vacancy by vacancy.UniqueRoomId
                into stay
                select new { urid = stay.Key, nightsCount = stay.Count(), personMaxCount = stay.First().PersonMaxCount }
                )
            where stay.nightsCount == nightsCount
            orderby stay.personMaxCount

            select stay.urid;


        var stays = urids
            .ToArray() //fetch into db good size, localization and timing

            .Select(urid =>
            {
                var price = pricing.GetPrice(urid, request.PersonCount, request.ArrivalDate, request.DepartureDate, request.Currency);

                var match = new StayMatch(
                    request.PersonCount,
                    request.ArrivalDate, request.DepartureDate,
                    price.Amount, price.Currency,
                    urid);

                return match;
            })

            .Where(stay => //now filter with dynamic pricing

                (!request.PriceMax.HasValue || stay.Price <= request.PriceMax) &&
                (!request.PriceMin.HasValue || (request.PriceMax.HasValue && request.PriceMax <= request.PriceMin) || stay.Price >= request.PriceMin) &&

                (request.Currency == null || string.Equals(stay.Currency, request.Currency, StringComparison.InvariantCultureIgnoreCase)))

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

   


    public const int FreeLockMinutes = 30;

    public StayProposition? LockProposition(StayMatch request)
    {
        var now = DateTime.UtcNow;
        ;
        var firstNight = OvernightStay.From(request.ArrivalDate);
        var lastNight = OvernightStay.FromCheckOutDate(request.DepartureDate);

        var nightsCount = firstNight.To(lastNight);

        var stillAvailable = sales.Vacancies

            .Where(room =>

                room.PersonMaxCount >= request.PersonCount &&

                room.DayNum >= firstNight.DayNum &&
                room.DayNum <= lastNight.DayNum &&

                room.UniqueRoomId == request.Urid
            )

            .GroupBy(freeRoom => freeRoom.UniqueRoomId)

            .Any(nights => nights.Count() == nightsCount);


        var uniqueRoomId = new UniqueRoomId(request.Urid);
        var hotel = admin.GetHotel(uniqueRoomId.HotelId);

        //adjust arrival/departure to hotel time
        var requestCheckInHours = request.ArrivalDate.Hour + request.ArrivalDate.Minute / 60d;
        var requestCheckOutHours = request.DepartureDate.Hour + request.DepartureDate.Minute / 60d;


        var arrivalDate = request.ArrivalDate;

        if (requestCheckInHours < hotel.EarliestCheckInHours)
        {
            arrivalDate = request.ArrivalDate
                .DayStart()
                .AddHours(hotel.EarliestCheckInHours)
                .RoundToTheSecond();
        }

        var departureDate = request.DepartureDate;

        if (requestCheckOutHours > hotel.LatestCheckOutHours)
        {
            departureDate = request.DepartureDate
                .DayStart()
                .AddHours(hotel.LatestCheckOutHours)
                .RoundToTheSecond();
        }


        var cannotPropose = sales.HasActiveProposition(now, request.Urid, arrivalDate, departureDate);

        if (cannotPropose)
        {
            return default;
        }

        var price = pricing.GetPrice(request.Urid, request.PersonCount, arrivalDate, departureDate, request.Currency);

        var prop = new StayProposition(
            request.PersonCount,
            arrivalDate, departureDate,
            price.Amount, price.Currency,
            now,
            now.AddMinutes(FreeLockMinutes),
            request.Urid);

        sales.AddStayProposition(prop);

        return prop;
    }

    


    public long[] Booking(UniqueRoomId urid, DateTime openingDate, DateTime closingDate, int personCount, bool scoped)
    {
        var room = admin.GetRoom(urid.Value);

        if (room == default)
        {
            throw new RoomDoesNotExistException();
        }

        var hotel = admin.GetHotel(urid.HotelId);

        if (hotel == default)
        {
            throw new HotelDoesNotExistException();
        }

        var hotelCell = geo.RefererGeoIndex(hotel);

        if (hotelCell == null)
        {
            throw new HotelNotGeoIndexedException();
        }

        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var (nearestKnownCity, nearestKnownCityKm) = geo.NearestCity(hotelCell);


            var firstNight = OvernightStay.From(openingDate);
            var lastNight = OvernightStay.FromCheckOutDate(closingDate);

            var vacancies = firstNight
                .StayUntil(lastNight, personCount, hotel.Latitude, hotel.Longitude,
                     hotel.HotelName, nearestKnownCity?.name, urid.Value)
                .ToArray();

            geo.CopyToReferers(hotel, vacancies, scoped);

            //
            //
            sales.AddVacancies(vacancies, false);
            //
            //

            scope?.Complete();

            return vacancies
                .Select(vacancy => vacancy.VacancyId)
                .ToArray();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public long[] CloseBooking(UniqueRoomId urid, DateTime openingDate, DateTime closingDate, bool scoped)
    {

        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var firstNight = OvernightStay.From(openingDate);
            var lastNight = OvernightStay.FromCheckOutDate(closingDate);

            var vacancyIds = firstNight.StayUntil(lastNight, urid.Value);

            //
            //
            sales.RemoveVacancies(vacancyIds, false);
            //
            //

            scope?.Complete();

            return vacancyIds;
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

}