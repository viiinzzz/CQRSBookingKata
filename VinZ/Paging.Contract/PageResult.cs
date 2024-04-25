namespace VinZ.Common;

public record PageResult<TEntity>
(
    string type, string elementType,
    int page, int pageSize,

    bool error, string? reason,
    int pageCount, int itemCount,
    PageLinks[] links
)
    : IHaveCollection<TEntity>
{
    public TEntity[]? items { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public IEnumerable<TEntity> Collection
    {
        get => this.GetCollection();
        set => this.SetCollection(value);
    }
}