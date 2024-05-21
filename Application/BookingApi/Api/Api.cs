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
   * /

/*╭----------------------------------------------------------------------------╮
  ╎                                                                            ╎
  ╎                             B O O K I N G  API                             ╎
  ╎                                                                            ╎
  ╰----------------------------------------------------------------------------╯*/



/*╭-----------------------------------------------------------------------------
  ╎ basic variables
  */

using Booking.API.Infrastructure;

var pif = ProgramInfo.Get();

var pauseOnError = pif.IsDebug && pif.IsTrueEnv("DEBUG_PAUSE_ON_ERROR");

var demoMode = pif.IsTrueEnv("DEMO_MODE");

var isMainContainer = pif.Env is "Development" or "Production-App";

var busUrl = ApiHelper.GetAppUrlPrefix("bus");

var myIps = ApiHelper.GetMyIps();

var Console = new cons.AnsiVtConsole(); //https://github.com/franck-gaspoz/AnsiVtConsole.NetCore

{
    var exeStr = pif.ExeName.Length > 0 || pif.ExeVersion.Length > 0 ? $"(bon){pif.ExeName} {pif.ExeVersion}(rdc)" : "";
    var buildArchiStr = $"(f=darkgray)Build {pif.BuildConfiguration} {pif.ProcessArchitecture}(rdc)";
    var osFwStr = $"(f=darkgray){pif.Os} {pif.Framework}(rdc)";
    var envStr = $"(invon,f={(pif.IsRelease ? "magenta" : "cyan")}){pif.Env}(rdc)";
    var pauseStr = pauseOnError ? " " + $"(invon,f=yellow)PauseOnError(rdc)" : "";

    Console.Out.WriteLine(@$"{exeStr} {envStr}{pauseStr}
{buildArchiStr} - {osFwStr} 

(f=darkgray)Network:(rdc)
{string.Join(Environment.NewLine, myIps.Select(ip => $"(uon)http://{ip}:{busUrl.Port}(rdc)"))}
(uon){busUrl}(rdc)
");
}

var demoStr = () => demoMode ? "" + $"(invon,f=yellow)DemoMode(rdc)" : "";

const double precisionMaxKm = 0.5;

const int dbContextKeepAliveMilliseconds = 30_000;

/*
                                                                              ╎
 -----------------------------------------------------------------------------╯*/





/*╭-----------------------------------------------------------------------------
  ╎ DI methods
  */

//adding components to the magic wiring box, aka. DI Container to achieve IoC

void ConfigureDependencyInjection(WebApplicationBuilder builder, out Action<WebApplication> configureApplication)
{
    //database
    var logLevelEFContext = builder.EnumConfiguration("Logging:LogLevel:Microsoft.EntityFrameworkCore.DbContext",
        "Logging:LogLevel:Default", LogLevel.Warning);
    var dbContextTypes = builder.GetConfigurationTypes("Api:DbContext", Dependencies.AvailableDbContextTypes);
    builder.RegisterDbContexts(dbContextTypes, pif.IsDebug, logLevelEFContext);

    configureApplication = app =>
        app.EnsureDatabaseCreated(dbContextTypes, logLevelEFContext, pif.IsDebug ? dbContextKeepAliveMilliseconds : null);


    var services = builder.Services;

    //infra
    services.AddSingleton<IScopeProvider, ScopeProvider>();
    services.AddExceptionHandler<GlobalExceptionHandler>();

    //web
    services.AddRazorPages();

    //open api doc
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    //security
    services.AddAntiforgery();
    services.AddCors();
    services.AddAuthentication().AddJwtBearer();
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-8.0

    //app services
    services.AddSingleton<ITimeService, TimeService>();
    services.AddSingleton<IRandomService, RandomService>();



    if (pif.Env is "Development" or "Production-Admin")
    {
        services.AddSingleton<IServerContextService, ServerContextService>();
    }
    else
    {
        services.AddSingleton<IServerContextService, ServerContextProxyService>();
    }

    //bus
    var mqConfig = new MessageQueueConfiguration
    {
        busUrl = busUrl,
        messageQueueUrl = Environment.GetEnvironmentVariable("MESSAGEQUEUE_URL") ?? builder.GetConfigurationValue("Api:MessageQueueUrl"),
        busTypes = builder.GetConfigurationTypes("Api:Bus", Dependencies.AvailableBusTypes).ToArray(),
        pauseOnError = pauseOnError
    };

    services.AddMessageQueue(mqConfig, out var messageQueueUrl, out var addedBus);

    Console.Out.WriteLine(@$"
(f=darkgray)MessageQueue: {string.Join(", ", addedBus)}(rdc)
(uon){messageQueueUrl}(rdc)
");

    //repo
    builder.AddScopedConfigurationTypes("Api:Repository", Dependencies.AvailableRepositories);

    //business/support/third-party
    builder.AddScopedConfigurationTypes("Api:Service", Dependencies.AvailableServices);

    var bookingConfig = new BookingConfiguration
    {
        PrecisionMaxKm = precisionMaxKm
    };
    services.AddSingleton(bookingConfig);

    //metrics
    services.AddObservability();

    //demo
    demoMode = demoMode || builder.IsTrueConfiguration("Api:DemoMode");

    Console.Out.WriteLine(@$"{demoStr()}");

    if (demoMode)
    {
        services.AddSingleton<DemoContextService>();
        services.AddSingleton<IDemoContext>(sp => sp.GetRequiredService<DemoContextService>());

        services.AddSingleton<DemoBus>();

        services.AddHostedService<DemoHostService>();
        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });
    }
    else
    {
        services.AddSingleton<IDemoContext, DemoContextProxyService>();
    }

}
/*
                                                                              ╎
 -----------------------------------------------------------------------------╯*/



var builder = WebApplication.CreateSlimBuilder(args);

//let's cast participants
//
ConfigureDependencyInjection(builder, out var configureApi);
//
//

//onboard participants
//
var api = builder.Build();
//
//setup the stage


var (isDevelopment, isStaging, isProduction, apiEnv) = api.GetEnv();


api.UseMiddleware<MyDebugMiddleware>();


if (isDevelopment)
{

    api.UseSwagger();
    api.UseSwaggerUI();
    // api.UseExceptionHandler();
}

configureApi(api);

api.UseStaticFiles();

MapRoutes(api, isMainContainer);

api.MapRazorPages();
api.UseAntiforgery();


api.MapObservability(); // path = /metrics


//let the show start...
//
api.Run();

//
//end of story
