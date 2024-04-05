namespace VinZ.Common;

public class ServerContextService : IServerContextService
{
    public ServerContextService()
    {
        Id = UuidHelper.GetUuidInt64();
    }

    public long Id { get; }
}

