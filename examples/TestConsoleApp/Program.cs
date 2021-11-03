using System;
using System.IO;
using Microsoft.OpenApi;
using RamlToOpenApiConverter;

namespace TestConsoleApp
{
    // https://mulesoft.github.io/oas-raml-converter/
    class Program
    {
        private const string DestFolder = "..\\..\\..\\Examples\\";

        static void Main(string[] args)
        {
            new RamlConverter().ConvertToFile("Examples\\ArrayExample.raml", Path.Combine(DestFolder, "ArrayExample.json"));

            //new RamlConverter().ConvertToFile("Examples\\MuleSoft\\test.raml", Path.Combine(DestFolder, "MuleSoft.converted.json"));

            //new RamlConverter().ConvertToFile("Examples\\HelloWorld.raml", Path.Combine(DestFolder, "HelloWorld.converted.json"));

            //new RamlConverter().ConvertToFile("Examples\\IncludePerson\\api.raml", Path.Combine(DestFolder, "IncludePerson\\api.converted.json"));

            //new RamlConverter().ConvertToFile("Examples\\MediaWiki.raml", Path.Combine(DestFolder, "MediaWiki.converted.json"));

            Console.WriteLine("DONE");
        }
    }
}