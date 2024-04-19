namespace BookingKata.Sales;

public partial class SalesQueryService
{
    public StayProposition? LockStay(StayMatch request, int customerId)
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


        var originator = GetType().FullName
                         ?? throw new Exception("invalid originator");

        var roomDetails = bus.AskResult<RoomDetails>(
            originator, Recipient.Admin, Verb.Admin.RequestSingleRoomDetails,
            new RoomDetailsRequest
            {
                onlyRoomNumbers = new[] { request.Urid }
            });

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

        var room = admin.GetRoomDetails(uniqueRoomId.HotelId, default);

        var customerProfile = GetCustomerProfile(customerId);

        var price = pricing.GetPrice(
            room.PersonMaxCount, room.FloorNum, room.FlootNumMax, room.HotelRank, room.Latitude, room.Longitude,
            request.PersonCount, arrivalDate, departureDate, request.Currency, customerProfile);

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
}