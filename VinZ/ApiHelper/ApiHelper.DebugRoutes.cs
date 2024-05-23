namespace VinZ.Common;

public partial class ApiHelper
{
    public static WebApplication MapDebugRoutes(this WebApplication app)
    {
        app
            .MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
            {
                return string.Join("\n", endpointSources

                    .SelectMany(source => source.Endpoints)

                    .Select(endpoint => new {
                        Pattern = (endpoint as RouteEndpoint)?.RoutePattern?.RawText,
                        Method = string.Join(',', endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods ?? [])
                    })
                    .SelectMany(r => r.Method.Split(',').Select(m => r with { Method = m }))

                    .OrderBy(r => ((r.Pattern ?? string.Empty).StartsWith('/') ? r.Pattern![1..] : r.Pattern)?.ToLower())
                    .ThenBy(r => r.Method?.ToLower())

                    .Select(r => $"{(r.Pattern.StartsWith('/') ? "" : '/')}{r.Pattern?.ToLower()} {r.Method}")

                );
            });

        app.MapGet("/debug/routes/details", (IEnumerable<EndpointDataSource> endpointSources) =>
        {
            return endpointSources

                .SelectMany(es => es.Endpoints).Select(endpoint =>
                {
                    var routeName = endpoint.Metadata.OfType<RouteNameMetadata>()
                        .FirstOrDefault();

                    var httpMethods = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();

                    var routeEndpoint = endpoint as RouteEndpoint;

                    return new
                    {
                        Name = routeName?.RouteName,

                        Method = httpMethods == null ? string.Empty : string.Join(",", httpMethods.HttpMethods), // [GET, POST, ...]

                        Pattern = routeEndpoint?.RoutePattern?.RawText,

                        Segments = routeEndpoint?.RoutePattern?.PathSegments?.ToArray(),

                        Parameters = routeEndpoint?.RoutePattern?.Parameters?.ToArray(),

                        Precedence = new
                        {
                            Inbound = routeEndpoint?.RoutePattern?.InboundPrecedence,
                            Outbound = routeEndpoint?.RoutePattern?.OutboundPrecedence
                        }
                    };
                })
                .SelectMany(r => r.Method.Split(',').Select(m => r with { Method = m }))

                .OrderBy(r => r.Pattern?.ToLower()).ThenBy(r => r.Method?.ToLower());
        });

        return app;
    }
}