namespace RamlToOpenApiConverterTest.Extensions
{
    internal static class StringExtensions
    {
        public static string NormalizeNewLines(this string x)
        {
            return x?.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}