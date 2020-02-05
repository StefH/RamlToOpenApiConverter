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

                var schema = new OpenApiSchema();
                foreach (string item in parameterDetails.Keys.OfType<string>())
                {
                    switch (item)
                    {
                        case "enum":
                            schema.Type = "string";

                            var enumAsCollection = parameterDetails.GetAsCollection(item).OfType<string>();
                            var enumValues = enumAsCollection.SelectMany(e => e.Split('|')).Select(x => new OpenApiString(x.Trim()));
                            schema.Enum = enumValues.OfType<IOpenApiAny>().ToList();
                            break;

                        default:
                            schema.Type = "string";
                            break;
                    }
                }

                // TODO only string?
                openApiParameters.Add(new OpenApiParameter
                {
                    In = parameterLocation,
                    Name = key,
                    Required = required,
                    Schema = schema
                });
            }

            return openApiParameters;
        }
    }
}