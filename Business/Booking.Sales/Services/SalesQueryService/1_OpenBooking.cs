namespace BookingKata.Sales;

public partial class SalesQueryService
{
    public long[] OpenBooking(UniqueRoomId urid, DateTime openingDate, DateTime closingDate, int personCount)
    {
        var originator = GetType().FullName
                         ?? throw new Exception("invalid originator");

        var roomDetails = bus.AskResult<RoomDetails>(Recipient.Admin, Verb.Admin.RequestSingleRoomDetails,
            new Id(urid.Value), originator);

        if (roomDetails == default)
        {
            new ArgumentException(ReferenceInvalid, nameof(urid));
        }


        var hotelGeoProxy = bus.AskResult<GeoProxy>(Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(urid.HotelId), originator);

        if (hotelGeoProxy?.Cells is null or { Count: 0 })
        {
            throw new HotelNotGeoIndexedException();
        }

        var (nearestKnownCity, nearestKnownCityKm) = geo.NearestCity(hotelGeoProxy.Cells.TopCell(), NearestKnownCityMaxKm);


        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var vacancies = firstNight
            .StayUntil(lastNight, personCount, roomDetails.Latitude, roomDetails.Longitude,
                roomDetails.HotelName, nearestKnownCity?.name, urid.Value)
            .ToArray();

        geo.CopyToReferers(hotelGeoProxy, vacancies);

        //
        //
        sales.AddVacancies(vacancies);
        //
        //

        return vacancies
            .Select(vacancy => vacancy.VacancyId)
            .ToArray();
    }
}