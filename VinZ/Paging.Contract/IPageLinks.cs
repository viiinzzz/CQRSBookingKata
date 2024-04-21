// ReSharper disable InconsistentNaming
namespace VinZ.Common;

public interface IPageLinks
{
    string url { get; }
    string? next { get; }
    string? prev { get; }
}