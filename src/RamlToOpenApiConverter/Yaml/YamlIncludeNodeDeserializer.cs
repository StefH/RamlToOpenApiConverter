using System;
using System.IO;
using System.Text.RegularExpressions;
using RamlToOpenApiConverter.Builders;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RamlToOpenApiConverter.Yaml
{
    public class YamlIncludeNodeDeserializer : INodeDeserializer
    {
        private static readonly Regex JsonExtensionRegex = new(@"^\.json$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex YamlExtensionRegex = new(@"^\.yaml$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

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
                    var extension = Path.GetExtension(fileName);
                    if (YamlExtensionRegex.IsMatch(extension))
                    {
                        value = deserializer.Deserialize(new Parser(includedFileTextReader), expectedType);
                    }
                    else if (JsonExtensionRegex.IsMatch(extension))
                    {
                        value = includedFileTextReader.ReadToEnd();
                    }
                    else
                    {
                        throw new NotSupportedException($"The file extension '{extension}' is not supported in a '{Constants.IncludeTag}' tag.");
                    }

                    parser.MoveNext();

                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}