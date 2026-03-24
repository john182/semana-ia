namespace SemanaIA.ServiceInvoice.Api.Swagger.Examples;

/// <summary>
/// Fabrica de exemplos de regras tipadas para documentacao Swagger.
/// Cada metodo retorna uma lista de regras representando um cenario real de configuracao.
/// </summary>
public static class RuleExamplesFactory
{
    /// <summary>
    /// Binding simples: vincula campo do dominio a campo do XML com formatacao de data.
    /// </summary>
    public static object BindingSimpleExample() => new[]
    {
        new
        {
            type = "Binding",
            target = "infDPS.dhEmi",
            source = "IssuedOn",
            format = "yyyy-MM-ddTHH:mm:sszzz"
        }
    };

    /// <summary>
    /// Binding com valor constante: o campo sempre recebe o mesmo valor fixo.
    /// </summary>
    public static object BindingConstantExample() => new[]
    {
        new
        {
            type = "Binding",
            target = "infDPS.tpEmit",
            sourceType = "constant",
            constantValue = "1"
        }
    };

    /// <summary>
    /// Binding com formatacao: extrai apenas digitos, aplica padding e complemento.
    /// </summary>
    public static object BindingFormattingExample() => new[]
    {
        new
        {
            type = "Binding",
            target = "infDPS.prest.CNPJ",
            source = "Provider.Cnpj",
            digitsOnly = true,
            padLeft = 14,
            padChar = "0"
        }
    };

    /// <summary>
    /// Default com fallback: usa valor do dominio quando disponivel, senao aplica valor padrao.
    /// </summary>
    public static object DefaultWithFallbackExample() => new[]
    {
        new
        {
            type = "Default",
            target = "infDPS.valores.trib.tribMun.tpRetISSQN",
            source = "RetentionType",
            fallbackValue = "1"
        }
    };

    /// <summary>
    /// Mapeamento de enum: traduz valores do dominio para codigos do provider.
    /// </summary>
    public static object EnumMappingExample() => new[]
    {
        new
        {
            type = "EnumMapping",
            target = "infDPS.valores.trib.tribMun.tribISSQN",
            source = "TaxationType",
            mappings = new Dictionary<string, string>
            {
                ["WithinCity"] = "1",
                ["OutsideCity"] = "1",
                ["Immune"] = "2",
                ["Export"] = "3",
                ["Free"] = "4"
            },
            defaultMapping = "1"
        }
    };

    /// <summary>
    /// Emissao condicional simples: emite campo somente quando a condicao e verdadeira.
    /// </summary>
    public static object ConditionalEmissionExample() => new[]
    {
        new
        {
            type = "ConditionalEmission",
            target = "infDPS.valores.trib.tribMun.tpImunidade",
            source = "ImmunityType",
            action = "Emit",
            condition = new
            {
                field = "TaxationType",
                @operator = "Equals",
                value = "Immune"
            }
        }
    };

    /// <summary>
    /// Emissao condicional composta (AND): emite campo quando multiplas condicoes sao atendidas.
    /// </summary>
    public static object ConditionalEmissionCompositeExample() => new[]
    {
        new
        {
            type = "ConditionalEmission",
            target = "infDPS.valores.trib.tribMun.Aliquota",
            source = "IssRate",
            action = "Emit",
            condition = new
            {
                logicalOperator = "And",
                conditions = new object[]
                {
                    new { field = "Provider.TaxRegime", @operator = "Equals", value = "SimplesNacional" },
                    new { field = "IssRate", @operator = "GreaterThan", value = "0" }
                }
            }
        }
    };

    /// <summary>
    /// Choice CPF/CNPJ: seleciona elemento XML baseado no tipo de pessoa (fisica ou juridica).
    /// </summary>
    public static object ChoiceCpfCnpjExample() => new[]
    {
        new
        {
            type = "Choice",
            target = "infDPS.prest",
            choiceField = "Provider.PersonType",
            options = new Dictionary<string, object>
            {
                ["LegalEntity"] = new
                {
                    element = "CNPJ",
                    source = "Provider.Cnpj",
                    padLeft = 14,
                    padChar = "0"
                },
                ["NaturalPerson"] = new
                {
                    element = "CPF",
                    source = "Provider.Cpf",
                    padLeft = 11,
                    padChar = "0"
                }
            }
        }
    };

    /// <summary>
    /// Conjunto completo: combina binding, enum mapping, condicional e choice em um unico array.
    /// Representa uma configuracao realista de provider com multiplas regras.
    /// </summary>
    public static object CompleteRuleSetExample() => new object[]
    {
        new
        {
            type = "Binding",
            target = "infDPS.dhEmi",
            source = "IssuedOn",
            format = "yyyy-MM-ddTHH:mm:sszzz"
        },
        new
        {
            type = "Binding",
            target = "infDPS.prest.CNPJ",
            source = "Provider.Cnpj",
            digitsOnly = true,
            padLeft = 14,
            padChar = "0"
        },
        new
        {
            type = "EnumMapping",
            target = "infDPS.valores.trib.tribMun.tribISSQN",
            source = "TaxationType",
            mappings = new Dictionary<string, string>
            {
                ["WithinCity"] = "1",
                ["OutsideCity"] = "1",
                ["Immune"] = "2",
                ["Export"] = "3",
                ["Free"] = "4"
            },
            defaultMapping = "1"
        },
        new
        {
            type = "ConditionalEmission",
            target = "infDPS.valores.trib.tribMun.tpImunidade",
            source = "ImmunityType",
            action = "Emit",
            condition = new
            {
                field = "TaxationType",
                @operator = "Equals",
                value = "Immune"
            }
        },
        new
        {
            type = "Choice",
            target = "infDPS.prest",
            choiceField = "Provider.PersonType",
            options = new Dictionary<string, object>
            {
                ["LegalEntity"] = new
                {
                    element = "CNPJ",
                    source = "Provider.Cnpj",
                    padLeft = 14,
                    padChar = "0"
                },
                ["NaturalPerson"] = new
                {
                    element = "CPF",
                    source = "Provider.Cpf",
                    padLeft = 11,
                    padChar = "0"
                }
            }
        }
    };
}
