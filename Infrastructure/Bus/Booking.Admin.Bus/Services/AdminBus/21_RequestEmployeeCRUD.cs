namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestDisableEmployee(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<IdDisable>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var employee = repo.DisableEmployee(request.id, request.disable);

        Notify(new ResponseNotification(notification.Originator, EmployeeDisabled, employee)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }

    private void Verb_Is_RequestModifyEmployee(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<IdData<UpdateEmployee>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var employee = repo.Update(request.id, request.data);

        Notify(new ResponseNotification(notification.Originator, EmployeeModified, employee)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }

    private void Verb_Is_RequestFetchEmployee(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<Id>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var employee = repo.GetEmployee(request.id);

        Notify(new ResponseNotification(notification.Originator, EmployeeFetched, employee)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }

    private void Verb_Is_RequestCreateEmployee(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<NewEmployee>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var employeeId = repo.Create(request);

        var id = new Id(employeeId);

        Notify(new ResponseNotification(notification.Originator, EmployeeCreated, id)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}