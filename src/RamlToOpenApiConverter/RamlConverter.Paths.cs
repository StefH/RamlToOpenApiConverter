using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter;

public partial class RamlConverter
{
    private OpenApiPaths MapPaths(IDictionary<object, object> o, IDictionary<object, object> uses, OpenApiSpecVersion specVersion)
    {
        var paths = new OpenApiPaths();

        foreach (var key in o.Keys.OfType<string>().Where(k => k.StartsWith("/")))
        {
            var pathItems = MapPathItems(key, [], o.GetAsDictionary(key)!, uses, specVersion);
            foreach (var pathItem in pathItems)
            {
                paths.Add(pathItem.AdjustedPath, pathItem.Item);
            }
        }

        return paths;
    }

    private ICollection<(IOpenApiPathItem Item, string AdjustedPath)> MapPathItems(
        string parent,
        IList<IOpenApiParameter> parentParameters,
        IDictionary<object, object> values,
        IDictionary<object, object> uses,
        OpenApiSpecVersion specVersion)
    {
        values = ReplaceUses(values, uses);

        var items = new List<(IOpenApiPathItem Item, string AdjustedPath)>();

        // Fetch all parameters from this path
        var parameters = MapParameters(values, specVersion);

        // And add parameters from parent
        foreach (var parameter in parentParameters)
        {
            parameters.Add(parameter);
        }

        var operations = new Dictionary<HttpMethod, OpenApiOperation>();

        // Loop all keys which do not start with a '/'
        foreach (var key in values.Keys.OfType<string>().Where(k => !k.StartsWith("/")))
        {
            // And try to match operations
            if (TryMapOperationType(key, out var operationType))
            {
                var operationValues = values.GetAsDictionary(key)!;
                var operation = MapOperation(operationValues, specVersion);

                // Add parameters from the path to this operation
                foreach (var parameter in parameters)
                {
                    operation.Parameters?.Add(parameter);
                }

                operations.Add(operationType, operation);
            }
        }

        // Operations found on this level from the PathItem, add these to a new PathItem
        if (operations.Any())
        {
            var singleItem = new OpenApiPathItem
            {
                Operations = operations
            };

            items.Add((singleItem, parent));
        }

        // Now check 1 level deeper (loop all keys which do start with a '/')
        foreach (var key in values.Keys.OfType<string>().Where(k => k.StartsWith("/")))
        {
            var d = values.GetAsDictionary(key) ?? new Dictionary<object, object>();
            var newPath = $"{parent}{key}";
            var mapItems = MapPathItems(newPath, parameters, d, uses, specVersion);
            items.AddRange(mapItems);
        }

        return items;
    }

    private OpenApiOperation MapOperation(IDictionary<object, object> values, OpenApiSpecVersion specVersion)
    {
        return new OpenApiOperation
        {
            Description = values.Get("description"),
            Parameters = MapParameters(values, specVersion),
            Responses = MapResponses(values.GetAsDictionary("responses"), specVersion),
            RequestBody = MapRequest(values.GetAsDictionary("body"), specVersion)
        };
    }

    private OpenApiResponses? MapResponses(IDictionary<object, object>? values, OpenApiSpecVersion specVersion)
    {
        if (values == null)
        {
            return null;
        }

        var openApiResponses = new OpenApiResponses();

        foreach (var key in values.Keys)
        {
            var response = values.GetAsDictionary(key);
            if (response != null)
            {
                OpenApiResponse openApiResponse;

                var body = response.GetAsDictionary("body");
                if (body != null)
                {
                    openApiResponse = new OpenApiResponse
                    {
                        Content = MapContents(body, specVersion)
                    };
                }
                else
                {
                    openApiResponse = new OpenApiResponse();
                }

                openApiResponse.Description = response.Get("description");

                openApiResponses.Add(global::System.Convert.ToString(key, CultureInfo.InvariantCulture)!, openApiResponse);
            }
        }

        return openApiResponses.Count > 0 ? openApiResponses : null;
    }

    private OpenApiRequestBody? MapRequest(IDictionary<object, object>? values, OpenApiSpecVersion specVersion)
    {
        if (values == null)
        {
            return null;
        }

        var requestBody = new OpenApiRequestBody
        {
            Content = MapContents(values, specVersion)
        };

        return requestBody;
    }

    private Dictionary<string, IOpenApiMediaType>? MapContents(IDictionary<object, object>? values, OpenApiSpecVersion specVersion)
    {
        if (values == null)
        {
            return null;
        }

        var content = new Dictionary<string, IOpenApiMediaType>();

        foreach (var key in new[] { "application/json", "application/xml" })
        {
            if (values.ContainsKey(key))
            {
                var items = values.GetAsDictionary(key); // Body and Example and Type and Schema
                var exampleAsJson = items?.Get("example");
                var examplesAsJson = items?.GetAsDictionary("examples");

                var type = items?.Get("type");
                var schemaValue = items?.Get("schema");

                IOpenApiSchema? schema = null;
                if (!string.IsNullOrEmpty(type))
                {
                    schema = MapMediaTypeSchema(type!, specVersion);
                }
                else if (!string.IsNullOrEmpty(schemaValue))
                {
                    schema = MapMediaTypeSchema(schemaValue!, specVersion);
                }

                var openApiMediaType = new OpenApiMediaType
                {
                    Schema = schema,
                    Example = !string.IsNullOrEmpty(exampleAsJson) ? MapExample(exampleAsJson!) : null
                };

                if (!string.IsNullOrEmpty(exampleAsJson))
                {
                    openApiMediaType.Example = MapExample(exampleAsJson!);
                }
                else if (examplesAsJson != null)
                {
                    openApiMediaType.Examples = MapExamples(examplesAsJson);
                }

                content.Add(key, openApiMediaType);
            }
        }

        return content;
    }

    private static JsonNode? MapExample(string exampleAsJson)
    {
        var normalizedJson = exampleAsJson.Replace("\0", string.Empty);
        return JsonNode.Parse(normalizedJson);
    }

    private static Dictionary<string, IOpenApiExample>? MapExamples(IDictionary<object, object> examplesAsJson)
    {
        var result = new Dictionary<string, IOpenApiExample>();
        foreach (var example in examplesAsJson)
        {
            if (example.Key is not string key)
            {
                continue;
            }

            var openApiExample = new OpenApiExample();
            if (example.Value is string valueAsString)
            {
                var normalizedJson = valueAsString.Replace("\0", string.Empty);
                openApiExample.SerializedValue = normalizedJson;
                openApiExample.Value = MapExample(normalizedJson);
            }

            if (example.Value is IList<object> valueAsListItems)
            {
                if (!valueAsListItems.Any())
                {
                    continue;
                }

                JsonNode? jsonNode;
                if (valueAsListItems.Count == 1)
                {
                    jsonNode = JsonSerializer.SerializeToNode(valueAsListItems.First());
                }
                else
                {
                    jsonNode = JsonSerializer.SerializeToNode(valueAsListItems);
                }

                openApiExample.Value = jsonNode;
            }

            result.Add(key, openApiExample);
        }

        return result;
    }

    private IOpenApiSchema MapMediaTypeSchema(string value, OpenApiSpecVersion specVersion)
    {
        if (value.StartsWith("{"))
        {
            var objectType = _deserializer.Deserialize<IDictionary<object, object>>(value)!;
            return MapValuesToSchema(objectType, specVersion);
        }

        var referenceSchemas = value
            .Split(['|'], StringSplitOptions.RemoveEmptyEntries)
            .Select(o => CreateOpenApiReferenceSchema(o.Trim(), false))
            .ToList();

        if (referenceSchemas.Count == 1)
        {
            return referenceSchemas.Single();
        }

        return new OpenApiSchema
        {
            AnyOf = referenceSchemas
        };
    }

    private static IOpenApiSchema CreateOpenApiReferenceSchema(string referenceId, bool nullable)
    {
        var reference = new OpenApiSchemaReference(referenceId);

        // Not sure if this is needed, can work? (2025-04-14)
        if (reference.Type != null && nullable)
        {
            reference.Type.Value.AddNullable(true);
        }

        return reference;
    }

    private static bool TryMapOperationType(string value, out HttpMethod operationType)
    {
        try
        {
            operationType = new HttpMethod(value);
            return true;
        }
        catch
        {
            // NO-OP
        }

        operationType = HttpMethod.Get;
        return false;
    }
}