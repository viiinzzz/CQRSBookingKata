namespace BookingKata.Sales;

public partial class BookingCommandService
{
    public void OpenHotelSeason(int hotelId, int[]? exceptRoomNumbers, DateTime openingDate, DateTime closingDate)
    {
        var originator = this.GetType().FullName;


        var hotelGeoProxy = bus.AskResult<GeoProxy>(Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(hotelId), originator);

        if (hotelGeoProxy == null)
        {
            throw new HotelNotFoundException();
        }


        var roomDetails = bus.AskResult<RoomDetails[]>(Recipient.Admin, Verb.Admin.RequestHotelRoomDetails,
            new RoomDetailsRequest(hotelId, exceptRoomNumbers), originator);

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