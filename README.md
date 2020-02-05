# RamlToOpenApiConverter
Converts a RAML to Open API Specification

[![NuGet: RamlToOpenApiConverter](https://buildstats.info/nuget/RamlToOpenApiConverter)](https://www.nuget.org/packages/RamlToOpenApiConverter)

## Usage

### Install the NuGet

```
PM> Install-Package RamlToOpenApiConverter
```

### Convert a RAML file
``` c#
new RamlConverter()
  .ConvertToFile("MediaWiki.raml", "MediaWiki.json", OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
        
```

### Limits
- ...

## Details
This project uses the following tools:
- SharpYaml --> to read the RAML (as YAML)
- YamlDotNet --> to read the RAML (as YAML or JSON)
- Microsoft.OpenApi --> to process the Open API Model and convert the model to the output file

