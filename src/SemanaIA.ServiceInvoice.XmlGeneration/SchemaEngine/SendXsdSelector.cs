namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class SendXsdSelector
{
    private const int PriorityExactSend = 1;
    private const int PriorityStrongCandidate = 2;
    private const int PriorityFallbackCandidate = 3;

    private static readonly (string Pattern, int Priority)[] SendPatterns =
    [
        ("enviar", PriorityExactSend),
        ("servico_enviar", PriorityExactSend),
        ("envio_lote", PriorityExactSend),
        ("DPS_", PriorityStrongCandidate),
        ("Pedido", PriorityStrongCandidate),
        ("schema_nfse", PriorityFallbackCandidate),
        ("nfse_", PriorityFallbackCandidate),
    ];

    private static readonly string[] ExcludePatterns =
    [
        ProviderProfile.XmlDsigFilePattern,
        "resposta", "retorno", "consulta", "cancelar", "cancela",
        "situacao", "cabecalho", "substituir", "gerar-nfse", "baixa",
        "tipos_", "tipos-", "tipos_v", "tiposComplexos", "tiposSimples", "tiposEventos", "tiposCnc",
        "TiposNFe",
        "evento_", "pedRegEvento",
        "CNC_v",
        "consulta_", "servico_cancelar", "servico_consultar",
        "_core",
    ];

    public XsdSelectionResult Select(string xsdDirectory, ProviderProfile? profile = null)
    {
        if (profile?.PrimaryXsdFile is not null)
        {
            var overridePath = Path.Combine(xsdDirectory, profile.PrimaryXsdFile);
            return File.Exists(overridePath)
                ? new XsdSelectionResult(overridePath, IsAmbiguous: false, Candidates: [], Reason: "Selected via PrimaryXsdFile override")
                : new XsdSelectionResult(null, IsAmbiguous: true, Candidates: [],
                    Reason: $"PrimaryXsdFile '{profile.PrimaryXsdFile}' not found in {xsdDirectory}");
        }

        var allXsdFiles = Directory.GetFiles(xsdDirectory, ProviderProfile.XsdSearchPattern);
        if (allXsdFiles.Length == 0)
            return new XsdSelectionResult(null, IsAmbiguous: false, Candidates: [], Reason: "No XSD files found");

        var candidateFiles = FilterExcludedFiles(allXsdFiles);

        if (candidateFiles.Count == 0)
            return new XsdSelectionResult(null, IsAmbiguous: false,
                Candidates: allXsdFiles.Select(f => Path.GetFileName(f)).ToList(),
                Reason: "No suitable send XSD found; all files matched exclusion patterns");

        if (candidateFiles.Count == 1)
            return new XsdSelectionResult(candidateFiles[0], IsAmbiguous: false, Candidates: [],
                Reason: "Single non-excluded XSD file in directory");

        var matchedByPattern = MatchByPatternPriority(candidateFiles);

        if (matchedByPattern.Count == 1)
            return new XsdSelectionResult(matchedByPattern[0], IsAmbiguous: false, Candidates: [],
                Reason: "Matched by send pattern heuristic");

        if (matchedByPattern.Count > 1)
            return new XsdSelectionResult(matchedByPattern[0], IsAmbiguous: true,
                Candidates: matchedByPattern.Select(f => Path.GetFileName(f)).ToList(),
                Reason: "Multiple XSD files matched send patterns");

        return new XsdSelectionResult(candidateFiles[0], IsAmbiguous: true,
            Candidates: candidateFiles.Select(f => Path.GetFileName(f)).ToList(),
            Reason: "No XSD matched send patterns; falling back to first non-excluded file");
    }

    // --- Private methods ---

    private static List<string> FilterExcludedFiles(string[] xsdFiles)
    {
        return xsdFiles
            .Where(filePath => !IsExcluded(Path.GetFileName(filePath)))
            .ToList();
    }

    private static bool IsExcluded(string fileName)
    {
        return ExcludePatterns.Any(excludePattern =>
            fileName.Contains(excludePattern, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> MatchByPatternPriority(List<string> candidateFiles)
    {
        var bestPriority = int.MaxValue;
        var bestMatches = new List<string>();

        foreach (var filePath in candidateFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var matchedPriority = FindMatchingPriority(fileName);

            if (matchedPriority is null)
                continue;

            if (matchedPriority.Value < bestPriority)
            {
                bestPriority = matchedPriority.Value;
                bestMatches = [filePath];
            }
            else if (matchedPriority.Value == bestPriority)
            {
                bestMatches.Add(filePath);
            }
        }

        return bestMatches;
    }

    private static int? FindMatchingPriority(string fileName)
    {
        foreach (var (pattern, priority) in SendPatterns)
        {
            if (fileName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return priority;
        }

        return null;
    }
}

public record XsdSelectionResult(
    string? SelectedFile,
    bool IsAmbiguous,
    IReadOnlyList<string> Candidates,
    string? Reason = null)
{
    public bool IsSelected => SelectedFile is not null && !IsAmbiguous;
}
