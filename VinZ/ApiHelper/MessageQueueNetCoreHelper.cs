/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace VinZ.Common;

public record MessageQueueConfiguration
(
    LogLevel logLevel = LogLevel.Trace,
    Uri busUrl = default,
    string? messageQueueUrl = default,
    Type[] busTypes = default,
    bool pauseOnError = default,
    MqEffects? effects = default
);

public class AddClientsService
(
    IScopeProvider scp
)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope0 = scp.GetScope<MessageQueueConfiguration>(out var mqConfig);
        var scope1 = scp.GetScope<ILogger<IMessageBus>>(out var log);

        var busTypes = mqConfig.busTypes;
        var logLevel = mqConfig.logLevel;

        await Task.WhenAll(busTypes.Select(async busType =>
        {
            if (logLevel <= LogLevel.Trace)
            {
                log.LogInformation(
                    $"Connecting {busType.Name} to remote bus {mqConfig.busUrl}...");
            }

            var scope2 = scp.GetScope(busType, out var domainBus);
            var client = (IMessageBusClient)domainBus;

            client.Log = log;

            //initial wait for master bus to be ready
            for (var i = 0; i < 6; i++)
            {
                await Task.Delay(10_000);

                log.Log(LogLevel.Warning, $"Delayed connection to bus {i * 10}s");

                stoppingToken.ThrowIfCancellationRequested();
            }

            //
            client.ConnectToBus(scp);
            //

            return client
                .Configure()
                .ContinueWith(prev =>
                {
                    if (logLevel <= LogLevel.Trace)
                    {
                        log.LogInformation(
                            $"<<<{busType.Name}:{client.GetHashCode().xby4()}>>> Connected.");
                    }
                }, stoppingToken);
        }));
    }
}

public static class MessageQueueNetCoreHelper
{

    public static IServiceCollection AddMessageQueue
    (
        this IServiceCollection services,
        MessageQueueConfiguration mqConfig,
        out string messageQueueUrl,
        out string[] addedBus
    )
    {
        if (mqConfig.messageQueueUrl == null)
        {
            Console.Error.WriteLine("Error: MessageQueueUrl undefined. (check appsettings.json)");

            Environment.Exit(-1);
        }

        var isLocalMessageQueue = mqConfig.messageQueueUrl == "self";

        var busConfig = new BusConfiguration
        {
            LocalUrl = mqConfig.busUrl.ToString(),

            RemoteUrl = isLocalMessageQueue
                ? mqConfig.busUrl.ToString() 
                : new Uri(mqConfig.messageQueueUrl).ToString()
        };

        messageQueueUrl = busConfig.RemoteUrl;


        services.AddSingleton(busConfig);

        var addedBusList = new List<string>();

        foreach (var busType in mqConfig.busTypes)
        {
            addedBusList.Add($"{busType.Name}");

            services.AddSingleton(busType);

            // var logBusType = typeof(ILogger<>).MakeGenericType(busType);
            // services.AddTransient(logBusType);
            // services.AddSingleton(sp =>
            // {
            //     var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(busType);
            //     return logger;
            // });
        }

        if (isLocalMessageQueue)
        {
            addedBusList.Add($"{nameof(MqServer)}");

            services.AddSingleton<MqEffects>(sp => mqConfig.effects);

            services.AddSingleton(_ => new MqServerConfig
            {
                DomainBusTypes = mqConfig.busTypes,
                PauseOnError = mqConfig.pauseOnError,
                logLevel = mqConfig.logLevel
            });

            services.AddSingleton<MqServer>();
            services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<MqServer>());
            services.AddHostedService(sp => sp.GetRequiredService<MqServer>());

            addedBus = [.. addedBusList];

            return services;
        }

        //remote message queue

        services.AddScoped<IMessageBus>(sp =>
        {
            var log = sp.GetRequiredService<ILogger<IMessageBus>>();
            var dateTime = sp.GetRequiredService<ITimeService>();
            var appConfig = sp.GetRequiredService<IConfiguration>();

            var remoteBus = new MessageBusHttp(busConfig, appConfig, dateTime, log);

            return remoteBus;
        });

        services.AddHostedService<AddClientsService>();

        addedBus = [.. addedBusList];

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


    public static Func<int?, int?, string[]?, IMessageBus, CancellationToken, Task<PageResult<TEntity>>?> ListMq<TEntity>
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
                [FromHeader] string[]? _steps,
                [FromServices] IMessageBus mq, CancellationToken requestCancel
            )
            =>
        {
            var pageRequest = new PageRequest(uri, page, pageSize, filter);

            var ret = await mq.Ask<PageResult<TEntity>>(
                originator, _steps ?? [],
                recipient, verb, pageRequest,
                requestCancel, responseTimeoutSeconds);

            if (ret == null)
            {
                throw new Exception("Internal Server Error");
            }

            return ret;
        };
    }

    public static Func<TNew, string[]?, IMessageBus, CancellationToken, Task<Id<TNew>>> PostMq<TNew>
    (
        this string originator,
        
        string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TNew : class
    {
        return async (
                [FromBody] TNew post,
                [FromHeader] string[]? _steps,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var id = await mq.Ask<Id<TNew>>(
                originator, _steps ?? [],
                recipient, verb, post,
                requestCancel, responseTimeoutSeconds);

            return id;
        };
    }

    public static Func<int, string[]?, IMessageBus, CancellationToken, Task<IResult>> GetMq<TEntity>
    (
        this string originator, 
        
        string recipient, string verb,
        int responseTimeoutSeconds
    )
        where TEntity : class
    {
        return async (
                int id,
                [FromHeader] string[]? _steps,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var entity = await mq.Ask<TEntity>(
                originator, _steps ?? [],
                recipient, verb, new Id<TEntity>(id), 
                requestCancel, responseTimeoutSeconds);

            return entity.AsResult();
        };
    }

    public static Func<int, TUpdate, string[]?, IMessageBus, CancellationToken, Task<IResult>> PatchMq<TUpdate>
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
                [FromHeader] string[]? _steps,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var ret = await mq.AskObject(
                originator, _steps ?? [],
                recipient, verb,
                new IdData<TUpdate>(id, patch),
                requestCancel, responseTimeoutSeconds);

            return ret.AsAccepted();
        };
    }


    public static Func<int, bool?, string[]?, IMessageBus, CancellationToken, Task<IResult>> DisableMq<TEntity>
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
                [FromHeader] string[]? _steps,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            ) => 
        {
            var ret = await mq.Ask<TEntity>(
                originator, _steps ?? [],
                recipient, verb, new IdDisable<TEntity>(id, disable ?? true), 
                requestCancel, responseTimeoutSeconds);

            return ret.AsAccepted();
        };
    }
}