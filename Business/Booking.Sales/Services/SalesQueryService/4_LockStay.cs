using System.Text.Json;

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

        var roomDetail = bus.AskResult<RoomDetails>(
            originator, Recipient.Admin, Verb.Admin.RequestSingleRoomDetails,
            new RoomDetailsRequest
            {
                onlyRoomNumbers = [request.Urid]
            });

        //adjust arrival/departure to hotel time
        var requestCheckInHours = request.ArrivalDate.Hour + request.ArrivalDate.Minute / 60d;
        var requestCheckOutHours = request.DepartureDate.Hour + request.DepartureDate.Minute / 60d;


        var arrivalDate = request.ArrivalDate;

        if (requestCheckInHours < roomDetail.EarliestCheckInHours)
        {
            arrivalDate = request.ArrivalDate
                .DayStart()
                .AddHours(roomDetail.EarliestCheckInHours)
                .RoundToTheSecond();
        }

        var departureDate = request.DepartureDate;

        if (requestCheckOutHours > roomDetail.LatestCheckOutHours)
        {
            departureDate = request.DepartureDate
                .DayStart()
                .AddHours(roomDetail.LatestCheckOutHours)
                .RoundToTheSecond();
        }


        var cannotPropose = sales.HasActiveProposition(now, request.Urid, arrivalDate, departureDate);

        if (cannotPropose)
        {
            return default;
        }

        var customerProfile = GetCustomerProfile(customerId);


        var price = bus.AskResult<Price>(
            originator, Support.Services.ThirdParty.Recipient, Support.Services.ThirdParty.Verb.RequestPricing,
            new PricingRequest
            {
                //room
                personMaxCount = roomDetail.PersonMaxCount,
                floorNum = roomDetail.FloorNum,
                floorNumMax = roomDetail.FloorNumMax,
                hotelRank = roomDetail.HotelRank,
                latitude = roomDetail.Latitude,
                longitude = roomDetail.Longitude,

                //booking
                personCount = request.PersonCount,
                arrivalDateUtc = request.ArrivalDate.SerializeUniversal(),
                departureDateUtc = request.DepartureDate.SerializeUniversal(),
                currency = request.Currency,
                customerProfileJson = JsonSerializer.Serialize(customerProfile)
            });


        var optionStart = now;
        var optionEnd = now.AddMinutes(FreeLockMinutes);

        var prop = new StayProposition
        {
            PersonCount = request.PersonCount,
            NightsCount = nightsCount,

            ArrivalDateUtc = arrivalDate.SerializeUniversal(),
            ArrivalDayNum = OvernightStay.From(arrivalDate).DayNum,

            DepartureDateUtc = departureDate.SerializeUniversal(),
            DepartureDayNum = OvernightStay.From(departureDate).DayNum,

            Price = price.Amount,
            Currency = price.Currency,

            OptionStartUtc = optionStart.SerializeUniversal(),
            OptionStartDayNum = OvernightStay.From(optionStart).DayNum,
            OptionEndUtc = optionEnd.SerializeUniversal(),
            OptionEndDayNum = OvernightStay.From(optionEnd).DayNum,

            Urid = request.Urid
        };
        

        sales.AddStayProposition(prop);

        return prop;
    }
}