## Context

O projeto possui 7 providers de DPS (nacional, abrasf, gissonline, issnet, paulistana, simpliss, webiss). Os testes estão organizados em:

- `UnitTests/Manual/` — testes do serializer manual Nacional + builders/helpers
- `UnitTests/Manual/Nacional/` — testes específicos de variações do Nacional (taxation, retention, optional blocks, deduction)
- `UnitTests/SchemaEngine/` — testes da engine de serialização baseada em schema + testes de providers via engine (ISSNet, GISSOnline, Webiss, Simpliss, Paulistana, Abrasf)
- `IntegrationsTests/` — testes E2E monolíticos (ProviderEndToEndFlowTests, ManualVsEngineApiComparisonTests)

**Problemas atuais:**
1. `SchemaEngine/` mistura testes da engine (XsdSchemaAnalyzer, SchemaBasedXmlSerializer, RuleResolver, etc.) com testes de providers (IssnetXmlSerializationTests, WebissXmlSerializationTests, etc.)
2. `Manual/` contém testes do serializer manual Nacional mas os helpers (DpsDocumentBuilder, XsdValidationHelper) são compartilhados
3. Testes de integração não separam fluxos de engine dos fluxos de providers
4. Providers abrasf, paulistana e simpliss têm cobertura parcial via engine, mas não exercitam todas as `FillingVariations`
5. Não existe padrão uniforme de nomenclatura entre os testes de diferentes providers

**Infraestrutura existente que será reutilizada:**
- `DpsDocumentBuilder` (30+ métodos fluent, 450 linhas)
- `DpsDocumentTestFixture.FillingVariations()` (25 cenários parametrizados)
- `XsdValidationHelper.ShouldBeValidAgainstDpsSchema()` e `ShouldBeValidAgainstProviderSchema()`
- `NacionalXmlParseHelpers` (parse de seções do XML)

## Goals / Non-Goals

**Goals:**
- Separar claramente testes de engine e testes de providers em ambos os projetos (unit e integration)
- Garantir que todos os 7 providers tenham testes de geração de DPS com validação de schema
- Exercitar `FillingVariations` por provider para cobrir múltiplas formas válidas de preenchimento
- Padronizar nomenclatura de classes, métodos e organização de pastas
- Manter helpers/builders existentes acessíveis a todos os testes

**Non-Goals:**
- Alterar código de produção (apenas testes)
- Criar framework de testes genérico ou abstração excessiva
- Atingir 100% de cobertura de linhas — foco em cobertura de cenários reais
- Reescrever testes que já funcionam e estão no local correto
- Testar providers que não suportam DPS

## Decisions

### 1. Estrutura de pastas para testes unitários

**Decisão:** Reorganizar `UnitTests/` com separação explícita:

```
UnitTests/
├── Engine/                              # Testes da engine de serialização
│   ├── SchemaAnalyzer/                  # XsdSchemaAnalyzer*, SchemaCodeGenerator
│   ├── Serializer/                      # SchemaBasedXmlSerializer*, Pipeline
│   ├── Rules/                           # RuleResolver, RuleCondition, TypedRule
│   ├── ProviderConfig/                  # ProviderConfigGenerator, ProviderResolver
│   └── Diagnostics/                     # ValidationDiagnosticEnricher, Baseline
├── Providers/                           # Testes de geração DPS por provider
│   ├── _Shared/                         # Builders, Fixtures, Helpers
│   │   ├── DpsDocumentBuilder.cs
│   │   ├── DpsDocumentTestFixture.cs
│   │   ├── XsdValidationHelper.cs
│   │   └── XmlParseHelpers.cs
│   ├── Nacional/                        # Provider nacional (manual serializer)
│   │   ├── NacionalDpsSerializationTests.cs
│   │   ├── NacionalTaxationTypeTests.cs
│   │   ├── NacionalRetentionTypeTests.cs
│   │   ├── NacionalOptionalBlocksTests.cs
│   │   └── NacionalDeductionTests.cs
│   ├── Issnet/
│   │   └── IssnetDpsSerializationTests.cs
│   ├── Gissonline/
│   │   └── GissonlineDpsSerializationTests.cs
│   ├── Abrasf/
│   │   └── AbrasfDpsSerializationTests.cs
│   ├── Webiss/
│   │   └── WebissDpsSerializationTests.cs
│   ├── Simpliss/
│   │   └── SimplissDpsSerializationTests.cs
│   └── Paulistana/
│       └── PaulistanaDpsSerializationTests.cs
├── Mappers/                             # Mappers (já existente)
├── Snapshots/                           # Golden master (já existente)
└── data/                                # Dados de teste (já existente)
```

**Alternativa descartada:** Manter `SchemaEngine/` como está e apenas adicionar pastas de providers. Descartada porque perpetua a mistura de responsabilidades dentro de `SchemaEngine/`.

**Alternativa descartada:** Criar projeto de teste separado para providers. Overengineering — a separação por pastas é suficiente.

### 2. Estrutura de pastas para testes de integração

**Decisão:** Separar testes E2E:

```
IntegrationsTests/
├── Engine/
│   └── ManualVsEngineComparisonTests.cs
├── Providers/
│   ├── NacionalEndToEndTests.cs
│   ├── IssnetEndToEndTests.cs
│   ├── GissonlineEndToEndTests.cs
│   └── ...
└── Infrastructure/
    └── ProviderFullLoadTests.cs
```

### 3. Estratégia de cobertura por provider

**Decisão:** Cada provider MUST ter:
- Um teste parametrizado `[Theory]` com `[MemberData(nameof(DpsDocumentTestFixture.FillingVariations))]` validando schema
- Testes `[Fact]` adicionais para cenários provider-specific (envelope, atributos, bindings)
- Validação de conteúdo XML (não apenas schema) para campos críticos

**Racional:** `FillingVariations` já cobre 25 cenários de preenchimento. Reusá-lo para cada provider garante cobertura ampla sem duplicação de setup.

### 4. Localização dos helpers compartilhados

**Decisão:** Mover builders/helpers para `Providers/_Shared/` dentro do projeto de testes unitários. Os testes de integração referenciam o projeto de testes unitários (project reference) para acessar os mesmos builders.

**Alternativa descartada:** Criar projeto `TestSupport` separado. Complexidade desnecessária — project reference resolve.

### 5. Uso do XSD como fonte de verdade

**Decisão:** O XSD é a referência para identificar:
- Campos obrigatórios vs opcionais
- Estruturas alternativas (choice elements)
- Tipos enumerados e restrições
- Validação de valores de atributos

Os cenários de teste devem ser derivados do XSD, não de suposições. O `DpsDocumentTestFixture.FillingVariations()` já mapeia as variações principais; novos cenários devem ser adicionados apenas quando o XSD indicar caminhos não cobertos.

### 6. Nomenclatura padronizada

**Decisão:**
- **Arquivo:** `<Provider>Dps<Aspecto>Tests.cs` (ex: `IssnetDpsSerializationTests.cs`, `NacionalTaxationTypeTests.cs`)
- **Classe:** Mesmo nome do arquivo
- **Método:** `Given_<contexto>_Should_<comportamento>` (já em uso, manter)
- **Namespace:** `SemanaIA.ServiceInvoice.UnitTests.Providers.<ProviderName>` e `SemanaIA.ServiceInvoice.UnitTests.Engine.<SubArea>`

## Risks / Trade-offs

**[Risco] Mover arquivos pode quebrar referências e namespaces**
→ Mitigação: Executar `dotnet build` e `dotnet test` após cada etapa de reorganização. Ajustar namespaces incrementalmente.

**[Risco] Testes parametrizados de FillingVariations podem falhar para providers que não suportam todas as variações**
→ Mitigação: Criar `ProviderFillingVariations()` específico por provider quando necessário, derivando de `FillingVariations` e excluindo cenários incompatíveis. Ou marcar cenários com `[Skip]` documentando o motivo.

**[Risco] Falsa sensação de cobertura — testes que apenas validam schema sem checar comportamento**
→ Mitigação: Todo teste parametrizado de schema MUST ser complementado por ao menos 3-5 testes `[Fact]` que validem conteúdo XML específico do provider (elementos, atributos, valores).

**[Risco] Duplicação entre testes unit e integration para o mesmo provider**
→ Mitigação: Testes unit validam serialização isolada. Testes integration validam fluxo HTTP completo. Responsabilidades diferentes, cenários podem parecer similares mas testam camadas distintas.

**[Trade-off] Reorganização gera diff grande em uma única PR**
→ Mitigação: Executar em etapas incrementais (ver plano no tasks.md). Primeiro mover, depois criar novos testes.
