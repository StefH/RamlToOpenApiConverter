﻿using System;
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
                    var enumValues = enumAsCollection.SelectMany(e => e.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
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

        private (string Type, string Format, bool Required, bool Nullable) MapSchemaTypeAndFormat(string schemaTypeFromRaml, string schemaFormat, bool required)
        {
            var schemaTypes = (schemaTypeFromRaml ?? "string").Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            var allSchemaTypes = schemaTypes.Where(s => s != "nil").Distinct().ToList();
            bool isUnion = allSchemaTypes.Count > 1;

            string schemaType = isUnion ? "object" : allSchemaTypes.FirstOrDefault() ?? "string";

            // https://github.com/raml-org/raml-spec/blob/master/versions/raml-10/raml-10.md#nil-type
            bool isNil = schemaTypes.Contains("nil");

            switch (schemaType)
            {
                case "datetime":
                    return ("string", "date-time", required, isNil);

                case "number":
                    switch (schemaFormat)
                    {
                        case "long":
                        case "int64":
                            return ("integer", "int64", required, isNil);

                        case "float":
                            return ("number", "float", required, isNil);

                        case "double":
                            return ("number", "double", required, isNil);

                        default:
                            return ("integer", "int", required, isNil);
                    }

                default:
                    // Check if the SchemaType is defined as simple or complex type in the _types list
                    if (_types.ContainsKey(schemaType))
                    {
                        var parameterDetails = _types.GetAsDictionary(schemaType);
                        return MapSchemaTypeAndFormat(
                            parameterDetails.Get("type"),
                            parameterDetails.Get("format"),
                            parameterDetails.Get<bool?>("required") ?? false);
                    }

                    return (schemaType, schemaFormat, required, isNil);
            }
        }
    }
}