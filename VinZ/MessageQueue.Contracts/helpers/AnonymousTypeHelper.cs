namespace VinZ.MessageQueue;

public static class AnonymousTypeHelper
{
    private static readonly Type RuntimeType = typeof(Type).GetType();

    public static bool IsAnonymousType(this Type type)
    {
        if (type == RuntimeType)
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

    public static bool IsAnonymous(this object? instance)
    {
        if (instance == null)
        {
            return false;
        }

        var type = instance.GetType();

        return IsAnonymousType(type);
    }
}