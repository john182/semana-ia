namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

/// <summary>
/// Infers ConditionalEmission rules from XSD schema structure.
/// Detects CPF/CNPJ choice groups and optional elements with known mappings,
/// generating the same rule format as hand-crafted rules.json files.
/// </summary>
public static class ConditionalEmissionInferrer
{
    private const string BorrowerFederalTaxNumberField = "Borrower.FederalTaxNumber";
    private const string ProviderFederalTaxNumberField = "Provider.FederalTaxNumber";
    private const string CpfUpperBound = "99999999999"; // 11-digit max (CPF range ceiling / CNPJ threshold)
    private const int CnpjPadLength = 14;
    private const int CpfPadLength = 11;
    private const string DefaultPadChar = "0";

    private static readonly HashSet<string> CpfElementNames = new(StringComparer.OrdinalIgnoreCase) { "Cpf" };
    private static readonly HashSet<string> CnpjElementNames = new(StringComparer.OrdinalIgnoreCase) { "Cnpj" };

    // Contexts that map to the provider (prest, Prestador, PrestadorServico, etc.)
    private static readonly HashSet<string> ProviderContextPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "prest", "Prestador", "PrestadorServico"
    };

    /// <summary>
    /// Groups elements of a complex type by their ChoiceGroup identifier.
    /// Returns only groups that contain at least 2 elements (actual choices).
    /// </summary>
    public static Dictionary<string, List<SchemaElement>> GroupChoiceElements(SchemaComplexType complexType)
    {
        var choiceGroups = new Dictionary<string, List<SchemaElement>>();

        foreach (var element in complexType.Elements)
        {
            if (!element.IsChoice || element.ChoiceGroup is null)
                continue;

            if (!choiceGroups.TryGetValue(element.ChoiceGroup, out var groupElements))
            {
                groupElements = [];
                choiceGroups[element.ChoiceGroup] = groupElements;
            }

            groupElements.Add(element);
        }

        // Only return groups with 2+ members (a real choice)
        return choiceGroups
            .Where(group => group.Value.Count >= 2)
            .ToDictionary(group => group.Key, group => group.Value);
    }

    /// <summary>
    /// Detects whether a choice group contains a CPF/CNPJ pattern.
    /// Returns true when the group contains at least one CPF and one CNPJ element,
    /// even if additional options (e.g., NIF, cNaoNIF) are present.
    /// </summary>
    public static bool IsCpfCnpjChoiceGroup(List<SchemaElement> choiceElements)
    {
        var hasCpf = choiceElements.Any(element => CpfElementNames.Contains(element.Name));
        var hasCnpj = choiceElements.Any(element => CnpjElementNames.Contains(element.Name));

        return hasCpf && hasCnpj;
    }

    /// <summary>
    /// Generates a pair of ConditionalEmission rules for the CPF/CNPJ elements in a choice group.
    /// The CNPJ rule emits when FederalTaxNumber > 99999999999 (14-digit range).
    /// The CPF rule emits when FederalTaxNumber > 0 AND <= 99999999999 (11-digit range).
    /// The source field is determined by the element path context (borrower vs provider).
    /// Returns the rules and the names of the handled elements (only CPF/CNPJ, not other options like NIF).
    /// </summary>
    public static (List<ProviderRule> Rules, HashSet<string> HandledElementNames) InferCpfCnpjChoiceRules(
        List<SchemaElement> choiceElements, string parentPath)
    {
        var rules = new List<ProviderRule>();
        var handledNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var sourceField = ResolveFederalTaxNumberSource(parentPath);

        var cnpjElement = choiceElements.First(element => CnpjElementNames.Contains(element.Name));
        var cpfElement = choiceElements.First(element => CpfElementNames.Contains(element.Name));

        handledNames.Add(cnpjElement.Name);
        handledNames.Add(cpfElement.Name);

        var cnpjTargetPath = string.IsNullOrEmpty(parentPath) ? cnpjElement.Name : $"{parentPath}.{cnpjElement.Name}";
        var cpfTargetPath = string.IsNullOrEmpty(parentPath) ? cpfElement.Name : $"{parentPath}.{cpfElement.Name}";

        // CNPJ rule: emit when FederalTaxNumber > 99999999999
        rules.Add(new ProviderRule
        {
            Type = RuleType.ConditionalEmission,
            Target = cnpjTargetPath,
            Source = sourceField,
            Action = RuleAction.Emit,
            Condition = new RuleCondition
            {
                Field = sourceField,
                Operator = ComparisonOperator.GreaterThan,
                Value = CpfUpperBound
            },
            PadLeft = CnpjPadLength,
            PadChar = DefaultPadChar
        });

        // CPF rule: emit when FederalTaxNumber > 0 AND <= 99999999999
        rules.Add(new ProviderRule
        {
            Type = RuleType.ConditionalEmission,
            Target = cpfTargetPath,
            Source = sourceField,
            Action = RuleAction.Emit,
            Condition = new RuleCondition
            {
                LogicalOperator = LogicalOperator.And,
                Conditions =
                [
                    new RuleCondition
                    {
                        Field = sourceField,
                        Operator = ComparisonOperator.GreaterThan,
                        Value = "0"
                    },
                    new RuleCondition
                    {
                        Field = sourceField,
                        Operator = ComparisonOperator.LessThanOrEqual,
                        Value = CpfUpperBound
                    }
                ]
            },
            PadLeft = CpfPadLength,
            PadChar = DefaultPadChar
        });

        return (rules, handledNames);
    }

    /// <summary>
    /// Generates a ConditionalEmission rule with HasValue condition for an optional element
    /// (MinOccurs == 0) that has a known mapping in the CommonFieldMappingDictionary.
    /// Returns null if the element is not optional or has no known mapping.
    /// </summary>
    public static ProviderRule? InferHasValueRule(
        SchemaElement element, string elementPath, string? contextualSource = null)
    {
        if (element.MinOccurs != 0)
            return null;

        var sourceField = contextualSource;

        if (sourceField is null)
        {
            if (!CommonFieldMappingDictionary.Mappings.TryGetValue(element.Name, out var mappedSource))
                return null;

            // Constant sources and pipe-expression sources should not get HasValue rules
            if (mappedSource.StartsWith("const:") || mappedSource.Contains('|'))
                return null;

            sourceField = mappedSource;
        }
        else
        {
            // Empty contextual source means "skip" -- no HasValue rule should be generated
            if (string.IsNullOrEmpty(sourceField))
                return null;

            // Extract the base source from pipe expressions (e.g., "Borrower.Email" from "Borrower.Email | padLeft:11:0")
            if (sourceField.Contains('|'))
                sourceField = sourceField.Split('|')[0].Trim();

            // Constant sources should not get HasValue rules
            if (sourceField.StartsWith("const:"))
                return null;
        }

        return new ProviderRule
        {
            Type = RuleType.ConditionalEmission,
            Target = elementPath,
            Source = sourceField,
            Action = RuleAction.Emit,
            Condition = new RuleCondition
            {
                Field = sourceField,
                Operator = ComparisonOperator.HasValue
            }
        };
    }

    // --- Private methods ---

    private static string ResolveFederalTaxNumberSource(string parentPath)
    {
        if (IsInContext(parentPath, ProviderContextPrefixes))
            return ProviderFederalTaxNumberField;

        // Default to borrower context (toma and any other consumer context)
        return BorrowerFederalTaxNumberField;
    }

    private static bool IsInContext(string path, HashSet<string> contextPrefixes)
    {
        foreach (var prefix in contextPrefixes)
        {
            // Match context segment anywhere in the path:
            // - ".toma." in the middle (e.g., "infDPS.toma.end.cMun")
            // - "toma." at the start (e.g., "toma.CNPJ")
            // - ".toma" at the end (e.g., "infDPS.toma")
            // - exact match (e.g., "toma")
            if (path.Contains($".{prefix}.", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith($"{prefix}.", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith($".{prefix}", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
