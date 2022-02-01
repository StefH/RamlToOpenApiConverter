using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter
{
    public partial class RamlConverter
    {
        private OpenApiComponents? MapComponents(IDictionary<object, object> types)
        {
            if (types == null)
            {
                return null;
            }

            var components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>()
            };

            foreach (var key in types.Keys.OfType<string>())
            {
                switch (types[key])
                {
                    case IDictionary<object, object> values:
                        var type = values.Get("type");
                        if (type == "object" || types.ContainsKey(type))
                        {
                            var required = values.GetAsCollection("required");
                            var properties = values.GetAsDictionary("properties");
                            components.Schemas.Add(key, MapSchema(properties, required));
                        }

                        if (values.ContainsKey("enum"))
                        {
                            var enumAsCollection = values.GetAsCollection("enum").OfType<string>();
                            var enumValues = enumAsCollection
                                .SelectMany(e => e.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                                .Select(x => new OpenApiString(x.Trim()));

                            var schema = new OpenApiSchema
                            {
                                Type = "string",
                                Enum = enumValues.OfType<IOpenApiAny>().ToList()
                            };
                            components.Schemas.Add(key, schema);
                        }

                        string? arrayType = null;
                        if (type == "array" && values.TryGetValue("items", out var arrayItem))
                        {
                            arrayType = arrayItem as string;
                        }
                        else if (type.EndsWith("[]"))
                        {
                            arrayType = type.Substring(0, type.Length - 2);

                        }

                        if (arrayType is not null)
                        {
                            if (components.Schemas.ContainsKey(arrayType))
                            {
                                components.Schemas.Add(key, new OpenApiSchema
                                {
                                    Type = "array",
                                    Items = CreateDummyOpenApiReferenceSchema(arrayType)
                                });
                            }
                        }

                        break;

                    case string jsonOrYaml:
                        var items = _deserializer.Deserialize<IDictionary<object, object>>(jsonOrYaml);
                        var requiredX = items.GetAsCollection("required");
                        var propertiesX = items.GetAsDictionary("properties");
                        components.Schemas.Add(key, MapSchema(propertiesX, requiredX));
                        break;
                }
            }

            return components.Schemas.Count > 0 ? components : null;
        }

        private OpenApiSchema MapSchema(IDictionary<object, object> properties, ICollection<object> required)
        {
            return new OpenApiSchema
            {
                Type = "object",
                Required = required != null ? new HashSet<string>(required.OfType<string>()) : null,
                Properties = MapProperties(properties, required)
            };
        }

        private IDictionary<string, OpenApiSchema> MapProperties(IDictionary<object, object> properties, ICollection<object>? required)
        {
            var openApiProperties = new Dictionary<string, OpenApiSchema>();
            foreach (var key in properties.Keys.OfType<string>())
            {
                OpenApiSchema schema;

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

                string propertyType = values.Get("type");
                if (propertyType == "object")
                {
                    // Object
                    var props = values.GetAsDictionary("properties");
                    var req = values.GetAsCollection("required");
                    schema = MapSchema(props, req);
                }
                else if (propertyType != null && _types.ContainsKey(propertyType))
                {
                    var simpleType = _types.GetAsDictionary(propertyType);
                    var props = simpleType?.GetAsDictionary("properties");
                    schema = props != null ?
                        CreateDummyOpenApiReferenceSchema(propertyType) : // Custom type
                        MapParameterOrPropertyDetailsToSchema(simpleType); //  Simple Type
                }
                else
                {
                    // Normal property
                    schema = MapParameterOrPropertyDetailsToSchema(values);
                }

                openApiProperties.Add(key, schema);
            }

            return openApiProperties;
        }

        private IDictionary<object, object> ReplaceUses(IDictionary<object, object> source, IDictionary<object, object> uses)
        {
            //replace uses in file

            IDictionary<object, object> useReplace = new Dictionary<object, object>();
            useReplace = ReplaceIs((IDictionary<object, object>)source, uses);
            source.Replace(useReplace, Constants.IsTag);

            if (uses.Count > 0)
            {
                foreach (var pathValue in source)
                {
                    if (pathValue.Value.GetType() == typeof(Dictionary<object, object>))
                    {
                        useReplace = ReplaceIs((IDictionary<object, object>)pathValue.Value, uses);
                        ((IDictionary<object, object>)pathValue.Value).Replace(useReplace, Constants.IsTag);
                    }
                }
            }
            return source;
        }

        private IDictionary<object, object> ReplaceIs(IDictionary<object, object> source, IDictionary<object, object> uses)
        {
            IDictionary<object, object> useReplace = new Dictionary<object, object>();
            if (uses.Count > 0)
            {
                var _is_val = ((IDictionary<object, object>)source).GetAsString(Constants.IsTag);
                if (_is_val != null)
                {
                    var path_is_separator = _is_val.ToString().Split('.');
                    foreach (var use in uses)
                    {
                        if (use.Key.ToString() == path_is_separator[0].ToString())
                        {
                            useReplace = ((IDictionary<object, object>)use.Value).GetAsDictionary(Constants.Traits);

                            for (int i = 1; i < path_is_separator.Count(); i++)
                            {
                                useReplace = useReplace.GetAsDictionary(path_is_separator[i]);
                            }
                        }
                    }
                }
            }

            return useReplace;
        }
    }
}