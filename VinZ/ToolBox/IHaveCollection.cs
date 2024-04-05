namespace VinZ.Common;

public interface IHaveCollection<T>
{
    IEnumerable<T> Collection { get; set; }
}