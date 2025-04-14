# RamlToOpenApiConverter
Converts a RAML to Open API Specification

[![NuGet: RamlToOpenApiConverter](https://img.shields.io/nuget/v/RamlToOpenApiConverter)](https://www.nuget.org/packages/RamlToOpenApiConverter)

## Usage

### Convert a RAML file
``` c#
new RamlConverter()
  .ConvertToFile("MediaWiki.raml", "MediaWiki.json", OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
```


### Limits
- ...


## Details
This project uses the following tools:
- YamlDotNet --> to read the RAML (as YAML or JSON)
- Microsoft.OpenApi.YamlReader --> to process the Open API Model and convert the model to the output file