namespace VinZ.Common;

public static class MessageQueueNetCoreHelper
{
    public static IServiceCollection AddMessageQueue
    (
        this IServiceCollection services,
        
        BusConfiguration busConfig,
        Type[] busTypes,

        bool pauseOnError
    )
    {
        services.AddSingleton(busConfig);

        foreach (var busType in busTypes)
        {
            services.AddSingleton(busType);

            // var logBusType = typeof(ILogger<>).MakeGenericType(busType);
            // services.AddTransient(logBusType);
            // services.AddSingleton(sp =>
            // {
            //     var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(busType);
            //     return logger;
            // });
        }

        services.AddSingleton(_ => new MqServerConfig
        {
            DomainBusTypes = busTypes,
            PauseOnError = pauseOnError,
            IsTrace = busConfig.IsTrace
        });

        services.AddSingleton<MqServer>();
        services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<MqServer>());
        services.AddHostedService(sp => sp.GetRequiredService<MqServer>());

        return services;
    }

    public static RouteHandlerBuilder MapListMq<TEntity>
    (
        this RouteGroupBuilder builder,

        string pattern, string uri, object filter,
        string recipient, string verb,
        string originator,
        int responseTimeoutSeconds
    )
        where TEntity : class
    {
        return builder.MapGet(pattern,
            originator.ListMq<TEntity>(recipient, verb, pattern, uri, filter, responseTimeoutSeconds)
        );
    }

    public static RouteHandlerBuilder MapPostMq<TNew>
    (
        this RouteGroupBuilder builder,
        
        string pattern,
        string recipient, string verb,
        string originator,
        int responseTimeoutSeconds)
        where TNew : class
    {
        return builder.MapPost(pattern,
            originator.PostMq<TNew>(recipient, verb, responseTimeoutSeconds)
        );
    }

    public static RouteHandlerBuilder MapGetMq<TEntity>
    (
        this RouteGroupBuilder builder,
        
        string pattern,
        string recipient, string verb,
        string originator,
        int responseTimeoutSeconds)
        where TEntity : class
    {
        return builder.MapGet(pattern,
            originator.GetMq<TEntity>(recipient, verb, responseTimeoutSeconds)
        );
    }

    public static RouteHandlerBuilder MapPatchMq<TUpdate>
    (
        this RouteGroupBuilder builder,
        
        string pattern,
        string recipient, string verb,
        string originator,
        int responseTimeoutSeconds)
        where TUpdate : class
    {
        return builder.MapPatch(pattern,
            originator.PatchMq<TUpdate>(recipient, verb, responseTimeoutSeconds)
        );
    }

    public static RouteHandlerBuilder MapDisableMq<TEntity>
    (
        this RouteGroupBuilder builder, 
        
        string pattern,
        string recipient, string verb,
        string originator,
        int responseTimeoutSeconds)
        where TEntity : class
    {
        return builder.MapDelete(pattern,
            originator.DisableMq<TEntity>(recipient, verb, responseTimeoutSeconds)
        );
    }


    public static Func<int?, int?, IMessageBus, CancellationToken, Task<PageResult<TEntity>>?> ListMq<TEntity>
    (
        this string originator, 
        
        string recipient, string verb,
        string pattern, string uri, object filter,
        int responseTimeoutSeconds
    )
        where TEntity : class
    {
        return async (
                int? page, int? pageSize,
                [FromServices] IMessageBus mq, CancellationToken requestCancel
            )
            =>
        {
            var pageRequest = new PageRequest(uri, page, pageSize, filter);

            var ret = await mq.Ask<PageResult<TEntity>>(
                originator, recipient, verb, pageRequest,
                requestCancel, responseTimeoutSeconds);

            if (ret == null)
            {
                throw new Exception("Internal Server Error");
            }

            return ret;
        };
    }

    public static Func<TNew, IMessageBus, CancellationToken, Task<Id>> PostMq<TNew>
    (
        this string originator,
        
        string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TNew : class
    {
        return async (
                [FromBody] TNew post,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var id = await mq.Ask<Id>(
                originator, recipient, verb, post,
                requestCancel, responseTimeoutSeconds);

            return id;
        };
    }

    public static Func<int, IMessageBus, CancellationToken, Task<IResult>> GetMq<TEntity>
    (
        this string originator,
        
        string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TEntity : class
    {
        return async (
                int id,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var entity = await mq.Ask<TEntity>(
                originator, recipient, verb, new Id(id), 
                requestCancel, responseTimeoutSeconds);

            return entity.AsResult();
        };
    }

    public static Func<int, TUpdate, IMessageBus, CancellationToken, Task<IResult>> PatchMq<TUpdate>
    (
        this string originator,
        
        string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TUpdate : class
    {
        return async (
                int id,
                [FromBody] TUpdate patch,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var ret = await mq.AskObject(originator, recipient, verb,
                new IdData<TUpdate>(id, patch),
                requestCancel, responseTimeoutSeconds);

            return ret.AsAccepted();
        };
    }


    public static Func<int, bool?, IMessageBus, CancellationToken, Task<IResult>> DisableMq<TEntity>
    (
        this string originator,
        
        string recipient, string verb,
        int responseTimeoutSeconds
    ) 
        where TEntity : class
    {
        return async
            (
                int id,
                bool? disable,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            ) => 
        {
            var ret = await mq.Ask<TEntity>(
                originator, recipient, verb, new IdDisable(id, disable ?? true), 
                requestCancel, responseTimeoutSeconds);

            return ret.AsAccepted();
        };
    }
}