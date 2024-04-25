namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestDisableHotel(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<IdDisable>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var hotel = repo.DisableHotel(request.id, request.disable);

        Notify(new ResponseNotification(notification.Originator, HotelDisabled, hotel)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }

    private void Verb_Is_RequestModifyHotel(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<IdData<UpdateHotel>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var hotel = repo.Update(request.id, request.data);

        Notify(new ResponseNotification(notification.Originator, HotelModified, hotel)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }

    private void Verb_Is_RequestFetchHotel(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<Id>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var hotel = repo.GetHotel(request.id);

        Notify(new ResponseNotification(notification.Originator, HotelFetched, hotel)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
    
    private void Verb_Is_RequestFetchHotelGeoProxy(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<Id>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.GetHotel(request.id);

        var geoProxy = ret?.GetGeoProxy();
        
        Notify(new ResponseNotification(notification.Originator, HotelGeoProxyFetched, geoProxy)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }

    private void Verb_Is_RequestCreateHotel(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<NewHotel>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var hotelId = repo.Create(request);

        var id = new Id(hotelId);

        Notify(new ResponseNotification(notification.Originator, HotelCreated, id)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}