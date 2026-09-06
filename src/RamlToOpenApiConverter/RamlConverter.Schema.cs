using System.Globalization;
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
            openApiSchema.Examples = [JsonSerializer.SerializeToNode(NormalizeYamlValueForJson(example))!];
        }
        else if (examples != null)
        {
            openApiSchema.Examples = examples.Values
                .OfType<IDictionary<object, object>>()
                .Select(exampleValue => JsonSerializer.SerializeToNode(NormalizeYamlValueForJson(exampleValue))!)
                .ToList()!;
        }

        return openApiSchema;
    }

    private static object? NormalizeYamlValueForJson(object? value)
    {
        return value switch
        {
            null => null,
            string text when text == "null" => null,
            IDictionary<object, object> dictionary => dictionary.ToDictionary(
                entry => System.Convert.ToString(entry.Key, CultureInfo.InvariantCulture)!,
                entry => NormalizeYamlValueForJson(entry.Value)),
            ICollection<object> collection => collection.Select(NormalizeYamlValueForJson).ToList(),
            _ => value
        };
    }
}
