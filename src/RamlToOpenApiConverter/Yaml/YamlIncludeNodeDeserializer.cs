using System;
using System.IO;
using RamlToOpenApiConverter.Builders;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RamlToOpenApiConverter.Yaml
{
    public class YamlIncludeNodeDeserializer : INodeDeserializer
    {
        private readonly YamlIncludeNodeDeserializerOptions _options;

        public YamlIncludeNodeDeserializer(YamlIncludeNodeDeserializerOptions options)
        {
            _options = options;
        }

        bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (parser.Accept(out Scalar scalar) && scalar.Tag == Constants.IncludeTag)
            {
                string fileName = scalar.Value.Replace('/', Path.DirectorySeparatorChar);
                string includePath = Path.Combine(_options.DirectoryName, fileName);
                var deserializer = IncludeNodeDeserializerBuilder.Build(Path.GetDirectoryName(includePath));
                FileInfo fi = new FileInfo(includePath);
                string extn = fi.Extension;
                if(extn.ToLower() == ".json")
                {
                    string includedJson = File.ReadAllText(includePath);
                    parser.MoveNext();
                    value = includedJson;
                    return true;
                }
                else
                {
                using (var includedFileText = File.OpenText(includePath))
                {
                    var includeRef = (IncludeRef)deserializer.Deserialize(new Parser(includedFileText), expectedType);
                    includeRef.FileName = fileName;
                    parser.MoveNext();
                    value = includeRef;
                    return true;
                }
                }
            }

            value = null;
            return false;
        }
    }
}