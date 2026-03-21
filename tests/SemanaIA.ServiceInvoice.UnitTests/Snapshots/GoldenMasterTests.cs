using System.Xml.Linq;
using SemanaIA.ServiceInvoice.XmlGeneration.Manual;
using SemanaIA.ServiceInvoice.UnitTests.Manual;
using Shouldly;

namespace SemanaIA.ServiceInvoice.UnitTests.Snapshots;

public class GoldenMasterTests
{
    private readonly NationalDpsManualSerializer _sut = new();

    [Fact]
    public void Given_MinimalFixture_Should_ProduceConsistentXml()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateValidMinimal();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var xdoc = XDocument.Parse(result.Xml);
        var snapshotPath = GetSnapshotPath("minimal-dps.xml");

        if (!File.Exists(snapshotPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
            File.WriteAllText(snapshotPath, xdoc.ToString());
        }

        var expected = XDocument.Load(snapshotPath);
        XNode.DeepEquals(xdoc, expected).ShouldBeTrue("Generated XML diverges from golden master minimal-dps.xml");
    }

    [Fact]
    public void Given_CompleteFixture_Should_ProduceConsistentXml()
    {
        // Arrange
        var document = DpsDocumentTestFixture.CreateComplete();

        // Act
        var result = _sut.Serialize(document);

        // Assert
        result.Xml.ShouldBeValidAgainstDpsSchema();

        var xdoc = XDocument.Parse(result.Xml);
        var snapshotPath = GetSnapshotPath("complete-dps.xml");

        if (!File.Exists(snapshotPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
            File.WriteAllText(snapshotPath, xdoc.ToString());
        }

        var expected = XDocument.Load(snapshotPath);
        XNode.DeepEquals(xdoc, expected).ShouldBeTrue("Generated XML diverges from golden master complete-dps.xml");
    }

    // ==========================================================
    // Helpers privados (final da classe)
    // ==========================================================

    private static string GetSnapshotPath(string fileName)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "tests", "SemanaIA.ServiceInvoice.UnitTests", "Snapshots", fileName);
            if (Directory.Exists(Path.GetDirectoryName(candidate)!))
                return candidate;

            var altCandidate = Path.Combine(dir, "Snapshots", fileName);
            if (Directory.Exists(Path.GetDirectoryName(altCandidate)!))
                return altCandidate;

            dir = Directory.GetParent(dir)?.FullName;
        }

        return Path.Combine(AppContext.BaseDirectory, "Snapshots", fileName);
    }
}
