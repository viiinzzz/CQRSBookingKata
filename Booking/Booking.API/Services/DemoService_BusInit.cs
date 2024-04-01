namespace BookingKata.API.Demo;

public partial class DemoService
{
    public override void Configure()
    {
        ConnectTo(bus);
    }
}