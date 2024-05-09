namespace VinZ.Common;

public class IgnorePropertiesResolver
(
    IEnumerable<string> propNamesToIgnore
)
    : DefaultContractResolver
{
    private readonly HashSet<string> _ignoreProps = [..propNamesToIgnore];

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (_ignoreProps.Contains(property.PropertyName))
        {
            property.ShouldSerialize = _ => false;
        }

        return property;
    }
}