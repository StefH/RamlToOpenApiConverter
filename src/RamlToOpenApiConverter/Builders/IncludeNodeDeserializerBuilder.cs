using SharpYaml.Schemas;
using SharpYaml.Serialization;

namespace RamlToOpenApiConverter.Builders;

internal static class IncludeNodeDeserializerBuilder
{
    public static Serializer Build()
    {
        var settings = new SerializerSettings(new FailsafeSchema());
        settings.RegisterTagMapping(Constants.IncludeTag, typeof(string), false);

        return new Serializer(settings);
    }
}