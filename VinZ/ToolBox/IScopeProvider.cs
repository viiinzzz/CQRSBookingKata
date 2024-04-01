using Microsoft.Extensions.DependencyInjection;

namespace VinZ.ToolBox;

public interface IScopeProvider
{
    IServiceScope GetScope<T>(out T t);
    IServiceScope GetScope(Type serviceType, out object t);
}