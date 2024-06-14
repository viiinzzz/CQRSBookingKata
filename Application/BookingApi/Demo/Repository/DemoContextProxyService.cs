namespace BookingKata.API.Demo;

public class DemoContextProxyService(IMessageBus mq, IServerContextService serverContext) : IDemoContext
{
    public long ServerId => serverContext.ServerId;
    public long SessionId => serverContext.SessionId;

    private DemoContext GetContext()
    {
        try
        {
            var context = mq.Ask<DemoContext>(
                    nameof(ServerContextProxyService), [nameof(DemoContextProxyService)],
                    Recipient.Demo, Verb.Demo.RequestDemoContext, null,
                    CancellationToken.None, 30)
                .Result ?? throw new NullReferenceException();

            System.Console.WriteLine(@$"=================================DemoContextProxyService GetContext
{context.ToJson(true)}");

            return context;
        }
        catch (Exception e)
        {
            System.Console.WriteLine(@$"=================================DemoContextProxyService GetContext failed.
{e.Message}
{e.StackTrace}");

            return new DemoContext([], [], [], [], [], false, 0, [], 0, 0);
        }
    }
    
    public int[] FakeStaffIds => GetContext().FakeStaffIds;

    public int[] FakeManagerIds => GetContext().FakeManagerIds;

    public int[] FakeHotelsIds => GetContext().FakeHotelsIds;

    public int[] FakeCustomerIds => GetContext().FakeCustomerIds;

    public (int customerId, FakeHelper.FakeCustomer)[] FakeCustomers => GetContext().FakeCustomers;

    public bool SeedComplete => GetContext().SeedComplete;

    public int SimulationDay => GetContext().SimulationDay;

    private Hotel[] GetHotels()
    {
        try
        {
            var hotels = mq.Ask<Hotel[]>(
                         nameof(ServerContextProxyService), [nameof(DemoContextProxyService)],
                         Recipient.Demo, Verb.Demo.RequestDemoHotels, null, 
                         CancellationToken.None, 30)
            .Result ?? throw new NullReferenceException();

            return hotels;
        }
        catch (Exception e)
        {
            System.Console.WriteLine(@$"=================================DemoContextProxyService GetHotels failed.
{e.Message}
{e.StackTrace}");

            return [];
        }
    }

    public Hotel[] Hotels => GetHotels();
}