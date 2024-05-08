namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private const int DayMilliseconds = 24 * 60 * 1000;
    public const double SpeedFactorOneDayOneMinute = 24 * 60;
    private readonly object Fake_BookingDay_lock = new();

    private void Verb_Is_RequestTimeForward(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<TimeForwardRequest>();

        /*   using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var context = new TransactionContext() * admin * money * sales * geo;

            for (var d = 0; d < days; d++)
            {
                var milliseconds = (int)(DayMilliseconds / (speedFactor ?? SpeedFactorOneDayOneMinute));
                if (milliseconds < 1000) milliseconds = 1000;

                await Task.Delay(milliseconds, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                DateTime.Forward(TimeSpan.FromDays(1));
                demoContext.SimulationDay++;

                context.ExecuteExclusive(() => Fake_BookingDay(), Fake_BookingDay_lock);

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
     */
    }
}