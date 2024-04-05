namespace Business.Common;

public interface IRandomService
{
    int Int();
    long Long();
    (long, long) Guid();
}