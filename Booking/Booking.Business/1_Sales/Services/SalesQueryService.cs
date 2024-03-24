﻿namespace CQRSBookingKata.Sales;

public class SalesQueryService
(
    ISalesRepository sales, 
    IAdminRepository admin,
   
    PricingQueryService pricing, 
    BookingCommandService booking,
   
    ITimeService DateTime
)
{
    private const int FindMinKm = 5;
    private const int FindMaxKm = 200;

    public static readonly City[] Cities = "cities"
        .GetJsonObjectArray<City>("1_Sales/assets")
        .Where(city => city != null)
        .Select(city => city!)
        .ToArray();


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


        var requestPositions = new List<Cells12>();

        if (request is { Latitude: not null, Longitude: not null })
        {
            requestPositions.Add(new Position(request.Latitude.Value, request.Longitude.Value).CellIdsLevel12(maxKm));
        };

        if (request.CityName != default)
        {
            var cities = Cities
                .Where(city => city.name.EqualsIgnoreCaseAndAccents(request.CityName));

            if (!cities.Any() && (request.ApproximateNameMatch ?? false))
            {
                cities = Cities
                    .Where(city => city.name.EqualsApprox(request.CityName));
            }

            if (request.CountryCode != default && request.CountryCode.Trim().Length > 0)
            {
                cities = cities
                    .Where(city => city.country.EqualsIgnoreCaseAndAccents(request.CountryCode));
            }

            requestPositions.AddRange(cities
                .Where(city => city.Position != default)
                .Select(city => city.Position.Value.CellIdsLevel12(maxKm)));
        }

        if (requestPositions.Count == 0)
        {
            throw new ArgumentException($"must specify either known {request.CityName} or {request.Latitude} and {request.Longitude}", nameof(request.CityName));
        }



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
            vacancies = 
                
                from vacancy in vacancies

                where requestPositions.Any(requestCells =>
                    (vacancy.Level12 != null && vacancy.Level12.Value == requestCells.Level12.Value) ||
                    (vacancy.Level11 != null && vacancy.Level11.Value == requestCells.Level11.Value) ||
                    (vacancy.Level10 != null && vacancy.Level10.Value == requestCells.Level10.Value) ||
                    (vacancy.Level9 != null && vacancy.Level9.Value == requestCells.Level9.Value) ||
                    (vacancy.Level8 != null && vacancy.Level8.Value == requestCells.Level8.Value) ||
                    (vacancy.Level7 != null && vacancy.Level7.Value == requestCells.Level7.Value) ||
                    (vacancy.Level6 != null && vacancy.Level6.Value == requestCells.Level6.Value) ||
                    (vacancy.Level5 != null && vacancy.Level5.Value == requestCells.Level5.Value) ||
                    (vacancy.Level4 != null && vacancy.Level4.Value == requestCells.Level4.Value) ||
                    (vacancy.Level3 != null && vacancy.Level3.Value == requestCells.Level3.Value) ||
                    (vacancy.Level2 != null && vacancy.Level2.Value == requestCells.Level2.Value) ||
                    (vacancy.Level1 != null && vacancy.Level1.Value == requestCells.Level1.Value) ||
                    (vacancy.Level0 != null && vacancy.Level0.Value == requestCells.Level0.Value) )

                select vacancy;
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

                string.Equals(stay.Currency, request.Currency, StringComparison.InvariantCultureIgnoreCase))

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

        var setHours = (DateTime date, double hours) 
            => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0)
                .AddHours(hours);

        var arrivalDate =
            requestCheckInHours < hotel.EarliestCheckInHours 
                ? setHours(request.ArrivalDate, hotel.EarliestCheckInHours) 
                : request.ArrivalDate;

        var departureDate =
            requestCheckOutHours > hotel.LatestCheckOutHours 
                ? setHours(request.DepartureDate, hotel.LatestCheckOutTime)
                : request.DepartureDate;

        var cannotPropose = !sales.HasActiveProposition(now, request.Urid, arrivalDate, departureDate);

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

        return prop;
    }

    


    public long[] Booking(UniqueRoomId urid, DateTime openingDate, DateTime closingDate, int personCount, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

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

           
            var nearestKnownCityName =
                hotel.Position == default
                    ? default
                    : Cities
                        .Select(city => new
                        {
                            cityName = city.name,
                            km = city.Position == default
                                ? double.MaxValue
                                : city?.Cells?[0].Id.EarthKm(hotel.Cells.First().S2CellId)
                        })
                        .MinBy(c => c.km)
                        ?.cityName;


            var firstNight = OvernightStay.From(openingDate);
            var lastNight = OvernightStay.FromCheckOutDate(closingDate);

            var vacancies = firstNight
                .StayUntil(lastNight, personCount, hotel.Latitude, hotel.Longitude, hotel.Cells12,
                     hotel.HotelName, nearestKnownCityName, urid.Value)
                .ToArray();

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