namespace BookingKata.API.Demo;

public record DemoContext
(
    int[] FakeStaffIds,
    int[] FakeManagerIds,
    int[] FakeHotelsIds,
    int[] FakeCustomerIds,
    (int customerId, FakeHelper.FakeCustomer)[] FakeCustomers,

    bool SeedComplete,

    int SimulationDay,

    Hotel[] Hotels,

    long SessionId,
    long ServerId
) : IDemoContext;