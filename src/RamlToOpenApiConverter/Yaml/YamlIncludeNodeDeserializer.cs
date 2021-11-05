using System;
using System.Collections.Generic;
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

                using (var includedFileTextReader = File.OpenText(includePath))
                {
                    IncludeRef? includeRef = null;
                    try
                    {
                        // var items = deserializer.Deserialize<IDictionary<object, object>>(includedFileTextReader);
                        includeRef = deserializer.Deserialize(new Parser(includedFileTextReader), expectedType) as IncludeRef;
                    }
                    catch
                    {
                        includeRef = new IncludeRef
                        {
                            { "x", "y"}
                        };
                    }
                    
                    // includeRef.FileName = fileName;

                    parser.MoveNext();

                    value = includeRef;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}