using System.Collections.Generic;

namespace RamlToOpenApiConverter.Yaml;

internal class IncludeRef : Dictionary<object, object>
{
    public string? FileName { get; set; }
}