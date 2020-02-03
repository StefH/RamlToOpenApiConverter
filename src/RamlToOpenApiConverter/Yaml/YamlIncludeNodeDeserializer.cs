using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace RamlToOpenApiConverter.Yaml
{
    public class YamlIncludeNodeDeserializer3 : IValueDeserializer
    {
        private readonly YamlIncludeNodeDeserializerOptions _options;

        public YamlIncludeNodeDeserializer3(YamlIncludeNodeDeserializerOptions options)
        {
            _options = options;
        }

        public object DeserializeValue(IParser parser, Type expectedType, SerializerState state, IValueDeserializer nestedObjectDeserializer)
        {
            throw new NotImplementedException();
        }

        bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (parser.TryConsume(out Scalar scalar) && scalar.Tag == "!include")
            {
                string fileName = scalar.Value;
                string includePath = Path.Combine(_options.DirectoryName, scalar.Value);
                var includeText = File.ReadAllText(includePath);

                value = _options.Deserializer.Deserialize(includeText, expectedType);

                _options.InlineTypeCallback.Add(Path.GetFileNameWithoutExtension(fileName), value as IDictionary<object, object>);

                parser.MoveNext();
                return true;
            }

            //var scalar = parser.Peek<Scalar>();

            //if (scalar != null && scalar.Tag == "!include")
            //{
            //    string fileName = scalar.Value;
            //    string includePath = Path.Combine(_options.DirectoryName, scalar.Value);
            //    var includeText = File.ReadAllText(includePath);

            //    value = _options.Deserializer.Deserialize(includeText, expectedType);

            //    _options.InlineTypeCallback.Add(Path.GetFileNameWithoutExtension(fileName), value as IDictionary<object, object>);

            //    parser.MoveNext();
            //    return true;
            //}

            //value = null;
            //return false;

            //if (!parser.TryConsume<MappingStart>(out var mapping))
            //{
            //    value = null;
            //    return false;
            //}

            //while (!parser.TryConsume<MappingEnd>(out var _))
            //{
            //    if (!parser.TryConsume<Scalar>(out Scalar scalar))
            //    {
            //        parser.SkipThisAndNestedEvents();
            //    }


            //}

            value = null;
            return true;
        }

        
    }
}