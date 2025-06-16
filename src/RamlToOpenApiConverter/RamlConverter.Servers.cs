using System.Collections.Generic;
using Microsoft.OpenApi;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter;

public partial class RamlConverter
{
    private static List<OpenApiServer>? MapServers(IDictionary<object, object> items)
    {
        var baseUri = items.Get("baseUri");
        if (string.IsNullOrEmpty(baseUri))
        {
            return null;
        }

        return
        [
            new OpenApiServer
            {
                Url = baseUri
            }
        ];
    }
}