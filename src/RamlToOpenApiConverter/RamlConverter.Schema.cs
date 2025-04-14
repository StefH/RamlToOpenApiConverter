using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter;

public partial class RamlConverter
{
    private IOpenApiSchema MapValuesToSchema(IDictionary<object, object> values)
    {
        var required = values.GetAsCollection("required");
        var properties = values.GetAsDictionary("properties");
        var example = values.GetAsDictionary("example");

        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = required != null ? new HashSet<string>(required.OfType<string>()) : null,
            Properties = MapProperties(properties, required),
            Example = example != null ? JsonSerializer.SerializeToNode(example) : null
        };
    }
}