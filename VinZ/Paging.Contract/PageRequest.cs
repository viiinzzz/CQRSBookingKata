namespace VinZ.Common;

public record PageRequest
(
    string Path,
    int? Page,
    int? PageSize, 
    object? Filter = default
);
