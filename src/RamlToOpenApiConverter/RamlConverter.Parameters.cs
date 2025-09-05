using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter;

public partial class RamlConverter
{
    private List<IOpenApiParameter> MapParameters(IDictionary<object, object> values, OpenApiSpecVersion specVersion)
    {
        var parameters = new List<IOpenApiParameter>();

        parameters.AddRange(MapParameters(values.GetAsDictionary("queryParameters"), ParameterLocation.Query, specVersion));
        parameters.AddRange(MapParameters(values.GetAsDictionary("uriParameters"), ParameterLocation.Path, specVersion));
        parameters.AddRange(MapParameters(values.GetAsDictionary("headers"), ParameterLocation.Header, specVersion));

        return parameters;
    }

    private IList<OpenApiParameter> MapParameters(IDictionary<object, object>? parameters, ParameterLocation parameterLocation, OpenApiSpecVersion specVersion)
    {
        var openApiParameters = new List<OpenApiParameter>();

        if (parameters == null)
        {
            return openApiParameters;
        }

        foreach (var key in parameters.Keys.OfType<string>())
        {
            var parameterDetails = parameters.GetAsDictionary(key) ?? new Dictionary<object, object>();
            var schema = MapParameterOrPropertyDetailsToSchema(parameterDetails, specVersion);

            bool required = parameterDetails.Get<bool?>("required") ?? false;

            openApiParameters.Add(new OpenApiParameter
            {
                In = parameterLocation,
                Required = required,
                Description = parameterDetails.Get("description"),
                Name = key,
                Schema = schema
            });
        }

        return openApiParameters;
    }

    private IOpenApiSchema MapParameterOrPropertyDetailsToSchema(IDictionary<object, object> details, OpenApiSpecVersion specVersion)
    {
        var schemaTypeFromRaml = details.Get("type");
        var schemaFormatFromRaml = details.Get("format");

        var schemaTypes = (schemaTypeFromRaml ?? "string")
            .Split(['|'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToArray();

        // https://github.com/raml-org/raml-spec/blob/master/versions/raml-10/raml-10.md#nil-type
        var isNil = schemaTypes.Contains("nil");
        var schemaTypesExceptNil = schemaTypes.Where(s => s != "nil").Distinct().ToArray();
        var schemaType = schemaTypesExceptNil.Length > 1 ? "object" : schemaTypesExceptNil.FirstOrDefault() ?? "string";

        var schema = new OpenApiSchema
        {
            Extensions = new Dictionary<string, IOpenApiExtension>(),
            Minimum = details.Get<string?>(OpenApiConstants.Minimum),
            Maximum = details.Get<string?>(OpenApiConstants.Maximum),
            MaxLength = details.Get<int?>(OpenApiConstants.MaxLength),
            MinLength = details.Get<int?>(OpenApiConstants.MinLength)
        };

        if (isNil && specVersion == OpenApiSpecVersion.OpenApi2_0)
        {
            // This specification extension is supported only in OpenAPI 2.0.
            schema.Extensions.Add(OpenApiConstants.NullableExtension, new OpenApiAny(JsonValue.Create(true)));
        }

        switch (schemaType)
        {
            case "datetime":
                schema.Type = JsonSchemaType.String.AddNullable(isNil);
                schema.Format = "date-time";
                break;

            case "number":
                switch (schemaFormatFromRaml)
                {
                    case "long":
                    case "int64":
                        schema.Type = JsonSchemaType.Integer.AddNullable(isNil);
                        schema.Format = "int64";
                        break;

                    case "float":
                        schema.Type = JsonSchemaType.Number.AddNullable(isNil);
                        schema.Format = "float";
                        break;

                    case "double":
                        schema.Type = JsonSchemaType.Number.AddNullable(isNil);
                        schema.Format = "double";
                        break;

                    default:
                        schema.Type = JsonSchemaType.Integer.AddNullable(isNil);
                        schema.Format = "int";
                        break;
                }

                return schema;

            default:
                // Check if the SchemaType is defined as enum, simple or complex type in the _types list
                if (_types.ContainsKey(schemaType))
                {
                    return CreateOpenApiReferenceSchema(schemaType, isNil);
                }

                var isEnum = details.Keys.OfType<string>().FirstOrDefault(k => k == "enum");
                if (isEnum != null)
                {
                    var enumAsCollection = details.GetAsCollection(isEnum)?.OfType<string>();
                    var enumValues = enumAsCollection?
                        .SelectMany(e => e.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                        .Select(x => JsonValue.Create(x.Trim()));

                    schema.Type = JsonSchemaType.String.AddNullable(isNil);
                    schema.Enum = enumValues?.OfType<JsonNode>().ToList() ?? [];
                }
                else
                {
                    schema.Type = ParseSchemaType(schemaType, isNil);
                    schema.Format = schemaFormatFromRaml;
                }
                break;
        }

        return schema;
    }

    private static JsonSchemaType ParseSchemaType(string schemaType, bool nullable)
    {
        var jsonSchemaType = schemaType.ToLower() switch
        {
            "string" => JsonSchemaType.String,
            "number" => JsonSchemaType.Number,
            "integer" => JsonSchemaType.Integer,
            "boolean" => JsonSchemaType.Boolean,
            "array" => JsonSchemaType.Array,
            _ => JsonSchemaType.Object,
        };

        return jsonSchemaType.AddNullable(nullable);
    }
}