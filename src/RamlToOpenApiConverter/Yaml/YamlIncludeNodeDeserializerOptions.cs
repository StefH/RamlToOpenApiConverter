using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace RamlToOpenApiConverter.Yaml
{
    public class YamlIncludeNodeDeserializerOptions
    {
        public IDeserializer Deserializer { get; set; }

        public string DirectoryName { get; set; }

        public IncludeRefCallback IncludeRefCallback { get; set; }
    }
}