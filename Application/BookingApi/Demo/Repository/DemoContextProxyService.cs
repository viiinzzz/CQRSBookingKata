namespace BookingKata.API.Demo;

public class DemoContextProxyService(IMessageBus mq) : IDemoContext
{
    private DemoContext GetContext()
    {
        var context = mq.Ask<DemoContext>(
                                  nameof(ServerContextProxyService), Recipient.Demo, Verb.Demo.RequestDemoContext, null,
                                  CancellationToken.None, 30)
                              ?? throw new NullReferenceException();

        context.Wait();

        return context.Result
               ?? throw new NullReferenceException();
    }
    
    public int[] FakeStaffIds => GetContext().FakeStaffIds;

    public int[] FakeManagerIds => GetContext().FakeManagerIds;

    public int[] FakeHotelsIds => GetContext().FakeHotelsIds;

    public int[] FakeCustomerIds => GetContext().FakeCustomerIds;

    public (int customerId, FakeHelper.FakeCustomer)[] FakeCustomers => GetContext().FakeCustomers;

    public bool SeedComplete => GetContext().SeedComplete;

    public int SimulationDay => GetContext().SimulationDay;
}