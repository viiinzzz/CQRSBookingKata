namespace BookingKata.Infrastructure.Common;

public record PageRequest(string Path, int? Page, int? PageSize);