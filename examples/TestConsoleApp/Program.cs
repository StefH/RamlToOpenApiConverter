using Microsoft.OpenApi;
using RamlToOpenApiConverter;

namespace TestConsoleApp
{
    // https://mulesoft.github.io/oas-raml-converter/
    class Program
    {
        static void Main(string[] args)
        {
            new RamlConverter().ConvertToFile("Examples\\MediaWiki.raml", "..\\..\\..\\Examples\\MediaWiki.converted.json", OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
        }
    }
}