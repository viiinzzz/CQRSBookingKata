namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_Vacancies(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var originator = GetType().FullName
                             ?? throw new ArgumentException("invalid originator");

            foreach (var hotelId in demo.FakeHotelsIds)
            {
                bus.Notify(originator, new AdvertisementNotification(Recipient.Audit)
                {
                    Message = $"Demo: Seeding Vacancies for hotel#{hotelId}...",
                    Immediate = true
                });

                booking.OpenHotelSeason(
                    hotelId, default,
                    DateTime.UtcNow, DateTime.UtcNow.AddDays(SeasonDayNumbers));
            }

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Vacancies)}' failed", e);
        }
    }
}