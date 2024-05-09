namespace VinZ.Common;

public record PageResult<TEntity> 
(
    int page,
    int pageSize,

    bool error,
    string? reason,

    int pageCount,
    int itemCount,

    PageLinks[] links
)
    : IHaveCollection<TEntity>
    
    where TEntity : class
{
    private static string Get_type()
    {
        if (typeof(TEntity).IsInterface)
        {
            throw new ArgumentException($"Invalid generic type {typeof(TEntity).Name}, must not be an interface.", nameof(TEntity));
        }

        return typeof(PageResult<TEntity>).FullName;
    }

    public string? _type { get; } = Get_type();
    public string? _itemType { get; } = typeof(TEntity).Name;

    public TEntity[]? items { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public IEnumerable<TEntity> Collection
    {
        get => this.GetCollection();
        set => this.SetCollection(value);
    }
}