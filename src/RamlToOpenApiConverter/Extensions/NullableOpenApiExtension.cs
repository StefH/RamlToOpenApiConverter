using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;

namespace RamlToOpenApiConverter.Extensions;

internal class NullableOpenApiExtension(bool isNil) : IOpenApiExtension
{
    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        if (specVersion == OpenApiSpecVersion.OpenApi2_0)
        {
            // Version 2 does not support 'null' as the data type
            return;
        }

        if (isNil)
        {
            writer.WriteValue(true);
        }
    }
}