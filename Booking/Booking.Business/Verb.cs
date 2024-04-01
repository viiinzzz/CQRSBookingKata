namespace BookingKata;

public static class Verb
{
    public class Sales
    {
        public const string NewBooking = $"{nameof(Booking)}:{nameof(NewBooking)}";
    }

    public class Audit
    {
        public const string Error = $"{nameof(Audit)}:{nameof(Error)}";
        public const string Warning = $"{nameof(Audit)}:{nameof(Warning)}";
        public const string Information = $"{nameof(Audit)}:{nameof(Information)}";
    }
}