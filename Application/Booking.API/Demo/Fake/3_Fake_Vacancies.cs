namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_Vacancies(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            foreach (var hotelId in demo.FakeHotelsIds)
            {
                bus.Notify(new Notification(Recipient.Audit, Verb.Audit.Information)
                {
                    Message = $"Demo: Seeding Vacancies for hotel#{hotelId}...",
                    Immediate = true
                });

                booking.OpenHotelSeason(
                    hotelId, default,
                    DateTime.UtcNow, DateTime.UtcNow.AddDays(SeasonDayNumbers), scoped: false);
            }

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Vacancies)}' failed", e);
        }
    }
}