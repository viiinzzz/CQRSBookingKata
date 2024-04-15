namespace BookingKata.Infrastructure.Common;

public class VerbInvalidException(string verb) : Exception($"Verb '{verb}' is invalid.");