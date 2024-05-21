namespace BookingKata.API.Demo;

public interface IDemoContext
{
    public int[] FakeStaffIds { get;  }
    public int[] FakeManagerIds { get;  }
    public int[] FakeHotelsIds { get; }
    public int[] FakeCustomerIds { get; }
    public (int customerId, FakeHelper.FakeCustomer)[] FakeCustomers { get; }

    public bool SeedComplete { get; }

    public int SimulationDay { get; }
}