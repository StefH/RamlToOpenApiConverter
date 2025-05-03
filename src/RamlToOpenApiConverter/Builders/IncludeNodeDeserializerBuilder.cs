using RamlToOpenApiConverter.Yaml;
using YamlDotNet.Serialization;

namespace RamlToOpenApiConverter.Builders;

internal static class IncludeNodeDeserializerBuilder
{
    public static IDeserializer Build(string directoryName)
    {
        var builder = new DeserializerBuilder();

        var includeNodeDeserializerOptions = new YamlIncludeNodeDeserializerOptions
        {
            DirectoryName = directoryName
        };

        var includeNodeDeserializer = new YamlIncludeNodeDeserializer(includeNodeDeserializerOptions);

        return builder
            .WithTagMapping(string.Empty, typeof(IncludeRef))
            .WithTagMapping(Constants.IncludeTag, typeof(IncludeRef))
            .WithNodeDeserializer(includeNodeDeserializer, s => s.OnTop())
            .Build();
    }
}