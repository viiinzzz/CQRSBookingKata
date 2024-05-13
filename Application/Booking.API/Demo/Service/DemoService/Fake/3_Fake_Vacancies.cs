namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_Vacancies()
    {
        try
        {
            var originator = GetType().FullName
                             ?? throw new ArgumentException("invalid originator");

            void createVacancies(int hotelId)
            {
                var message = "Demo: Seeding Vacancies for hotel#{0}...";

                var args = new object[] { hotelId };

                bus.Notify(new AdvertisementNotification(message, args)
                {
                    Originator = originator,
                    Immediate = true
                });


                var season = new OpenHotelSeasonRequest
                {
                    openingDateUtc = DateTime.UtcNow.SerializeUniversal(),
                    closingDateUtc = DateTime.UtcNow.AddDays(SeasonDayNumbers).SerializeUniversal(),

                    exceptRoomNumbers = default,
                    hotelId = hotelId
                };

                var opening = bus.AskResult<HotelOpening>(Recipient.Sales, Verb.Sales.RequestOpenHotelSeason,
                    season,
                    originator);

                if (opening == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(opening));
                }
            }

            demoContext.FakeHotelsIds
                .AsParallel()
                .ForAll(createVacancies);
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Vacancies)}' failed", e);
        }
    }
}