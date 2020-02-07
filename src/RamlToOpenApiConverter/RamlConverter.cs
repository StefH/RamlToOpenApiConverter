using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using RamlToOpenApiConverter.Extensions;
using RamlToOpenApiConverter.Yaml;
using YamlDotNet.Serialization;

namespace RamlToOpenApiConverter
{
    public partial class RamlConverter
    {
        private readonly IDictionary<object, object> _types = new Dictionary<object, object>();

        private IDeserializer _deserializer;
        private OpenApiDocument _doc;

        /// <summary>
        /// Converts the input RAML file to an Open API Specification output string using the provided options.
        /// </summary>
        /// <param name="inputPath">The path to the RAML file.</param>
        /// <param name="specVersion">The Open API specification version.</param>
        /// <param name="format">Open API document format.</param>
        public string Convert(string inputPath, OpenApiSpecVersion specVersion = OpenApiSpecVersion.OpenApi3_0, OpenApiFormat format = OpenApiFormat.Json)
        {
            var document = ConvertToOpenApiDocument(inputPath);

            return document.Serialize(specVersion, format);
        }

        /// <summary>
        /// Converts the input RAML file to an Open API Specification output file using the provided options.
        /// </summary>
        /// <param name="inputPath">The path to the RAML file.</param>
        /// <param name="outputPath">The path to the generated Open API Specification file.</param>
        /// <param name="specVersion">The Open API specification version.</param>
        /// <param name="format">Open API document format.</param>
        public void ConvertToFile(string inputPath, string outputPath, OpenApiSpecVersion specVersion = OpenApiSpecVersion.OpenApi3_0, OpenApiFormat format = OpenApiFormat.Json)
        {
            string contents = Convert(inputPath, specVersion, format);

            File.WriteAllText(outputPath, contents);
        }

        /// <summary>
        /// Converts the input RAML stream to an Open API Specification document.
        /// </summary>
        /// <param name="inputPath">The path to the RAML file.</param>
        public OpenApiDocument ConvertToOpenApiDocument(string inputPath)
        {
            var builder = new DeserializerBuilder();

            var includeNodeDeserializer = new YamlIncludeNodeDeserializer(new YamlIncludeNodeDeserializerOptions
            {
                DirectoryName = Path.GetDirectoryName(inputPath),
                Deserializer = builder.Build()
            });

            _deserializer = builder
                .WithTagMapping(Constants.IncludeTag, typeof(IncludeRef))
                .WithNodeDeserializer(includeNodeDeserializer, s => s.OnTop())
                .Build();

            var result = _deserializer.Deserialize<Dictionary<object, object>>(File.ReadAllText(inputPath));

            // Step 1 - Get all types and schemas
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

            // Step 2 - Get Info, Servers and Components
            _doc = new OpenApiDocument
            {
                Info = MapInfo(result),
                Servers = MapServers(result),
                Components = MapComponents(_types)
            };

            // Step 3 - Get Paths
            _doc.Paths = MapPaths(result);

            // Check if valid
            var text = _doc.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);

            return _doc;
        }
    }
}