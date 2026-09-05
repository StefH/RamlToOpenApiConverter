using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.OpenApi;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter;

public partial class RamlConverter
{
    private OpenApiSchema MapValuesToSchema(IDictionary<object, object> values, OpenApiSpecVersion specVersion)
    {
        var required = values.GetAsCollection("required");
        var properties = values.GetAsDictionary("properties");
        var example = values.GetAsDictionary("example");
        var examples = values.GetAsDictionary("examples");

        var openApiSchema =  new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = required != null ? new HashSet<string>(required.OfType<string>()) : null,
            Properties = MapProperties(properties, specVersion)
        };

        if (example != null)
        {
            openApiSchema.Examples = [JsonSerializer.SerializeToNode(example)!];
        }
        else if (examples != null)
        {
            openApiSchema.Examples = examples.Values
                .OfType<IDictionary<object, object>>()
                .Select(exampleValue => JsonSerializer.SerializeToNode(exampleValue))
                .Where(node => node != null)
                .ToList()!;
        }

        return openApiSchema;
    }
}