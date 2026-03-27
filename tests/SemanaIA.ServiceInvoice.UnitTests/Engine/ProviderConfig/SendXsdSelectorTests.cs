using SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Engine.ProviderConfig;

public class SendXsdSelectorTests
{
    private readonly SendXsdSelector _sut = new();

    // ==========================================================
    // Selection by send pattern heuristic
    // ==========================================================

    [Fact]
    public void Given_GissonlineXsdDirectory_Should_SelectEnviarLoteRpsXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("gissonline");

        // Act
        var selectionResult = _sut.Select(xsdDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("enviar-lote-rps-envio-v2_04.xsd");
    }

    [Fact]
    public void Given_NacionalXsdDirectory_Should_SelectDpsXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("nacional");

        // Act
        var selectionResult = _sut.Select(xsdDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("DPS_v1.01.xsd");
    }

    [Fact]
    public void Given_PaulistanaXsdDirectory_Should_SelectPedidoXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("paulistana");

        // Act
        var selectionResult = _sut.Select(xsdDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("PedidoEnvioLoteRPS_v02.xsd");
    }

    [Fact]
    public void Given_SimplissXsdDirectory_Should_SelectNfseXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("simpliss");

        // Act
        var selectionResult = _sut.Select(xsdDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("nfse_v2-03.xsd");
    }

    [Fact]
    public void Given_AbrasfXsdDirectory_Should_SelectMainAbrasfXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("abrasf");

        // Act
        var selectionResult = _sut.Select(xsdDir);

        // Assert
        selectionResult.SelectedFile.ShouldNotBeNull();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("wne_model_xsd_nota_fiscal_abrasf.xsd");
    }

    [Fact]
    public void Given_IssnetXsdDirectory_Should_SelectSchemaXsd()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("issnet");

        // Act
        var selectionResult = _sut.Select(xsdDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("schema_v101.xsd");
    }

    // ==========================================================
    // Override via PrimaryXsdFile
    // ==========================================================

    [Fact]
    public void Given_ProfileWithPrimaryXsdFile_Should_UseOverrideDirectly()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("nacional");
        var profile = new ProviderProfile { PrimaryXsdFile = "DPS_v1.01.xsd" };

        // Act
        var selectionResult = _sut.Select(xsdDir, profile);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("DPS_v1.01.xsd");
        selectionResult.Reason.ShouldContain("PrimaryXsdFile override");
    }

    [Fact]
    public void Given_ProfileWithNonExistentPrimaryXsdFile_Should_ReturnAmbiguous()
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir("nacional");
        var profile = new ProviderProfile { PrimaryXsdFile = "nonexistent.xsd" };

        // Act
        var selectionResult = _sut.Select(xsdDir, profile);

        // Assert
        selectionResult.IsSelected.ShouldBeFalse();
        selectionResult.IsAmbiguous.ShouldBeTrue();
        selectionResult.Reason.ShouldContain("not found");
    }

    // ==========================================================
    // Ambiguity and diagnostics
    // ==========================================================

    [Fact]
    public void Given_DirectoryWithSingleNonExcludedXsd_Should_SelectItUnambiguously()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("single-file.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("single-file.xsd");
        selectionResult.Reason.ShouldContain("Single non-excluded");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_DirectoryWithOnlyXmldsigFile_Should_ReturnNoSelection()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("xmldsig-core-schema.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.SelectedFile.ShouldBeNull();
        selectionResult.IsAmbiguous.ShouldBeFalse();
        selectionResult.Reason.ShouldContain("No suitable send XSD found");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_DirectoryWithOnlyTypesAndXmldsig_Should_ReturnNoSuitableSendXsd()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("tipos_v03.xsd", "xmldsig-core-schema20020212.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.SelectedFile.ShouldBeNull();
        selectionResult.IsAmbiguous.ShouldBeFalse();
        selectionResult.Candidates.Count.ShouldBe(2);
        selectionResult.Reason.ShouldContain("No suitable send XSD found");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_EmptyDirectory_Should_ReturnNoSelection()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.SelectedFile.ShouldBeNull();
        selectionResult.IsAmbiguous.ShouldBeFalse();
        selectionResult.Reason.ShouldContain("No XSD files found");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_DirectoryWithMultipleUnmatchedXsds_Should_ReturnAmbiguousWithCandidates()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("custom-a.xsd", "custom-b.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.IsAmbiguous.ShouldBeTrue();
        selectionResult.Candidates.Count.ShouldBe(2);
        selectionResult.Reason.ShouldContain("No XSD matched send patterns");

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // Exclusion patterns: CompNfse, ConsultarNfse, SubstituirNfse
    // ==========================================================

    [Fact]
    public void Given_DirectoryWithCompNfseAndEnvioXsd_Should_ExcludeCompNfseAndSelectEnvio()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("CompNfse_v1.00.xsd", "servico_enviar_lote_v1.00.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("servico_enviar_lote_v1.00.xsd");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_DirectoryWithConsultarNfseXsd_Should_ExcludeIt()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("ConsultarNfse_v2.02.xsd", "enviar_lote_v2.02.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("enviar_lote_v2.02.xsd");

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // Send patterns: RecepcionarLoteRps, GerarNfse, EnviarLoteRps
    // ==========================================================

    [Fact]
    public void Given_DirectoryWithRecepcionarLoteRpsXsd_Should_SelectAsStrongCandidate()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("RecepcionarLoteRps_v2.02.xsd", "custom-other.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("RecepcionarLoteRps_v2.02.xsd");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_DirectoryWithGerarNfseXsd_Should_SelectAsStrongCandidate()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("GerarNfse_v2.02.xsd", "custom-other.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("GerarNfse_v2.02.xsd");

        CleanupTempDirectory(tempDir);
    }

    [Fact]
    public void Given_DirectoryWithEnviarLoteRpsXsd_Should_SelectAsStrongCandidate()
    {
        // Arrange
        var tempDir = CreateTempXsdDirectory("EnviarLoteRps_v2.02.xsd", "custom-other.xsd");

        // Act
        var selectionResult = _sut.Select(tempDir);

        // Assert
        selectionResult.IsSelected.ShouldBeTrue();
        Path.GetFileName(selectionResult.SelectedFile).ShouldBe("EnviarLoteRps_v2.02.xsd");

        CleanupTempDirectory(tempDir);
    }

    // ==========================================================
    // All real providers should select without error
    // ==========================================================

    [Theory]
    [InlineData("nacional")]
    [InlineData("abrasf")]
    [InlineData("gissonline")]
    [InlineData("issnet")]
    [InlineData("paulistana")]
    [InlineData("simpliss")]
    // webiss removed — was a test artifact, not a permanent provider
    public void Given_RealProvider_Should_SelectAnXsdFile(string providerName)
    {
        // Arrange
        var xsdDir = TestProviderPaths.FindXsdDir(providerName);

        // Act
        var selectionResult = _sut.Select(xsdDir);

        // Assert
        selectionResult.SelectedFile.ShouldNotBeNull(
            $"Provider '{providerName}' should have a selected XSD. Reason: {selectionResult.Reason}");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static string CreateTempXsdDirectory(params string[] fileNames)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        foreach (var fileName in fileNames)
        {
            var minimalXsd = """
                <?xml version="1.0" encoding="UTF-8"?>
                <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
                  <xs:element name="Root" type="xs:string"/>
                </xs:schema>
                """;
            File.WriteAllText(Path.Combine(tempDir, fileName), minimalXsd);
        }

        return tempDir;
    }

    private static void CleanupTempDirectory(string tempDir)
    {
        try { Directory.Delete(tempDir, true); }
        catch { /* Cleanup is best-effort */ }
    }
}
