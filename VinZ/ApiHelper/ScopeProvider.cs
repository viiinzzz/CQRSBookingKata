namespace VinZ.Common;

public class ScopeProvider(IServiceProvider sp) : IScopeProvider
{
    public IServiceScope GetScope<T>(out T t) where T : notnull
    {
        var scope = sp.CreateScope();

        t = scope.ServiceProvider.GetRequiredService<T>();

        return scope;
    }

    public IServiceScope GetScope(Type serviceType, out object t)
    {
        var scope = sp.CreateScope();

        t = scope.ServiceProvider.GetRequiredService(serviceType);

        return scope;
    }

}