namespace BookingKata.API;

public class BookingDemoContext
{
    public int[] FakeStaffIds { get; set; }
    public int[] FakeManagerIds { get; set; }
    public int[] FakeHotelsIds { get; set; }
    public int[] FakeCustomerIds { get; set; }
    public Dictionary<int, RandomHelper.FakeCustomer> FakeCustomers { get; } = new();

    public int SimulationDay { get; set; } = 0;
}