using System.IO;
using RamlToOpenApiConverter.Yaml;
using YamlDotNet.Serialization;

namespace RamlToOpenApiConverter.Builders
{
    internal static class IncludeNodeDeserializerBuilder
    {
        public static IDeserializer Build(string directoryName)
        {
            var builder = new DeserializerBuilder();

            var includeNodeDeserializerOptions = new YamlIncludeNodeDeserializerOptions
            {
                DirectoryName = directoryName //!string.IsNullOrEmpty(directoryName) ? Path.GetDirectoryName(directoryName) : string.Empty
            };

            var includeNodeDeserializer = new YamlIncludeNodeDeserializer(includeNodeDeserializerOptions);

            return builder
                .WithTagMapping(Constants.IncludeTag, typeof(IncludeRef))
                .WithNodeDeserializer(includeNodeDeserializer, s => s.OnTop())
                .Build();
        }
    }
}