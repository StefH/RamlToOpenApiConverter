using System;
using System.IO;
using System.Text.RegularExpressions;
using RamlToOpenApiConverter.Builders;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RamlToOpenApiConverter.Yaml;

public class YamlIncludeNodeDeserializer : INodeDeserializer
{
    private static readonly Regex JsonExtensionRegex = new(@"^\.json$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex RamlExtensionRegex = new(@"^\.raml$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private readonly YamlIncludeNodeDeserializerOptions _options;

    public YamlIncludeNodeDeserializer(YamlIncludeNodeDeserializerOptions options)
    {
        _options = options;
    }

    bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        if (parser.Accept(out Scalar scalar) && scalar != null)
        {
            string fileName = scalar.Value.Replace('/', Path.DirectorySeparatorChar);
            var extension = Path.GetExtension(fileName);

            if (scalar.Tag == Constants.IncludeTag || (scalar.Tag != Constants.IncludeTag && (RamlExtensionRegex.IsMatch(extension) || JsonExtensionRegex.IsMatch(extension))))
            {
                var includePath = Path.Combine(_options.DirectoryName, fileName);
                value = ReadIncludedFile(includePath, expectedType);
                parser.MoveNext();
                return true;
            }
        }

        value = null;
        return false;
    }

    private static object? ReadIncludedFile(string includePath, Type expectedType)
    {
        var extension = Path.GetExtension(includePath);

        if (RamlExtensionRegex.IsMatch(extension))
        {
            var deserializer = IncludeNodeDeserializerBuilder.Build(Path.GetDirectoryName(includePath));
            return deserializer.Deserialize(new Parser(File.OpenText(includePath)), expectedType);
        }

        if (JsonExtensionRegex.IsMatch(extension))
        {
            return File.ReadAllText(includePath);
        }

        throw new NotSupportedException($"The file extension '{extension}' is not supported in a '{Constants.IncludeTag}' tag.");
    }
}