using System.Dynamic;
using System.Text.RegularExpressions;

namespace VinZ.MessageQueue;

public static class MessageHelper
{
    public static CorrelationId CorrelationId(this IClientNotificationSerialized notification)
    {
        return new CorrelationId(notification.CorrelationId1, notification.CorrelationId2);
    }

    public static string CorrelationGuid(this IClientNotificationSerialized notification)
    {
        return CorrelationId(notification).Guid;
    }

    public static TMessage MessageAs<TMessage>(this IHaveSerializedMessage n)
    {
        return (TMessage)n.MessageAs(typeof(TMessage));
    }

    public static object MessageAsObject(this IHaveSerializedMessage n)
    {
        return n.MessageAs(null);
    }


    public static object Empty = new();
    public static string EmptyType = Empty.GetType().FullName;
    public static string EmptySerialized = System.Text.Json.JsonSerializer.Serialize(Empty);

    public static string AnonymousType = "Anonymous";

    public static string GetTypeSerializedName(this object? messageObj)
    {
        if (messageObj == null)
        {
            return EmptyType;
        }

        if (messageObj.IsAnonymous())
        {
            return AnonymousType;
        }

        return messageObj.GetType().FullName 
               ?? throw new NullReferenceException("GetType().FullName");
    }

    public static Type? GetTypeFromSerializedName(this string? typeString)
    {
        if (typeString == null)
        {
            return null;
        }

        if (typeString == AnonymousType ||
            typeString.StartsWith("<>f__AnonymousType0"))
        {
            typeString = typeof(ExpandoObject).FullName
                         ?? throw new NullReferenceException("GetType().FullName");
        }

        var isNullable = false;
        var isArray = false;

        if (typeString.EndsWith("?"))
        {
            isNullable = true;
            typeString = typeString[..^1];
        }

        if (typeString.EndsWith("[]"))
        {
            isArray = true;
            typeString = typeString[..^2];
        }

        if (typeString == default)
        {
            throw new ArgumentNullException(nameof(typeString));
        }

        var genericRx = new Regex(@$"^([^`]+)`(\d+)\[(.*)\]$");
        var argTypesRx = new Regex(@"^(\[([^]]+)\])+$");

        var m = genericRx.Match(typeString);

        Type type;

        if (!m.Success)
        {
            type = typeString.GetTypeFromFullNameWithLoading();

            if (type == null)
            {
                throw new ArgumentException($"type not found: {typeString}", nameof(typeString));
            }
        }
        else
        {
            var genericTypeString = m.Groups[1].Value;
            var genericType = genericTypeString.GetTypeFromFullNameWithLoading();

            if (genericType == null)
            {
                throw new ArgumentException($"generic type not found: {genericTypeString}", nameof(typeString));
            }

            var argCount = int.Parse(m.Groups[2].Value);

            var argTypesString = m.Groups[3].Value;

            var m2 = argTypesRx.Match(argTypesString);

            if (!m2.Success)
            {
                throw new ArgumentException($"Invalid type serialized name: {typeString}", nameof(typeString));
            }

            var argTypes = m2.Groups.Cast<Group>().ToArray()[^1].Captures
                .Select(c =>
                {
                    var argTypeString = c.Value;
                    var argType = argTypeString.GetTypeFromFullNameWithLoading();

                    if (argType == null)
                    {
                        throw new ArgumentException($"argument type not found: {argTypeString}", nameof(typeString));
                    }

                    return argType;
                })
                .ToArray();

            if (argCount != argTypes.Length)
            {
                throw new ArgumentException($"Invalid generic type, argument count mismatch: {typeString}",
                    nameof(typeString));
            }

            type = genericType.MakeGenericType(argTypes);
        }


        if (isArray)
        {
            type = type.MakeArrayType();
        }

        if (isNullable)
        {
            type = typeof(Nullable<>).MakeGenericType([type]);
        }

        return type;
    }

    public static object MessageAs(this IHaveSerializedMessage notification, Type? tMessage)
    {
        if (notification == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(notification.Message) || notification.Message == EmptySerialized)
        {
            if (tMessage != null)
            {
                throw new ArgumentNullException(nameof(notification.Message));
            }
        }

        var nMessageType = notification.MessageType;

        var messageType = nMessageType.GetTypeFromSerializedName();

        if (messageType?.IsInterface ?? false)
        {
            throw new ArgumentException($"invalid type {messageType.FullName} : must be concrete", nameof(notification));
        }

        if ((tMessage != null && messageType != tMessage) || 
            (messageType == null && notification.Message != null)
            )
        {
            throw new InvalidOperationException($"type mismatch {messageType.FullName} != {tMessage.FullName}");
        }

        if (messageType == null)
        {
            return null;
        }

        object? messageObj;

        if (messageType.IsClass)
        {
            messageObj = JsonConvert.DeserializeObject(notification.Message, messageType);

            if (messageObj == null)
            {
                throw new ArgumentNullException(nameof(messageObj));
            }
        }
        else
        {
            messageObj = Convert.ChangeType(notification.Message, messageType, CultureInfo.InvariantCulture);
        }

        return messageObj;
    }

}