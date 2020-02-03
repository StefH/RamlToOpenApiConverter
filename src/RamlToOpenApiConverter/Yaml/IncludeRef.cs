using System.Collections.Generic;

namespace RamlToOpenApiConverter.Yaml
{
    public class IncludeRef : Dictionary<string, object>
    {
        public string FileName { get; set; }
    }
}