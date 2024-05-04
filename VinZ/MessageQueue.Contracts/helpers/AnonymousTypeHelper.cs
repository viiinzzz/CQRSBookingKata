namespace VinZ.MessageQueue;

public static class AnonymousTypeHelper
{
    public static bool IsAnonymous(this Type type)
    {
        var runtimeType = typeof(Type).GetType();

        if (type == runtimeType)
        {
            return true;
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        var d = type.GetGenericTypeDefinition();

        if (!d.IsClass ||
            !d.IsSealed ||
            !d.Attributes.HasFlag(TypeAttributes.NotPublic))
        {
            return false;
        }
        
        var attributes = d.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
        
        if (attributes is not { Length: > 0 })
        {
            return false;
        }

        return true;
    }

    public static bool IsAnonymousType<T>(this T instance)
    {
        return IsAnonymous(instance.GetType());
    }
}