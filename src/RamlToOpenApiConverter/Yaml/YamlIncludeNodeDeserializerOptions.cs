using System.Collections.Generic;

namespace RamlToOpenApiConverter.Yaml
{
    public class YamlIncludeNodeDeserializerOptions
    {
        public string DirectoryName { get; set; } = default!;

        public IList<IncludeRef> includeRefs { get; set; } = default!;
    }
}