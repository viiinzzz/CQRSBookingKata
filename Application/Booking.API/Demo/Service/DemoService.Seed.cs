namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Seed()
    {
        var context = new TransactionContext() * admin * money * sales * geo;

        //admin setup
        context.Execute(() => Fake_Employees(false));
        context.Execute(() => Fake_Hotels(false));
        context.Execute(() => Fake_Vacancies(false));

        //sales setup
        context.Execute(() => Fake_Customers(false));


        // DateTime.Unfreeze();
    }




    public int SimulationDay => demo.SimulationDay;

    private const int DayMilliseconds = 24 * 60 * 1000;
    public const double SpeedFactorOneDayOneMinute = 24 * 60;
    public async Task<DateTime> Forward(int days, double? speedFactor, CancellationToken cancellationToken)
    {
        try
        {
            var context = new TransactionContext() * admin * money * sales * geo;

            for (var d = 0; d < days; d++)
            {
                var milliseconds = (int)(DayMilliseconds / (speedFactor ?? SpeedFactorOneDayOneMinute));
                if (milliseconds < 1000) milliseconds = 1000;

                await Task.Delay(milliseconds, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                DateTime.Forward(TimeSpan.FromDays(1));
                demo.SimulationDay++;

                context.Execute(() => Fake_BookingDay());

                if (days == SeasonDayNumbers)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            var childNotification = new RequestNotification(nameof(Demo), nameof(Forward));

            bus.Notify(new NegativeResponseNotification(childNotification, ex, "aborted!")
            {
                Originator = originator,
                Immediate = true
            });
        }

        return DateTime.UtcNow;
    }
}