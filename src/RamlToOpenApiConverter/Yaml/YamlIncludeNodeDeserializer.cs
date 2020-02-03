﻿using System;
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
            if (parser.Accept(out Scalar scalar) && scalar.Tag == "!include")
            {
                string fileName = scalar.Value;
                string includePath = Path.Combine(_options.DirectoryName, fileName);

                
                using (var includedFile = File.OpenText(includePath))
                {
                    var includeRef = (IncludeRef) _options.Deserializer.Deserialize(new Parser(includedFile), expectedType);
                    includeRef.FileName = fileName;

                    value = includeRef;

                    parser.MoveNext();
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}

// Path.GetFileNameWithoutExtension(fileName)