using System.Xml.Linq;
using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Providers.Nacional;

public class NacionalXmlSerializerRetentionTypeTests
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";
    private readonly NationalDpsManualSerializer _sut = new();

    [Theory]
    [InlineData(1, "1")]
    [InlineData(2, "2")]
    [InlineData(3, "3")]
    public void Given_RetentionType_Should_EmitCorrectTpRetISSQN(int retentionType, string expectedValue)
    {
        // Arrange
        var document = new DpsDocumentBuilder().WithRetentionType(retentionType).Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = XmlParseHelpers.ParseTribMun(result.Xml);
        tribMun.Element(Ns + "tpRetISSQN")?.Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Given_NullRetentionType_Should_DefaultTo1()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = XmlParseHelpers.ParseTribMun(result.Xml);
        tribMun.Element(Ns + "tpRetISSQN")?.Value.ShouldBe("1");
    }

    [Fact]
    public void Given_RetentionType3_WithIntermediary_Should_EmitTpRetISSQN3()
    {
        // Arrange
        var document = new DpsDocumentBuilder()
            .WithIntermediary()
            .WithRetentionType(3)
            .Build();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var tribMun = XmlParseHelpers.ParseTribMun(result.Xml);
        tribMun.Element(Ns + "tpRetISSQN")?.Value.ShouldBe("3");
    }

}
