using System;
using System.Collections.Generic;
using System.IO;
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

        bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (parser.TryConsume(out Scalar scalar) && scalar.Tag == "!include")
            {
                string fileName = scalar.Value;
                string includePath = Path.Combine(_options.DirectoryName, scalar.Value);
                var includeText = File.ReadAllText(includePath);

                value = _options.Deserializer.Deserialize(includeText, expectedType);

                // TODO : how to get the TAG name here ??
                string tagname = "???";

                _options.InlineTypeCallback.Add(tagname, value as IDictionary<object, object>);

                parser.MoveNext();
                return true;
            }

            value = null;
            return true;
        }
    }
}

// Path.GetFileNameWithoutExtension(fileName)