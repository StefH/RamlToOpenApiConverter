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

 
## Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **RamlToOpenApiConverter**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)