using System;
using System.Collections.Generic;
using System.Linq;
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
                    IDictionary<object, object> items = null;

                    switch (types[key])
                    {
                        case IDictionary<object, object> values:
                            bool isObject = values.Get("type") == "object";

                            if (isObject)
                            {
                                items = values;
                            }
                            break;

                        case string jsonOrYaml:
                            items = _deserializer.Deserialize<IDictionary<object, object>>(jsonOrYaml);
                            break;
                    }

                    if (items != null)
                    {
                        var required = items.GetAsCollection("required");
                        var properties = items.GetAsDictionary("properties");
                        components.Schemas.Add(key, MapSchema(properties, required));
                    }
                }
            }

            return components;
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

        private OpenApiSchema MapProperty(IDictionary<object, object> values)
        {
            // bool required = values.Get<bool?>("required") == true;
            string type = values.Get("type");
            string format = values.Get("format");

            if (type == "datetime")
            {
                type = "string";
                format = "date-time";
            }

            return new OpenApiSchema
            {
                Type = type,
                Format = format,
                // Nullable = !required,
                Description = values.Get("description"),
                Minimum = values.Get<decimal?>("minimum"),
                Maximum = values.Get<decimal?>("maximum"),
                MaxLength = values.Get<int?>("maxLength"),
                MinLength = values.Get<int?>("minLength")
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
                        values = new Dictionary<object, object>();
                        values.Add("type", stringValue);
                        values.Add("properties", new Dictionary<object, object>());
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
                    schema = MapProperty(simpleType);
                }
                else
                {
                    // Normal property
                    schema = MapProperty(values);
                }

                openApiProperties.Add(key, schema);
            }

            return openApiProperties;
        }
    }
}