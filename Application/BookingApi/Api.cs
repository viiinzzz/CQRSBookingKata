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

var Console = new cons.AnsiVtConsole(); //https://github.com/franck-gaspoz/AnsiVtConsole.NetCore


var pif = ProgramInfo.Get();

var pauseOnError = pif.IsDebug && pif.IsTrueEnv("DEBUG_PAUSE_ON_ERROR");

var demoMode = pif.IsTrueEnv("DEMO_MODE");

var busUrl = ApiHelper.GetAppUrlPrefix("bus");

var myIps = ApiHelper.GetMyIps();

if (myIps.Length == 0)
{
    Console.Out.WriteLine(@$"
(f=darkgray)Error:(rdc) IP not found
");

    Environment.Exit(-1);
}

{
    var exeStr = pif.ExeName.Length > 0 || pif.ExeVersion.Length > 0 ? $"(bon){pif.ExeName} {pif.ExeVersion}(rdc)" : "";
    var buildArchiStr = $"(f=darkgray)Build {pif.BuildConfiguration} {pif.ProcessArchitecture}(rdc)";
    var osFwStr = $"(f=darkgray){pif.Os} {pif.Framework}(rdc)";
    var envStr = $"(invon,f={(pif.IsRelease ? "magenta" : "cyan")}){pif.Env}(rdc)";
    var pauseStr = pauseOnError ? " " + $"(invon,f=yellow)PauseOnError(rdc)" : "";

    Console.Out.WriteLine(@$"{exeStr} {envStr}{pauseStr} {buildArchiStr} {osFwStr} 
(f=darkgray)Network:(rdc) {string.Join(", ", myIps.Select(ip => $"(uon)http://{ip}:{busUrl.Port}(rdc)"))}");
}
// (uon){busUrl}(rdc)

const string demoModeStr = "(invon,f=yellow)DemoMode(rdc)";

const double precisionMaxKm = 0.5;

const int dbContextKeepAliveMilliseconds = 30_000;

/*
                                                                              ╎
 -----------------------------------------------------------------------------╯*/





/*╭-----------------------------------------------------------------------------
  ╎ DI methods
  */

//adding components to the magic wiring box, aka. DI Container to achieve IoC
//configureApplication is the hook for using services

void ConfigureDependencyInjection
(
    WebApplicationBuilder builder,
    out Action<WebApplication> configureApplication
)
{
    var configureApiHooks = new List<Action<WebApplication>>();

    //database
    var logLevelEFContext = builder.EnumConfiguration("Logging:LogLevel:Microsoft.EntityFrameworkCore.DbContext",
        "Logging:LogLevel:Default", LogLevel.Warning);

    var dbContextTypes = builder.GetConfigurationTypes("Api:DbContext", Dependencies.AvailableDbContextTypes);

    builder.RegisterDbContexts(dbContextTypes, pif.IsDebug, pif.Env, logLevelEFContext);

    configureApiHooks.Add(app =>
    {
        app.EnsureDatabaseCreated(dbContextTypes, logLevelEFContext,
            pif.IsDebug ? dbContextKeepAliveMilliseconds : null);
    });


    var services = builder.Services;


    //infra
    services.AddSingleton<IScopeProvider, ScopeProvider>();
    services.AddExceptionHandler<GlobalExceptionHandler>();


    //web
    var servePages = builder.IsTrueConfiguration("Api:ServePages");
    if (servePages)
    {
        services.AddRazorPages();

        configureApiHooks.Add(app =>
        {
            app.UseStaticFiles();

            app.MapRazorPages();
            app.UseAntiforgery();
        });
    }
    else
    {
        services.AddDefaultProblemDetailsFactory();
    }


    //open api doc
    var serveApiDoc = builder.IsTrueConfiguration("Api:ServeApiDoc");
    if (serveApiDoc)
    {
        // /test -> /swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        configureApiHooks.Add(app =>
        {
                app.UseSwagger();
                app.UseSwaggerUI();
                // api.UseExceptionHandler();
        });
    }

    // if (pif.IsDebug)
    // {
    services.AddHostedService<DebugRoutesHostService>();
    // }


    //security
    services.AddAntiforgery();
    services.AddCors();
    services.AddAuthentication().AddJwtBearer();
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-8.0


    //app services
    services.AddSingleton<ITimeService, TimeService>();
    services.AddSingleton<IRandomService, RandomService>();

    if (pif.Env is "Development" or "Production-App")
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

    services.AddSingleton(mqConfig);

    services.AddMessageQueue(mqConfig, out var messageQueueUrl, out var addedBus);

    Console.Out.WriteLine(@$"(f=darkgray)Bus:(rdc) (uon){messageQueueUrl}(rdc) (f=darkgray){string.Join(", ", addedBus)}(rdc)");


    //repo
    builder.AddScopedConfigurationTypes("Api:Repository", Dependencies.AvailableRepositories, out var registeredRepository);
    if (servePages &&
        !registeredRepository.Contains(typeof(IAdminRepository)))
    {
        throw new ApplicationException(@"Invalid configuration:
{
  Api:
    ServePages: true
-requires-
    DbContext: [
      ""BookingKata.Infrastructure.Storage.BookingAdminContext""
    ],
    Repository: [
      ""BookingKata.Admin.IAdminRepository""
    ]");
    }

    //business/support/third-party
    builder.AddScopedConfigurationTypes("Api:Service", Dependencies.AvailableServices, out var registeredServices);

    var bookingConfig = new BookingConfiguration
    {
        PrecisionMaxKm = precisionMaxKm
    };
    services.AddSingleton(bookingConfig);


    //observability
    services.AddObservability();

    configureApiHooks.Add(app =>
    {
        app.MapObservability(); // path = /metrics
    });


    //demo
    demoMode = demoMode || builder.IsTrueConfiguration("Api:DemoMode");

    if (demoMode)
    {
        if (demoModeStr != null) Console.Out.WriteLine(@$"
{demoModeStr}
");
        services.ConfigureDemo();
    }
    else
    {
        services.ConfigureDemoProxy();
    }


    configureApplication = app =>
    {
        foreach (var hook in configureApiHooks)
        {
            hook(app);
        }
    };
}
/*
                                                                              ╎
 -----------------------------------------------------------------------------╯*/



var builder = WebApplication.CreateSlimBuilder(args);
// builder.WebHost.ConfigureKestrel(options =>
// {
//   
// });


//let's cast actors
//
ConfigureDependencyInjection(builder, out var useServices);
//
//

//onboard actors
//
var api = builder.Build();
//
//setup the stage


var (isDevelopment, isStaging, isProduction, apiEnv) = api.GetEnv();


api.UseMiddleware<MyDebugMiddleware>();

//task the actors
//
useServices(api);
//
//


//api routes
var serveApi = builder.IsTrueConfiguration("Api:ServeApi");

MapParticipantRoutes(api); //basic compulsory for participants collaboration

if (serveApi)
{
    MapApiRoutes(api); //front api gateway
}


//let the show start...
//
api.Run();

//
//end of story
