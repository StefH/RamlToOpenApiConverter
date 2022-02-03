using System.Collections.Generic;

namespace RamlToOpenApiConverter.Yaml
{
    public class IncludeRef : Dictionary<object, object>
    {
        public string? FileName { get; set; }
    }
}