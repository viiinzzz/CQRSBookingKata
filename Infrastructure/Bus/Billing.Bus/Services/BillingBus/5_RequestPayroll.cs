namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestPayroll(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PayrollRequest>();

        using var scope = sp.GetScope<IBillingCommandService>(out var billing);

        //
        //
        var payrollId = billing.EnrollEmployee
        (
            request.employeeId,
            request.monthlyIncome,
            request.currency,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        Notify(new ResponseNotification(Omni, PayrollEmitted, payrollId)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}