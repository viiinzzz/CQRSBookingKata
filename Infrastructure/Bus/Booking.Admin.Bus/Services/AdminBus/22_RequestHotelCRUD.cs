using BookingKata.Admin;
using static BookingKata.Services.Verb;

namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestDisableHotel(IClientNotification notification)
    {
        var request = notification.MessageAs<IdDisable>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        repo.DisableHotel(request.id, request.disable);

        Notify(new Notification(notification.Originator, HotelDisabled)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = request
        });
    }

    private void Verb_Is_RequestModifyHotel(IClientNotification notification)
    {
        var request = notification.MessageAs<IdData<UpdateHotel>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.Update(request.id, request.data);

        Notify(new Notification(notification.Originator, HotelModified)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = ret
        });
    }

    private void Verb_Is_RequestFetchHotel(IClientNotification notification)
    {
        var request = notification.MessageAs<Id>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.GetHotel(request.id);

        Notify(new Notification(notification.Originator, HotelFetched)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = ret
        });
    }
    
    private void Verb_Is_RequestFetchHotelGeoProxy(IClientNotification notification)
    {
        var request = notification.MessageAs<Id>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.GetHotel(request.id);

        var geoProxy = ret?.GetGeoProxy();
        
        Notify(new Notification(notification.Originator, HotelFetched)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = geoProxy
        });
    }

    private void Verb_Is_RequestCreateHotel(IClientNotification notification)
    {
        var request = notification.MessageAs<NewHotel>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.Create(request);

        Notify(new Notification(notification.Originator, HotelCreated)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = ret
        });
    }
}