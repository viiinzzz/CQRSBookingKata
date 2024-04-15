namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestDisableEmployee(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<IdDisable>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        repo.DisableEmployee(request.id, request.disable);

        Notify(new Notification(notification.Originator, EmployeeDisabled)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = request
        });
    }

    private void Verb_Is_RequestModifyEmployee(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<IdData<UpdateEmployee>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.Update(request.id, request.data);

        Notify(new Notification(notification.Originator, EmployeeModified)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = ret
        });
    }

    private void Verb_Is_RequestFetchEmployee(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<Id>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.GetEmployee(request.id);

        Notify(new Notification(notification.Originator, EmployeeFetched)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = ret
        });
    }

    private void Verb_Is_RequestCreateEmployee(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<NewEmployee>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.Create(request);

        Notify(new Notification(notification.Originator, EmployeeCreated)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = ret
        });
    }
}