using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace RamlToOpenApiConverter;

/// <summary>
/// A wrapper class for JsonNode.
/// Copied from old source-code Microsoft.OpenApi.YamlReader
/// </summary>
internal class OpenApiAny(JsonNode jsonNode) : IOpenApiElement, IOpenApiExtension
{
    //
    // Summary:
    //     Gets the underlying JsonNode.
    public JsonNode Node => jsonNode;

    //
    // Summary:
    //     Writes out the OpenApiAny type.
    //
    // Parameters:
    //   writer:
    //
    //   specVersion:
    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        writer.WriteAny(Node);
    }
}