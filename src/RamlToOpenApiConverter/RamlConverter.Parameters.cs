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
                var schema = MapParameterOrPropertyDetailsToSchema(parameterDetails);

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

        private OpenApiSchema MapParameterOrPropertyDetailsToSchema(IDictionary<object, object> details)
        {
            string schemaTypeFromRaml = details.Get("type");
            string schemaFormatFromRaml = details.Get("format");

            var schemaTypes = (schemaTypeFromRaml ?? "string")
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).ToList();
            
            // https://github.com/raml-org/raml-spec/blob/master/versions/raml-10/raml-10.md#nil-type
            bool isNil = schemaTypes.Contains("nil");

            var schemaTypesExceptNil = schemaTypes.Where(s => s != "nil").Distinct().ToList();
            string schemaType = schemaTypesExceptNil.Count > 1 ? "object" : schemaTypesExceptNil.FirstOrDefault() ?? "string";

            var schema = new OpenApiSchema
            {
                Nullable = isNil,
                Minimum = details.Get<decimal?>("minimum"),
                Maximum = details.Get<decimal?>("maximum"),
                MaxLength = details.Get<int?>("maxLength"),
                MinLength = details.Get<int?>("minLength")
            };

            switch (schemaType)
            {
                case "datetime":
                    schema.Type = "string";
                    schema.Format = "date-time";
                    break;

                case "number":
                    switch (schemaFormatFromRaml)
                    {
                        case "long":
                        case "int64":
                            schema.Type = "integer";
                            schema.Format = "int64";
                            break;

                        case "float":
                            schema.Type = "number";
                            schema.Format = "float";
                            break;

                        case "double":
                            schema.Type = "number";
                            schema.Format = "double";
                            break;

                        default:
                            schema.Type = "integer";
                            schema.Format = "int";
                            break;
                    }

                    return schema;

                default:
                    // Check if the SchemaType is defined as enum, simple or complex type in the _types list
                    if (_types.ContainsKey(schemaType))
                    {
                        return CreateDummyOpenApiReferenceSchema(schemaType);
                        //var childDetails = _types.GetAsDictionary(schemaType);
                        //return MapParameterOrPropertyDetailsToSchema(childDetails);
                    }

                    //string isEnum = details.Keys.OfType<string>().FirstOrDefault(k => k == "enum");
                    //if (isEnum != null)
                    //{
                    //    var enumAsCollection = details.GetAsCollection(isEnum).OfType<string>();
                    //    var enumValues = enumAsCollection
                    //        .SelectMany(e => e.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                    //        .Select(x => new OpenApiString(x.Trim()));

                    //    schema.Type = "string";
                    //    schema.Enum = enumValues.OfType<IOpenApiAny>().ToList();
                    //}
                    //else
                    //{
                    //    schema.Type = schemaType;
                    //    schema.Format = schemaFormatFromRaml;
                    //}

                    schema.Type = schemaType;
                    schema.Format = schemaFormatFromRaml;

                    break;
            }

            return schema;
        }
    }
}