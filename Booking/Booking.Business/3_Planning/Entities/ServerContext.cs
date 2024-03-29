namespace BookingKata.Planning;

public record ServerContext
(
    DateTime UtcNow, 
    double UtcNowDayNum,
    long ServerContextId
);
