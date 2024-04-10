namespace VinZ.Common;

public record Id(int id);

public record IdDisable(int id, bool disable) : Id(id);

public record IdData<TData>(int id, TData data) where TData : class;