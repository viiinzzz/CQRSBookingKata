
using CQRSBookingKata.Assets;
using CQRSBookingKata.Billing;
using CQRSBookingKata.Common;
using CQRSBookingKata.ThirdParty;

namespace CQRSBookingKata.Sales;

//sales and marketing, event planning

public class SalesQueryService(
   ISalesRepository sales, IAssetsRepository assets,
   TimeService DateTime, PricingQueryService pricing, BookingCommandService booking
   )
{
    private const int FindMinKm = 5;
    private const int FindMaxKm = 200;
    private IEnumerable<StayMatch> FullFind(StayRequest request)
    {
        var maxKm =
            request.maxKm < FindMinKm ? FindMinKm 
            : request.maxKm > FindMaxKm ? FindMaxKm
            : request.maxKm;

        var firstNight = OvernightStay.From(request.ArrivalDate);
        var lastNight = OvernightStay.FromCheckOutDate(request.DepartureDate);

        var nightsCount = firstNight.To(lastNight);

        var urids = sales.Stock

            .Where(room => 
                
                room.PersonMaxCount >= request.PersonCount &&

                room.DayNum >= firstNight.DayNum &&
                room.DayNum <= lastNight.DayNum &&

                new Position(room.Latitude, room.Longitude).EarthDistKm(
                    new Position(request.Latitude, request.Longitude)) < maxKm
                )

            .GroupBy(freeRoom => freeRoom.Urid)

            .Where(nights => nights.Count() == nightsCount)

            .Select(nights => nights.Key);

        return urids

            .ToArray()

            .Select(urid =>
            {
                var price = pricing.GetPrice(urid, request.PersonCount, request.ArrivalDate, request.DepartureDate);

                var match = new StayMatch(
                    request.PersonCount,
                    request.ArrivalDate, request.DepartureDate,
                    price.Amount, price.Currency,
                    urid);

                return match;
            })
            .Where(match => 
                            match.Price >= request.PriceMin &&
                            match.Price <= request.PriceMax &&

                            string.Equals(match.Currency, request.Currency, StringComparison.InvariantCultureIgnoreCase))
            
            .ToArray();
    }


    public StayMatch[] Find(StayRequest request, int start, int count)
    {
        var matches = FullFind(request);

        return matches
            .Skip(start)
            .Take(count)
            .ToArray();
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
        var hotel = assets.GetHotel(uniqueRoomId.HotelId);

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

        var price = pricing.GetPrice(request.Urid, request.PersonCount, arrivalDate, departureDate);

        var prop = new StayProposition(
            request.PersonCount,
            arrivalDate, departureDate,
            price.Amount, price.Currency,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(FreeLockMinutes),
            request.Urid);

        return prop;
    }

    


    public void OpenBooking(UniqueRoomId urid, DateTime openingDate, DateTime closingDate, int personCount)
    {
        var room = assets.GetRoom(urid.Value);

        if (room == default)
        {
            throw new RoomDoesNotExistException();
        }

        var hotel = assets.GetHotel(urid.HotelId);

        if (hotel == default)
        {
            throw new HotelDoesNotExistException();
        }

        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var vacancies = firstNight.Until(lastNight, personCount, hotel.Latitude, hotel.Longitude, urid.Value);

        //
        //
        sales.AddVacancy(vacancies);
        //
        //
    }

    public void CloseBooking(UniqueRoomId urid, DateTime openingDate, DateTime closingDate)
    {
        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var vacancyIds = firstNight.Until(lastNight, 0,0,0, urid.Value)
            
            .Select(vacancy => vacancy.VacancyId);

        //
        //
        sales.RemoveVacancies(vacancyIds);
        //
        //
    }
}