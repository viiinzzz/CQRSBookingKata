using VinZ.Common;

namespace VinZ.ServerContext;

public class ServerContextService : IServerContextService
{
    public ServerContextService()
    {
        Id = UuidHelper.GetUuidInt64();
    }

    public long Id { get; }
}

