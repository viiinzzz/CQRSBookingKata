namespace VinZ.ToolBox;

public interface IHaveCollection<T>
{
    IEnumerable<T> Collection { get; set; }
}