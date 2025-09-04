using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi;
using RamlToOpenApiConverter.Builders;
using RamlToOpenApiConverter.Extensions;
using YamlDotNet.Serialization;

namespace RamlToOpenApiConverter;

public partial class RamlConverter
{
    private readonly IDictionary<object, object> _types = new Dictionary<object, object>();
    private readonly IDictionary<object, object> _uses = new Dictionary<object, object>();

    private IDeserializer _deserializer = null!;
    private OpenApiDocument _doc = null!;

    /// <summary>
    /// Converts the input RAML file to an Open API Specification output string using the provided options.
    /// </summary>
    /// <param name="inputPath">The path to the RAML file.</param>
    /// <param name="specificationVersion">The Open API specification version.</param>
    /// <param name="format">Open API document format.</param>
    public string Convert(string inputPath, OpenApiSpecificationVersion specificationVersion = OpenApiSpecificationVersion.OpenApi3_0, string format = OpenApiConstants.Json)
    {
        var document = ConvertToOpenApiDocument(inputPath, specificationVersion);

        using var stringWriter = new StringWriter();
        IOpenApiWriter openApiWriter = format == OpenApiConstants.Json ? new OpenApiJsonWriter(stringWriter) : new OpenApiYamlWriter(stringWriter);

        document.SerializeAs((OpenApiSpecVersion)specificationVersion, openApiWriter);

        return stringWriter.ToString();
    }

    /// <summary>
    /// Converts the input RAML file to an Open API Specification output file using the provided options.
    /// </summary>
    /// <param name="inputPath">The path to the RAML file.</param>
    /// <param name="outputPath">The path to the generated Open API Specification file.</param>
    /// <param name="specificationVersion">The Open API specification version.</param>
    /// <param name="format">Open API document format.</param>
    public void ConvertToFile(string inputPath, string outputPath, OpenApiSpecificationVersion specificationVersion = OpenApiSpecificationVersion.OpenApi3_0, string format = OpenApiConstants.Json)
    {
        var contents = Convert(inputPath, specificationVersion, format);

        File.WriteAllText(outputPath, contents);
    }

    /// <summary>
    /// Converts the input RAML stream to an Open API Specification document.
    /// </summary>
    /// <param name="inputPath">The path to the RAML file.</param>
    /// <param name="specificationVersion">The Open API specification version.</param>
    public OpenApiDocument ConvertToOpenApiDocument(string inputPath, OpenApiSpecificationVersion specificationVersion = OpenApiSpecificationVersion.OpenApi3_0)
    {
        var specVersion = (OpenApiSpecVersion)specificationVersion;

        _deserializer = IncludeNodeDeserializerBuilder.Build(Path.GetDirectoryName(inputPath)!);

        var result = _deserializer.Deserialize<IDictionary<object, object>>(File.ReadAllText(inputPath));

        // Step 1 - Get all uses
        var uses = result.GetAsDictionary("uses");
        if (uses != null)
        {
            foreach (var use in uses.Where(x => !_uses.ContainsKey(x.Key)))
            {
                _uses.Add(use.Key, use.Value);
            }
            result.Remove("uses");
            result = ReplaceUses(result, _uses);
        }

        // Step 2 - Get all types and schemas
        var types = result.GetAsDictionary("types");
        if (types != null)
        {
            foreach (var type in types.Where(x => !_types.ContainsKey(x.Key)))
            {
                _types.Add(type.Key, type.Value);
            }
        }
        var schemas = result.GetAsDictionary("schemas");
        if (schemas != null)
        {
            foreach (var schema in schemas.Where(x => !_types.ContainsKey(x.Key)))
            {
                _types.Add(schema.Key, schema.Value);
            }
        }

        _doc = new OpenApiDocument
        {
            // Step 3 - Get Info, Servers and Components
            Info = MapInfo(result),
            Servers = MapServers(result),
            Components = MapComponents(_types, specVersion),

            // Step 4 - Get Paths
            Paths = MapPaths(result, _uses, specVersion)
        };

        // Check if valid
        using var stringWriter = new StringWriter();
        _doc.SerializeAs(specVersion, new OpenApiJsonWriter(stringWriter));

        return _doc;
    }
}