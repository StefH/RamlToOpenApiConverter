using Microsoft.OpenApi.Models;

namespace RamlToOpenApiConverter.Extensions;

internal static class JsonSchemaTypeExtensions
{
    internal static JsonSchemaType AddNullable(this JsonSchemaType jsonSchemaType, bool nullable)
    {
        if (!nullable)
        {
            return jsonSchemaType;
        }

        return jsonSchemaType | JsonSchemaType.Null;
    }
}