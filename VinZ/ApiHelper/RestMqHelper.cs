using BookingKata.Infrastructure.Common;

namespace VinZ.Common;

public static class RestMqHelper
{
    public static RouteHandlerBuilder MapListMq<TEntity>
    (
        this RouteGroupBuilder builder,
        string pattern, string uri, string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TEntity : class
    {
        return builder.MapGet(pattern,
            (recipient, verb).ListMq<TEntity>(pattern, uri, responseTimeoutSeconds)
        );
    }

    public static RouteHandlerBuilder MapPostMq<TNew>
    (
        this RouteGroupBuilder builder,
        string pattern, string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TNew : class
    {
        return builder.MapPost(pattern,
            (recipient, verb).PostMq<TNew>(responseTimeoutSeconds)
        );
    }

    public static RouteHandlerBuilder MapGetMq<TEntity>
    (
        this RouteGroupBuilder builder,
        string pattern, string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TEntity : class
    {
        return builder.MapGet(pattern,
            (recipient, verb).GetMq<TEntity>(responseTimeoutSeconds)
        );
    }

    public static RouteHandlerBuilder MapPatchMq<TUpdate>
    (
        this RouteGroupBuilder builder,
        string pattern, string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TUpdate : class
    {
        return builder.MapPatch(pattern,
            (recipient, verb).PatchMq<TUpdate>(responseTimeoutSeconds)
        );
    }

    public static RouteHandlerBuilder MapDisableMq<TEntity>
    (
        this RouteGroupBuilder builder,
        string pattern, string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TEntity : class
    {
        return builder.MapDelete(pattern,
            (recipient, verb).DisableMq<TEntity>(responseTimeoutSeconds)
        );
    }


    public static Func<int?, int?, IMessageBus, CancellationToken, Task<PageResult<TEntity>>?>
        ListMq<TEntity>
        (
            this (string recipient, string verb) notification,
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
            var ret = await mq.Ask<PageResult<TEntity>>(notification.recipient, notification.verb,
                new PageRequest(uri, page, pageSize, filter),
                requestCancel, responseTimeoutSeconds);

            return ret;
        };
    }

    public static Func<TNew, IMessageBus, CancellationToken, Task<Id>>
        PostMq<TNew>
        (
            this (string recipient, string verb) notification,
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
            var ret = await mq.Ask(notification.recipient, notification.verb,
                post,
                requestCancel, responseTimeoutSeconds);

            if (ret is not int id)
            {
                throw new InvalidCastException("not an id");
            }

            return new Id(id);
        };
    }

    public static Func<int, IMessageBus, CancellationToken, Task<IResult>>
        GetMq<TEntity>
        (
            this (string recipient, string verb) notification,
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
            var entity = await mq.Ask<TEntity>(notification.recipient, notification.verb,
                new Id(id),
                requestCancel, responseTimeoutSeconds);

            return entity.AsResult();
        };
    }

    public static Func<int, TUpdate, IMessageBus, CancellationToken, Task<IResult>>
        PatchMq<TUpdate>
        (
            this (string recipient, string verb) notification,
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
            var ret = await mq.Ask(notification.recipient, notification.verb,
                new IdData<TUpdate>(id, patch),
                requestCancel, responseTimeoutSeconds);

            return ret.AsAccepted();
        };
    }


    public static Func<int, bool?, IMessageBus, CancellationToken, Task<IResult>>
        DisableMq<TEntity>
        (
            this (string recipient, string verb) notification,
            int responseTimeoutSeconds
        ) 
        where TEntity : class
    {
        return async (
                int id,
                bool? disable,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var ret = await mq.Ask<TEntity>(notification.recipient, notification.verb,
                new IdDisable(id, disable ?? true),
                requestCancel, responseTimeoutSeconds);

            return ret.AsAccepted();
        };
    }
}