using System.Collections.Generic;
using Microsoft.OpenApi;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter;

public partial class RamlConverter
{
    private static OpenApiInfo MapInfo(IDictionary<object, object> o)
    {
        return new OpenApiInfo
        {
            Title = o.Get("title"),
            Description = o.Get("description"),
            Version = o.Get("version")
        };
    }
}