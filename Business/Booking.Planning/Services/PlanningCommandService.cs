namespace BookingKata.Planning;

public class PlanningCommandService
(
    IPlanningRepository planning,

    IMessageBus bus,

    IGazetteerService geo,
    ITimeService DateTime,
    IServerContextService server
)
{
    public void Add(ReceptionCheck check)
    {
        var originator = this.GetType().FullName;

        var hotelGeoProxy = bus.AskResult<GeoProxy>(
            originator, Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(check.HotelId));

        if (hotelGeoProxy == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(check.HotelId));
        }

        planning.Add(check);

        geo.CopyToReferer(hotelGeoProxy, check);
    }

    public void Add(RoomServiceDuty duty)
    {
        var originator = this.GetType().FullName;

        var hotelGeoProxy = bus.AskResult<GeoProxy>(
            originator, Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(duty.HotelId));

        if (hotelGeoProxy == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(duty.HotelId));
        }

        planning.Add(duty);

        geo.CopyToReferer(hotelGeoProxy, duty);
    }

    void DoneCheck(int checkId)
    {
        //TODO - authentication
        var currentEmployeeId = 8888_8888;

        planning.DoneCheck(checkId, currentEmployeeId);
    }

    void CancelCheck(int checkId)
    {
        planning.CancelCheck(checkId, DateTime.UtcNow);
    }
    void DoneDuty(int dutyId)
    {
        //TODO - authentication
        var currentEmployeeId = 8888_8888;

        planning.DoneDuty(dutyId, currentEmployeeId);
    }

    void CancelDuty(int dutyId)
    {
        planning.CancelDuty(dutyId, DateTime.UtcNow);
    }
}