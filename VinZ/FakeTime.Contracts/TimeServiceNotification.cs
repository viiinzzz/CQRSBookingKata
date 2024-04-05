namespace VinZ.Common;

public record TimeServiceNotification
(
    string verb, 
    DateTime UtcNow,
    bool Freeze, 
    bool Fake, 
    string state
);