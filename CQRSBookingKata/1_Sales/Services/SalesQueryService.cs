
using CQRSBookingKata.Common;

namespace CQRSBookingKata.Sales;

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

    public static readonly City[] Cities = "cities".GetJsonObjectArray<City>("1_Sales/assets")
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


        var requestPositions = new List<Position>();

        if (request is { Latitude: not null, Longitude: not null })
        {
            requestPositions.Add(new Position(request.Latitude.Value, request.Longitude.Value));
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
                .Select(city => city.Position ?? new Position(0,0)));
        }

        if (requestPositions.Count == 0)
        {
            throw new ArgumentException($"must specify either known {request.CityName} or {request.Latitude} and {request.Longitude}", nameof(request.CityName));
        }



        var vacancies = 
            
            from vacancy in sales.Stock
            
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
            
                where requestPositions.Any(requestPosition =>
                    requestPosition.IsEarthMatch(vacancy.Latitude, vacancy.Longitude, maxKm))
                
            select vacancy;
        }


        var urids =
            
            from stay in (
                from vacancy in vacancies
                group vacancy by vacancy.Urid
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
        var firstNight = OvernightStay.From(request.ArrivalDate);
        var lastNight = OvernightStay.FromCheckOutDate(request.DepartureDate);

        var nightsCount = firstNight.To(lastNight);

        var stillAvailable = sales.Stock

            .Where(room =>

                room.PersonMaxCount >= request.PersonCount &&

                room.DayNum >= firstNight.DayNum &&
                room.DayNum <= lastNight.DayNum &&

                room.Urid == request.Urid
            )

            .GroupBy(freeRoom => freeRoom.Urid)

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

        var cannotPropose = !sales.HasActiveProposition(request.Urid, arrivalDate, departureDate);

        if (cannotPropose)
        {
            return default;
        }

        var price = pricing.GetPrice(request.Urid, request.PersonCount, arrivalDate, departureDate, request.Currency);

        var prop = new StayProposition(
            request.PersonCount,
            arrivalDate, departureDate,
            price.Amount, price.Currency,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(FreeLockMinutes),
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
                            km = city.Position == default ? double.MaxValue : city.Position - hotel.Position
                        })
                        .MinBy(c => c.km)
                        ?.cityName;


            var firstNight = OvernightStay.From(openingDate);
            var lastNight = OvernightStay.FromCheckOutDate(closingDate);

            var vacancies = firstNight
                .StayUntil(lastNight, personCount, hotel.Latitude, hotel.Longitude, hotel.HotelName, nearestKnownCityName, urid.Value)
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