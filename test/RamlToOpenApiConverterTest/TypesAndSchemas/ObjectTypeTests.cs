using System.IO;
using FluentAssertions;
using RamlToOpenApiConverter;
using Xunit;

namespace RamlToOpenApiConverterTest.TypesAndSchemas
{
    public class ObjectTypeTests
    {
        private readonly RamlConverter _sut;

        public ObjectTypeTests()
        {
            _sut = new RamlConverter();
        }

        [Fact]
        public void CanConvertTypes_Inline()
        {
            // Arrange
            string expected = File.ReadAllText(Path.Combine("TypesAndSchemas", "TypesObjectInline.json"));

            // Act
            string result = _sut.Convert(Path.Combine("TypesAndSchemas", "TypesObjectInline.raml"));

            // Assert
            result.Should().Be(expected);
        }
    }
}