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

            foreach (var hotelId in demoContext.FakeHotelsIds)
            {
                var message = "Demo: Seeding Vacancies for hotel#{0}...";

                var args = new object[] { hotelId };

                bus.Notify(new AdvertisementNotification(message, args)
                {
                    Originator = originator,
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