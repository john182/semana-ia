namespace SemanaIA.ServiceInvoice.UnitTests.Providers.Shared;

internal static class TestProviderPaths
{
    public static string FindProvidersDir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers");
            if (Directory.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new DirectoryNotFoundException("providers/ not found");
    }

    public static string FindXsdPath(string provider, string fileName)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", provider, "xsd", fileName);
            if (File.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"XSD not found: {provider}/{fileName}");
    }

    public static string FindXsdDir(string provider)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "providers", provider, "xsd");
            if (Directory.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new DirectoryNotFoundException($"XSD dir not found: {provider}");
    }

    public static string FindRulesPath(string provider)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var typedCandidate = Path.Combine(dir, "providers", provider, "rules", "rules.json");
            if (File.Exists(typedCandidate)) return typedCandidate;

            var legacyCandidate = Path.Combine(dir, "providers", provider, "rules", "base-rules.json");
            if (File.Exists(legacyCandidate)) return legacyCandidate;

            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new FileNotFoundException($"Rules not found: {provider}");
    }
}
