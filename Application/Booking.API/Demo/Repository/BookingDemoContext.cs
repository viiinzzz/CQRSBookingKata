namespace BookingKata.API.Demo;

public class BookingDemoContext
{
    public int[] FakeStaffIds { get; set; }
    public int[] FakeManagerIds { get; set; }
    public int[] FakeHotelsIds { get; set; }
    public int[] FakeCustomerIds { get; set; }
    public Dictionary<int, FakeHelper.FakeCustomer> FakeCustomers { get; } = new();

    public int SimulationDay { get; set; } = 0;
}
