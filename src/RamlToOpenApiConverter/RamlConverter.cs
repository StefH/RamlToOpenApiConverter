using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RamlToOpenApiConverter.Extensions;
using RamlToOpenApiConverter.Yaml;
using SharpYaml.Serialization;
using YamlDotNet.Serialization;

//using YamlDotNet.Serialization;

// using SharpYaml.Serialization;

namespace RamlToOpenApiConverter
{
    public partial class RamlConverter
    {
        private readonly IDictionary<object, object> _types = new Dictionary<object, object>();

        private OpenApiDocument _doc;

        /// <summary>
        /// Converts the input RAML file to an Open API Specification output file using the provided options.
        /// </summary>
        /// <param name="inputPath">The path to the RAML file.</param>
        /// <param name="outputPath">The path to the generated Open API Specification file.</param>
        /// <param name="specVersion">The Open API specification version.</param>
        /// <param name="format">Open API document format.</param>
        public void ConvertToFile(string inputPath, string outputPath, OpenApiSpecVersion specVersion = OpenApiSpecVersion.OpenApi3_0, OpenApiFormat format = OpenApiFormat.Json)
        {
            //var document = ConvertToOpenApiDocument(File.OpenRead(inputPath));
            var document = ConvertToOpenApiDocument(inputPath);

            string contents = document.Serialize(specVersion, format);

            File.WriteAllText(outputPath, contents);
        }

        /// <summary>
        /// Converts the input RAML stream to an Open API Specification document.
        /// </summary>
        /// <param name="inputPath">The path to the RAML file.</param>
        public OpenApiDocument ConvertToOpenApiDocument(string inputPath)
        {
            //var settingz = new SerializerSettings();
            //settingz.RegisterTagMapping("!include", typeof(IncludeRef));
            //settingz.RegisterSerializer(typeof(IncludeRef), new SharpYamlIncludeRefSerializer(Path.GetDirectoryName(inputPath), new IncludeRefCallback(_types)));

            //var serializer = new SharpYaml.Serialization.Serializer(settingz);


            //var result = serializer.Deserialize<IDictionary<object, object>>(File.ReadAllText(inputPath), out var __c);




            var builder = new DeserializerBuilder();

            var includeNodeDeserializer = new YamlIncludeNodeDeserializer(new YamlIncludeNodeDeserializerOptions
            {
                DirectoryName = Path.GetDirectoryName(inputPath),
                Deserializer = builder.Build(),
                IncludeRefCallback = new IncludeRefCallback(_types)
            });

            var deserializer = builder
                .WithTagMapping("!include", typeof(IncludeRef))
                .WithNodeDeserializer(includeNodeDeserializer, s => s.OnTop())
                .Build();

            var result = deserializer.Deserialize<Dictionary<object, object>>(File.ReadAllText(inputPath));

            // Step 1 - Get all types
            var types = result.GetAsDictionary("types");
            if (types != null)
            {
                foreach (var type in types.Where(x => !_types.ContainsKey(x.Key)))
                {
                    _types.Add(type.Key, type.Value);
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