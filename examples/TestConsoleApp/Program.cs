using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.OpenApi;
using Microsoft.OpenApi.YamlReader;
using RamlToOpenApiConverter;

namespace TestConsoleApp;

// https://mulesoft.github.io/oas-raml-converter/
class Program
{
    private static readonly string DestFolder = Path.GetFullPath("..\\..\\..\\Examples");

    static void Main(string[] args)
    {
        new RamlConverter().ConvertToFile("Examples\\exampleinclude\\exampletest.raml", Path.Combine(DestFolder, "exampleinclude.json"));
        new RamlConverter().ConvertToFile("Examples\\ArrayExample.raml", Path.Combine(DestFolder, "ArrayExample.json"));
        new RamlConverter().ConvertToFile("Examples\\MuleSoft\\test.raml", Path.Combine(DestFolder, "MuleSoft.converted.json"));
        new RamlConverter().ConvertToFile("Examples\\InheritedDatatype\\simpleinherited.raml", Path.Combine(DestFolder, "InheritedDatatype", "simpleinherited.json"));

        new RamlConverter().ConvertToFile("Examples\\HelloWorld.raml", Path.Combine(DestFolder, "HelloWorld.converted.json"));
        new RamlConverter().ConvertToFile("Examples\\IncludePerson\\api.raml", Path.Combine(DestFolder, "IncludePerson\\api.converted.json"));
        new RamlConverter().ConvertToFile("Examples\\MediaWiki.raml", Path.Combine(DestFolder, "MediaWiki.converted.json"));

        Console.WriteLine("DONE");
        var doc = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/example"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new OpenApiOperation
                        {
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    // No description
                                }
                            }
                        }
                    }
                }
            }
        };

        using var stringWriter = new StringWriter();
        doc.SerializeAs(OpenApiSpecVersion.OpenApi3_0, new OpenApiJsonWriter(stringWriter));
        var result = stringWriter.ToString();

        int x = 0;
    }
}