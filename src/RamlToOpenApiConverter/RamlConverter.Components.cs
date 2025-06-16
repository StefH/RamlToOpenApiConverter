using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter;

public partial class RamlConverter
{
    private OpenApiComponents? MapComponents(IDictionary<object, object>? types, OpenApiSpecVersion specVersion)
    {
        if (types == null)
        {
            return null;
        }

        var components = new OpenApiComponents
        {
            Schemas = new Dictionary<string, IOpenApiSchema>()
        };

        foreach (var key in types.Keys.OfType<string>())
        {
            switch (types[key])
            {
                case IDictionary<object, object> values:
                    var type = values.Get("type");
                    if (type == "object" || (type != null && types.ContainsKey(type)))
                    {
                        components.Schemas.Add(key, MapValuesToSchema(values, specVersion));
                    }

                    if (values.ContainsKey("enum"))
                    {
                        var enumAsCollection = values.GetAsCollection("enum")?.OfType<string>();
                        if (enumAsCollection != null)
                        {
                            var enumValues = enumAsCollection
                                .SelectMany(e => e.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                                .Select(x => JsonValue.Create(x.Trim()));

                            var schema = new OpenApiSchema
                            {
                                Type = JsonSchemaType.String,
                                Enum = enumValues.OfType<JsonNode>().ToList()
                            };
                            components.Schemas.Add(key, schema);
                        }
                    }

                    string? arrayType = null;
                    if (type == "array" && values.TryGetValue("items", out var arrayItem))
                    {
                        arrayType = arrayItem as string;
                    }
                    else if (type?.EndsWith("[]") == true)
                    {
                        arrayType = type.Substring(0, type.Length - 2);
                    }

                    if (arrayType is not null)
                    {
                        if (components.Schemas.ContainsKey(arrayType))
                        {
                            components.Schemas.Add(key, new OpenApiSchema
                            {
                                Type = JsonSchemaType.Array,
                                Items = CreateDummyOpenApiReferenceSchema(arrayType, false)
                            });
                        }
                    }
                    break;

                case string jsonOrYaml:
                    var items = _deserializer.Deserialize<IDictionary<object, object>>(jsonOrYaml);
                    components.Schemas.Add(key, MapValuesToSchema(items, specVersion));
                    break;
            }
        }

        return components.Schemas.Count > 0 ? components : null;
    }

    private Dictionary<string, IOpenApiSchema> MapProperties(IDictionary<object, object>? properties, ICollection<object>? required, OpenApiSpecVersion specVersion)
    {
        var openApiProperties = new Dictionary<string, IOpenApiSchema>();

        if (properties == null)
        {
            return openApiProperties;
        }

        foreach (var key in properties.Keys.OfType<string>())
        {
            IDictionary<object, object> values;
            switch (properties[key])
            {
                case string stringValue:
                    values = new Dictionary<object, object>
                    {
                        { "type", stringValue },
                        { "properties", new Dictionary<object, object>() }
                    };
                    break;

                case IDictionary<object, object> complex:
                    values = complex;
                    break;

                default:
                    throw new NotSupportedException();
            }

            var propertyType = values.Get("type");
            if (propertyType == "object")
            {
                openApiProperties.Add(key, MapValuesToSchema(values, specVersion));
                continue;
            }

            if (propertyType != null && _types.ContainsKey(propertyType))
            {
                var simpleType = _types.GetAsDictionary(propertyType);
                var props = simpleType?.GetAsDictionary("properties");
                var schema = props != null ?
                    CreateDummyOpenApiReferenceSchema(propertyType, false) : // Custom type
                    MapParameterOrPropertyDetailsToSchema(simpleType!, specVersion); // Simple Type

                openApiProperties.Add(key, schema);
                continue;
            }

            string? arrayType = null;
            if (propertyType == "array" && values.TryGetValue("items", out var arrayItem))
            {
                arrayType = arrayItem as string;
            }
            else if (propertyType?.EndsWith("[]") == true)
            {
                arrayType = propertyType.Substring(0, propertyType.Length - 2);
            }

            if (arrayType is not null)
            {
                openApiProperties.Add(key, new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = CreateDummyOpenApiReferenceSchema(arrayType, false)
                });
                continue;
            }

            openApiProperties.Add(key, MapParameterOrPropertyDetailsToSchema(values, specVersion));
        }

        return openApiProperties;
    }

    /// <summary>
    /// Replace uses in file
    /// </summary>
    private static IDictionary<object, object> ReplaceUses(IDictionary<object, object> source, IDictionary<object, object> uses)
    {
        var useReplace = ReplaceIs(source, uses);
        source.Replace(useReplace, Constants.IsTag);

        if (uses.Count <= 0)
        {
            return source;
        }

        foreach (var pathValue in source.Values.OfType<Dictionary<object, object>>())
        {
            useReplace = ReplaceIs(pathValue, uses);
            pathValue.Replace(useReplace, Constants.IsTag);
        }

        return source;
    }

    private static IDictionary<object, object> ReplaceIs(IDictionary<object, object> source, IDictionary<object, object> uses)
    {
        IDictionary<object, object> useReplace = new Dictionary<object, object>();

        if (uses.Count <= 0)
        {
            return useReplace;
        }

        var isTag = source.GetAsString(Constants.IsTag);
        if (isTag == null)
        {
            return useReplace;
        }

        var pathSeparators = isTag.Split('.');
        foreach (var use in uses.Where(u => u.Key.ToString() == pathSeparators.First()))
        {
            var traits = (use.Value as IDictionary<object, object>)?.GetAsDictionary(Constants.Traits);
            if (traits != null)
            {
                useReplace = traits;

                foreach (var pathSeparator in pathSeparators.Skip(1))
                {
                    var replaced = useReplace.GetAsDictionary(pathSeparator);
                    if (replaced != null)
                    {
                        useReplace = replaced;
                    }
                }
            }
        }

        return useReplace;
    }
}