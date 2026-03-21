using System.Text;
using System.Text.RegularExpressions;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.UnitTests.SchemaEngine;

public enum DivergenceType
{
    Equivalent,
    MissingInManual,
    MissingInGenerated,
    ExternalRuleGap,
    AcceptableByDesign,
    SchemaManualDivergence
}

public record ComparisonResult(
    string ComplexType,
    string ElementName,
    bool IsRequired,
    DivergenceType Divergence,
    string? Notes = null);

public class BaselineComparisonAnalyzer
{
    private static readonly Dictionary<string, string> ComplexTypeToBuildMethod = new()
    {
        ["TCDPS"] = "Serialize",
        ["TCInfDPS"] = "BuildInfDps",
        ["TCInfoPrestador"] = "BuildProvider",
        ["TCRegTrib"] = "BuildRegTrib",
        ["TCInfoPessoa"] = "BuildPerson",
        ["TCEndereco"] = "BuildEndereco",
        ["TCEnderNac"] = "BuildEndereco",
        ["TCEnderExt"] = "BuildEndereco",
        ["TCServ"] = "BuildServico",
        ["TCLocPrest"] = "BuildLocPrest",
        ["TCCServ"] = "BuildCServ",
        ["TCComExterior"] = "BuildComExt",
        ["TCLocacaoSublocacao"] = "BuildLsadppu",
        ["TCInfoObra"] = "BuildObra",
        ["TCAtvEvento"] = "BuildAtvEvento",
        ["TCInfoCompl"] = "BuildInfoCompl",
        ["TCInfoValores"] = "BuildValores",
        ["TCVServPrest"] = "BuildValores",
        ["TCVDescCondIncond"] = "BuildValores",
        ["TCInfoDedRed"] = "BuildValores",
        ["TCInfoTributacao"] = "BuildValores",
        ["TCTribMunicipal"] = "BuildTribMun",
        ["TCTribFederal"] = "BuildTribFed",
        ["TCTribOutrosPisCofins"] = "BuildTribFed",
        ["TCTribTotal"] = "BuildTotTrib",
        ["TCTribTotalMonet"] = "BuildTotTrib",
        ["TCTribTotalPercent"] = "BuildTotTrib",
        ["TCExigSuspensa"] = "BuildTribMun",
        ["TCBeneficioMunicipal"] = "BuildTribMun",
        ["TCSubstituicao"] = null!,
        ["TCExploracaoRodoviaria"] = null!,
        ["TCRTCInfoIBSCBS"] = "BuildIbsCbs",
        ["TCEnderecoSimples"] = "BuildEnderecoSimples",
        ["TCEnderObraEvento"] = "BuildEnderecoSimples",
        ["TCEnderExtSimples"] = "BuildEnderecoSimples",
        ["TCDocDedRed"] = "BuildDeductionDocuments",
        ["TCDocOutNFSe"] = "BuildDeductionDocuments",
        ["TCDocNFNFS"] = "BuildDeductionDocuments",
        ["TCListaDocDedRed"] = "BuildDeductionDocuments",
        ["TCInfoItemPed"] = "BuildInfoCompl"
    };

    public List<ComparisonResult> Compare(SchemaDocument schema, string manualSource)
    {
        var results = new List<ComparisonResult>();

        foreach (var ct in schema.ComplexTypes)
        {
            var buildMethod = FindBuildMethod(ct.Name);

            if (buildMethod is null)
            {
                foreach (var el in ct.Elements)
                {
                    results.Add(new ComparisonResult(
                        ct.Name, el.Name, el.IsRequired,
                        DivergenceType.MissingInManual,
                        $"ComplexType {ct.Name} has no corresponding Build* method"));
                }
                continue;
            }

            foreach (var el in ct.Elements)
            {
                var isEmitted = IsElementEmitted(manualSource, el.Name);
                var divergence = ClassifyDivergence(el, isEmitted, ct.Name, manualSource);
                results.Add(new ComparisonResult(ct.Name, el.Name, el.IsRequired, divergence));
            }
        }

        return results;
    }

    public string GenerateDetailedReport(List<ComparisonResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Detailed Comparison: Generated vs. Manual Baseline");
        sb.AppendLine();

        var total = results.Count;
        var equivalent = results.Count(r => r.Divergence == DivergenceType.Equivalent);
        var missingManual = results.Count(r => r.Divergence == DivergenceType.MissingInManual);
        var ruleGap = results.Count(r => r.Divergence == DivergenceType.ExternalRuleGap);
        var acceptable = results.Count(r => r.Divergence == DivergenceType.AcceptableByDesign);

        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"| Metric | Count | % |");
        sb.AppendLine($"|--------|-------|---|");
        sb.AppendLine($"| Total elements | {total} | 100% |");
        sb.AppendLine($"| Equivalent | {equivalent} | {Pct(equivalent, total)} |");
        sb.AppendLine($"| Missing in manual | {missingManual} | {Pct(missingManual, total)} |");
        sb.AppendLine($"| External rule gap | {ruleGap} | {Pct(ruleGap, total)} |");
        sb.AppendLine($"| Acceptable by design | {acceptable} | {Pct(acceptable, total)} |");
        sb.AppendLine();

        sb.AppendLine("## Equivalence Criteria");
        sb.AppendLine();
        sb.AppendLine("The generated artifacts are considered functionally equivalent to the manual baseline when:");
        sb.AppendLine("1. All required elements of TCInfDPS are present in both");
        sb.AppendLine("2. Choice groups are represented (CNPJ/CPF/NIF/cNaoNIF, endNac/endExt)");
        sb.AppendLine("3. Formatting rules from base-rules.json are applied");
        sb.AppendLine("4. Conditional emission rules are documented or implemented");
        sb.AppendLine("5. XML output validates against the same XSD");
        sb.AppendLine();

        sb.AppendLine("## Detail by ComplexType");
        sb.AppendLine();
        sb.AppendLine("| ComplexType | Element | Required | Divergence | Notes |");
        sb.AppendLine("|------------|---------|----------|------------|-------|");

        foreach (var r in results)
        {
            sb.AppendLine($"| {r.ComplexType} | {r.ElementName} | {(r.IsRequired ? "yes" : "no")} | {r.Divergence} | {r.Notes ?? ""} |");
        }

        return sb.ToString();
    }

    public string GenerateEvolutionBacklog(List<ComparisonResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Generation Evolution Backlog");
        sb.AppendLine();
        sb.AppendLine("Gaps to close for generation to reach manual baseline equivalence.");
        sb.AppendLine();

        var gaps = results.Where(r => r.Divergence is DivergenceType.MissingInManual or DivergenceType.ExternalRuleGap).ToList();
        var requiredGaps = gaps.Where(r => r.IsRequired).ToList();
        var optionalGaps = gaps.Where(r => !r.IsRequired).ToList();

        sb.AppendLine("## High Priority (required elements)");
        sb.AppendLine();
        sb.AppendLine("| ComplexType | Element | Divergence | Notes |");
        sb.AppendLine("|------------|---------|------------|-------|");
        foreach (var r in requiredGaps)
            sb.AppendLine($"| {r.ComplexType} | {r.ElementName} | {r.Divergence} | {r.Notes ?? ""} |");

        sb.AppendLine();
        sb.AppendLine("## Medium Priority (optional elements)");
        sb.AppendLine();
        sb.AppendLine("| ComplexType | Element | Divergence | Notes |");
        sb.AppendLine("|------------|---------|------------|-------|");
        foreach (var r in optionalGaps)
            sb.AppendLine($"| {r.ComplexType} | {r.ElementName} | {r.Divergence} | {r.Notes ?? ""} |");

        sb.AppendLine();
        sb.AppendLine($"**Total gaps:** {gaps.Count} ({requiredGaps.Count} required, {optionalGaps.Count} optional)");

        return sb.ToString();
    }

    // --- Private methods ---

    private static string? FindBuildMethod(string complexTypeName)
    {
        return ComplexTypeToBuildMethod.TryGetValue(complexTypeName, out var method) ? method : null;
    }

    private static bool IsElementEmitted(string source, string elementName)
    {
        var pattern = $@"xml\.{Regex.Escape(elementName)}\(|\.{Regex.Escape(elementName)}\(";
        return Regex.IsMatch(source, pattern);
    }

    private static DivergenceType ClassifyDivergence(SchemaElement el, bool isEmitted, string complexType, string source)
    {
        if (isEmitted) return DivergenceType.Equivalent;

        // Known acceptable omissions
        if (el.Name is "Signature") return DivergenceType.AcceptableByDesign;

        // ComplexTypes not in scope of manual
        var notInManualScope = new HashSet<string>
        {
            "TCNFSe", "TCInfNFSe", "TCEmitente", "TCValoresNFSe",
            "TCRTCIBSCBS", "TCRTCValoresIBSCBS", "TCRTCValoresIBSCBSUF",
            "TCRTCValoresIBSCBSMun", "TCRTCValoresIBSCBSFed",
            "TCRTCTotalCIBS", "TCRTCTotalIBS", "TCRTCTotalIBSCredPres",
            "TCRTCTotalIBSUF", "TCRTCTotalIBSMun", "TCRTCTotalCBS",
            "TCRTCTotalCBSCredPres", "TCRTCTotalTribRegular",
            "TCRTCTotalTribCompraGov"
        };

        if (notInManualScope.Contains(complexType))
            return DivergenceType.AcceptableByDesign;

        if (!el.IsRequired)
            return DivergenceType.MissingInManual;

        return DivergenceType.MissingInManual;
    }

    private static string Pct(int count, int total) =>
        total == 0 ? "0%" : $"{count * 100 / total}%";
}
