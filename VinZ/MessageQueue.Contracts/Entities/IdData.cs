namespace VinZ.Common;

public record IdData<TData>(int id, TData data) where TData : class;