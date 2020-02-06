using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RamlToOpenApiConverter.Extensions;

namespace RamlToOpenApiConverter
{
    public partial class RamlConverter
    {
        private IList<OpenApiParameter> MapParameters(IDictionary<object, object> values)
        {
            var parameters = new List<OpenApiParameter>();

            parameters.AddRange(MapParameters(values.GetAsDictionary("queryParameters"), ParameterLocation.Query));
            parameters.AddRange(MapParameters(values.GetAsDictionary("uriParameters"), ParameterLocation.Path));
            parameters.AddRange(MapParameters(values.GetAsDictionary("headers"), ParameterLocation.Header));

            return parameters;
        }

        private IList<OpenApiParameter> MapParameters(IDictionary<object, object> parameters, ParameterLocation parameterLocation)
        {
            var openApiParameters = new List<OpenApiParameter>();

            if (parameters == null)
            {
                return openApiParameters;
            }

            foreach (string key in parameters.Keys.OfType<string>())
            {
                var parameterDetails = parameters.GetAsDictionary(key) ?? new Dictionary<object, object>();
                bool required = parameterDetails.Get<bool?>("required") ?? false;
                var map = MapSchemaTypeAndFormat(parameterDetails.Get("type"), parameterDetails.Get("format"), required);

                OpenApiSchema schema = null;

                string isEnum = parameterDetails.Keys.OfType<string>().FirstOrDefault(k => k == "enum");
                if (isEnum != null)
                {
                    var enumAsCollection = parameterDetails.GetAsCollection(isEnum).OfType<string>();
                    var enumValues = enumAsCollection.SelectMany(e => e.Split('|'))
                        .Select(x => new OpenApiString(x.Trim()));

                    schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = enumValues.OfType<IOpenApiAny>().ToList()
                    };
                }
                else
                {
                    schema = new OpenApiSchema
                    {
                        Type = map.Type,
                        Format = map.Format
                    };
                }

                openApiParameters.Add(new OpenApiParameter
                {
                    In = parameterLocation,
                    Description = parameterDetails.Get("description"),
                    Name = key,
                    Required = map.Required,
                    Schema = schema
                });
            }

            return openApiParameters;
        }

        private (string Type, string Format, bool Required) MapSchemaTypeAndFormat(string schemaType, string schemaFormat, bool required)
        {
            switch (schemaType)
            {
                case "datetime":
                    return ("string", "date-time", required);

                case "number":
                    switch (schemaFormat)
                    {
                        case "long":
                        case "int64":
                            return ("integer", "int64", required);

                        case "float":
                            return ("number", "float", required);

                        case "double":
                            return ("number", "double", required);

                        default:
                            return ("integer", "int", required);
                    }

                default:
                    // Check if the SchemaType is defined as simple or complex type in the _types list
                    if (schemaType != null && _types.ContainsKey(schemaType))
                    {
                        var parameterDetails = _types.GetAsDictionary(schemaType);
                        return MapSchemaTypeAndFormat(
                            parameterDetails.Get("type"),
                            parameterDetails.Get("format"),
                            parameterDetails.Get<bool?>("required") ?? false);
                    }

                    return (schemaType ?? "string", schemaFormat, required);
            }
        }
    }
}