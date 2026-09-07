using System.Text.RegularExpressions;
using RamlToOpenApiConverter.Builders;

namespace RamlToOpenApiConverter.Yaml;

internal static class YamlIncludeNodeDeserializer
{
    private static readonly Regex JsonExtensionRegex = new(@"^\.json$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));
    private static readonly Regex RamlExtensionRegex = new(@"^\.raml$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));

    public static object ResolveIncludes(object value, string directoryName)
    {
        if (value is IDictionary<object, object> dictionary)
        {
            var keys = dictionary.Keys.ToArray();
            foreach (var key in keys)
            {
                dictionary[key] = ResolveIncludes(dictionary[key], directoryName);
            }

            return dictionary;
        }

        if (value is ICollection<object> collection)
        {
            var resolvedItems = collection.Select(item => ResolveIncludes(item, directoryName)).ToArray();
            collection.Clear();

            foreach (var item in resolvedItems)
            {
                collection.Add(item);
            }

            return collection;
        }

        if (value is string fileName)
        {
            var normalizedFileName = fileName.Replace('/', Path.DirectorySeparatorChar);
            var extension = Path.GetExtension(normalizedFileName);
            if (RamlExtensionRegex.IsMatch(extension) || JsonExtensionRegex.IsMatch(extension))
            {
                var includePath = Path.Combine(directoryName, normalizedFileName);
                return ReadIncludedFile(includePath);
            }
        }

        return value;
    }

    private static object ReadIncludedFile(string includePath)
    {
        var extension = Path.GetExtension(includePath);

        if (RamlExtensionRegex.IsMatch(extension))
        {
            var deserializer = IncludeNodeDeserializerBuilder.Build();
            var value = deserializer.Deserialize<IDictionary<object, object>>(File.ReadAllText(includePath))!;
            return ResolveIncludes(value, Path.GetDirectoryName(includePath)!);
        }

        if (JsonExtensionRegex.IsMatch(extension))
        {
            return File.ReadAllText(includePath);
        }

        throw new NotSupportedException($"The file extension '{extension}' is not supported in a '{Constants.IncludeTag}' tag.");
    }
}