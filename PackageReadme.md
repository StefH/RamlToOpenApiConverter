## RamlToOpenApiConverter
Converts a RAML to Open API Specification

### Usage

#### Convert a RAML file
``` c#
new RamlConverter()
  .ConvertToFile("MediaWiki.raml", "MediaWiki.json", OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
```
