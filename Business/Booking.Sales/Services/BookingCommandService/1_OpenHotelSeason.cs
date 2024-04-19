namespace BookingKata.Sales;

public partial class BookingCommandService
{
    public void OpenHotelSeason(int hotelId, int[]? exceptRoomNumbers, DateTime openingDate, DateTime closingDate)
    {
        var originator = this.GetType().FullName;


        var hotelGeoProxy = bus.AskResult<GeoProxy>(
            originator, Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(hotelId));

        if (hotelGeoProxy == null)
        {
            throw new HotelNotFoundException();
        }


        var roomDetails = bus.AskResult<RoomDetails[]>(
            originator, Recipient.Admin, Verb.Admin.RequestRoomDetails,
            new HotelRoomDetailsRequest(hotelId, exceptRoomNumbers));

        if (roomDetails is null or { Length: 0 })
        {
            throw new RoomNotFoundException();
        }


        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var dayNumbers = firstNight.DayNum.RangeTo(lastNight.DayNum);


        var vacancies = roomDetails
            .SelectMany(roomDetail => dayNumbers
                .Select(dayNum => new Vacancy(dayNum,
                    roomDetail.PersonMaxCount, roomDetail.Latitude, roomDetail.Longitude,
                    roomDetail.HotelName, roomDetail.NearestKnownCityName, false, roomDetail.Urid)
                )
            ).ToList();


        geo.CopyToReferers(hotelGeoProxy, vacancies);

        sales.AddVacancies(vacancies);
    }
}