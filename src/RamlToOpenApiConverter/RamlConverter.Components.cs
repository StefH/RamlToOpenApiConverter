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
        private OpenApiComponents MapComponents(IDictionary<object, object> types)
        {
            var components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>()
            };

            if (types != null)
            {
                foreach (var key in types.Keys.OfType<string>())
                {
                    switch (types[key])
                    {
                        case IDictionary<object, object> values:
                            if (values.Get("type") == "object")
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
                            break;

                        case string jsonOrYaml:
                            var items = _deserializer.Deserialize<IDictionary<object, object>>(jsonOrYaml);
                            var requiredX = items.GetAsCollection("required");
                            var propertiesX = items.GetAsDictionary("properties");
                            components.Schemas.Add(key, MapSchema(propertiesX, requiredX));
                            break;
                    }
                }
            }

            return components.Schemas.Count > 0 ? components: null;
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

        private IDictionary<string, OpenApiSchema> MapProperties(IDictionary<object, object> properties, ICollection<object> required)
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
                    // Simple Type
                    var simpleType = _types.GetAsDictionary(propertyType);
                    schema = MapParameterOrPropertyDetailsToSchema(simpleType);
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
    }
}