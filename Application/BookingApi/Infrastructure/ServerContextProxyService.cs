namespace BookingKata.API.Infrastructure;

public class ServerContextProxyService(IMessageBus mq)
    : IServerContextService
{
    private ServerContext? GetContext()
    {
            var serverContext = mq.Ask<ServerContext>(
                    nameof(ServerContextProxyService), [nameof(ServerContextProxyService)],
                    Recipient.Admin, Verb.Admin.RequestServerContext, null, 
                    CancellationToken.None, 30)
                .Result;

            return serverContext;
    } 
    
    public long Id => GetContext()?.ServerContextId ?? 0;
    
    public long SessionId => GetContext()?.SessionId ?? 0;
}