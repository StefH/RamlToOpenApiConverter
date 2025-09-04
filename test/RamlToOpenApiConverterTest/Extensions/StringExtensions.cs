namespace RamlToOpenApiConverterTest.Extensions;

internal static class StringExtensions
{
    public static string NormalizeNewLines(this string source)
    {
        return source.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}