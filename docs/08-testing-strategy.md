# Estrategia de Testes

## Visao Geral

O projeto possui **727 testes** distribuidos em duas suites:
- **611 testes unitarios** — cobrem a engine de schema, serializer, auto-geracao, regras tipadas, diagnosticos e modelos de dominio.
- **116 testes de integracao** — cobrem endpoints da API, persistencia MongoDB e fluxos E2E completos.

---

## Piramide de Testes

```
                    ┌──────────────┐
                    │   Manual     │  Swagger UI / curl
                    │   (ad hoc)   │
                    ├──────────────┤
                ┌───┤  Integracao  ├───┐  116 testes
                │   │  (API + DB)  │   │  WebApplicationFactory + MongoDB
                │   ├──────────────┤   │
            ┌───┤   │   Unitarios  │   ├───┐  611 testes
            │   │   │  (engine +   │   │   │  xUnit + Shouldly
            │   │   │   dominio)   │   │   │
            └───┴───┴──────────────┴───┴───┘
```

### Testes Unitarios (611)

Cobrem componentes isolados sem dependencias externas (sem banco, sem rede, sem filesystem externo).

**Principais areas cobertas:**
- Analise de schema XSD (`XsdSchemaAnalyzer`)
- Serializacao XML baseada em schema (`SchemaBasedXmlSerializer`)
- Auto-geracao de configuracao (`ProviderConfigGenerator`)
- Resolucao de regras tipadas (`TypedRuleResolver`)
- Validacao de regras (`ProviderRuleValidator`)
- Diagnosticos de validacao (`ValidationDiagnosticEnricher`)
- Data binding (`ServiceInvoiceSchemaDataBinder`)
- Pipeline de serializacao (`SchemaSerializationPipeline`)
- Selecao de XSD de envio (`SendXsdSelector`)
- Condicoes de regra (`RuleCondition`)
- Modelo de dominio (`ManagedProvider`)
- Servico de gestao (`ProviderManagementService`)
- Mappers de request (`NfseRequestToDpsDocumentModelMapper`)
- Serializer manual baseline (`NationalDpsManualSerializer`, `IbsCbsManualBuilder`)
- Golden master / snapshot tests

### Testes de Integracao (116)

Cobrem fluxos completos com HTTP real (via `WebApplicationFactory`) e MongoDB (service container).

**Principais areas cobertas:**
- Endpoints de NFS-e (`NfseEndpointIntegrationTests`)
- Endpoints de gestao de providers (`ProviderManagementEndpointTests`)
- Fluxo E2E para 48 providers (`ProviderEndToEndFlowTests`)
- Lifecycle completo de 7 providers (`ProviderFullLoadTests`)
- Onboarding em massa (`ProviderOnboardingLoadTests`)

### Testes Manuais

Via Swagger UI (`/swagger`) ou curl direto nos endpoints da API. Uteis para validacao visual do XML gerado e testes exploratórios.

---

## Classes de Teste Principais

### AllProvidersXsdValidationSummaryTests

Valida XML gerado contra o schema XSD para todos os providers do diretorio `providers/`. Garante que o XML produzido pela engine e valido segundo o schema de cada provider.

**Localizacao:** `tests/SemanaIA.ServiceInvoice.UnitTests/SchemaEngine/`

### ProviderEndToEndFlowTests

Testa o fluxo completo via API para 48 providers (test data providers). Para cada provider:
1. Cria via `POST /api/v1/providers` com os XSDs
2. Verifica status
3. Gera XML via `POST /api/v1/nfse/xml`
4. Valida que o XML foi gerado

**Requer MongoDB** — roda no CI com service container.

**Localizacao:** `tests/SemanaIA.ServiceInvoice.IntegrationsTests/ProviderEndToEndFlowTests.cs`

### ProviderManagementEndpointTests

Testa todos os endpoints da API de gestao de providers:
- CRUD completo (Create, Read, Update, Delete)
- Gestao de municipios (Add, Remove)
- Gestao de regras (Get, Replace, Add, Remove)
- Validacao sob demanda
- Transicoes de status (Activate, Deactivate)

**Localizacao:** `tests/SemanaIA.ServiceInvoice.IntegrationsTests/ProviderManagementEndpointTests.cs`

### ValidationDiagnosticEnricherTests

Testa a logica de sugestao de mapeamento para campos pendentes. Verifica que o enricher:
- Encontra matches exatos no `CommonFieldMappingDictionary`
- Encontra matches parciais com strip de prefixos (`tc`, `Inf`, `TC`, `Tc`)
- Retorna "Manual mapping required" quando nao ha sugestao

**Localizacao:** `tests/SemanaIA.ServiceInvoice.UnitTests/SchemaEngine/ValidationDiagnosticEnricherTests.cs`

### XsdSchemaAnalyzerAttributeTests

Testa a captura de atributos XSD (como `@Id`, `@versao`) pelo analisador de schema. Garante que atributos obrigatorios sao detectados e incluidos no modelo de schema.

**Localizacao:** `tests/SemanaIA.ServiceInvoice.UnitTests/SchemaEngine/XsdSchemaAnalyzerAttributeTests.cs`

### SchemaBasedXmlSerializerAttributeTests

Testa a emissao de atributos XML pelo serializer. Garante que atributos como `@Id` e `@versao` sao emitidos corretamente no XML gerado.

**Localizacao:** `tests/SemanaIA.ServiceInvoice.UnitTests/SchemaEngine/SchemaBasedXmlSerializerAttributeTests.cs`

### TypedRuleResolverTests

Testa a resolucao de todos os 6 tipos de regra (Binding, Default, EnumMapping, ConditionalEmission, Choice, Formatting).

**Localizacao:** `tests/SemanaIA.ServiceInvoice.UnitTests/SchemaEngine/TypedRuleResolverTests.cs`

### ProviderRuleValidatorTests

Testa a validacao de regras tipadas: regras invalidas (sem target, source inexistente), regras validas, e deteccao de erros.

**Localizacao:** `tests/SemanaIA.ServiceInvoice.UnitTests/SchemaEngine/ProviderRuleValidatorTests.cs`

### RuleConditionTests

Testa a avaliacao de condicoes: operadores simples, condicoes compostas (And/Or), comparacoes numericas, operador In, IsNull, HasValue.

**Localizacao:** `tests/SemanaIA.ServiceInvoice.UnitTests/SchemaEngine/RuleConditionTests.cs`

### Outras classes relevantes

| Classe | O que testa |
|--------|------------|
| `SchemaBasedXmlSerializerTests` | Serializacao XML basica com schema |
| `SchemaBasedXmlSerializerXmlStructureTests` | Estrutura XML (namespaces, hierarquia) |
| `ServiceInvoiceSchemaDataBinderTests` | Binding de dados do dominio para schema |
| `SendXsdSelectorTests` | Selecao inteligente do XSD de envio |
| `SchemaSerializationPipelineTests` | Pipeline completa de serializacao |
| `GissonlineSchemaGenerationTests` | Auto-geracao especifica para GISSOnline |
| `OperationalStatusTests` | Calculo de status operacional |
| `ManagedProviderTests` | Modelo de dominio ManagedProvider |
| `ProviderManagementServiceTests` | Servico de gestao (mocks) |
| `GoldenMasterTests` | Snapshot tests (baseline XML) |
| `NfseRequestToDpsDocumentModelMapperTests` | Mapper request -> dominio |

---

## Convencao de Nomenclatura

Todos os testes seguem o padrao:

```
Given_<contexto>_Should_<comportamento_esperado>
```

Exemplos reais do projeto:
- `Given_RequiredAttribute_Should_IncludeInAttributes`
- `Given_ValidProvider_Should_ReturnReady`
- `Given_NullField_Should_ReturnIsNullTrue`
- `Given_EnumMapping_Should_MapToExpectedCode`
- `Given_MissingXsd_Should_ReturnBlocked`

---

## Estrutura de Teste (Arrange / Act / Assert)

Todos os testes seguem o padrao AAA:

```csharp
[Fact]
public void Given_ValidBindingRule_Should_ResolveSourceValue()
{
    // Arrange
    var rule = new ProviderRule
    {
        Type = RuleType.Binding,
        Target = "CNPJ",
        Source = "Provider.Cnpj"
    };
    var resolver = new TypedRuleResolver([rule]);

    // Act
    var result = resolver.Resolve("CNPJ", fieldPath => "12345678000199");

    // Assert
    result.ShouldBe("12345678000199");
}
```

---

## Frameworks e Ferramentas

| Framework | Uso |
|-----------|-----|
| **xUnit** | Framework de teste principal |
| **Shouldly** | Assertions fluentes (`result.ShouldBe(...)`, `result.ShouldNotBeNull()`) |
| **WebApplicationFactory** | Servidor HTTP in-process para testes de integracao |
| **MongoDB** | Service container no CI (mongo:7 via GitHub Actions) |

O projeto nao usa Moq extensivamente nos testes unitarios — a maioria dos testes trabalha com implementacoes reais dos componentes da engine, pois sao classes puras sem dependencias externas.

---

## CI: GitHub Actions

O pipeline de CI esta definido em `.github/workflows/ci.yml`:

```yaml
jobs:
  build-and-test:
    runs-on: ubuntu-latest
    services:
      mongodb:
        image: mongo:7
        ports:
          - 27017:27017
    env:
      MongoDb__ConnectionString: mongodb://localhost:27017
      MongoDb__DatabaseName: semana_ia_test
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.0.x"
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Unit Tests
        run: dotnet test tests/SemanaIA.ServiceInvoice.UnitTests --no-build -v normal
      - name: Integration Tests
        run: dotnet test tests/SemanaIA.ServiceInvoice.IntegrationsTests --no-build -v normal
```

**Pontos importantes:**
- .NET 10 (preview/RC)
- MongoDB 7 como service container — disponivel automaticamente para testes de integracao
- Triggers: push em `master`, `main`, `feature/**` e pull requests

---

## Como Executar os Testes

### Todos os testes unitarios

```bash
dotnet test tests/SemanaIA.ServiceInvoice.UnitTests/
```

### Todos os testes de integracao

**Pre-requisito:** MongoDB rodando em `localhost:27017`.

```bash
# Com Docker:
docker run -d -p 27017:27017 mongo:7

# Executar testes:
dotnet test tests/SemanaIA.ServiceInvoice.IntegrationsTests/
```

### Filtrar por classe de teste

```bash
# Apenas testes E2E de providers
dotnet test tests/SemanaIA.ServiceInvoice.IntegrationsTests/ --filter "ProviderEndToEndFlowTests"

# Apenas testes do serializer
dotnet test tests/SemanaIA.ServiceInvoice.UnitTests/ --filter "SchemaBasedXmlSerializerTests"

# Apenas testes de regras tipadas
dotnet test tests/SemanaIA.ServiceInvoice.UnitTests/ --filter "TypedRuleResolverTests"

# Apenas testes de validacao XSD
dotnet test tests/SemanaIA.ServiceInvoice.UnitTests/ --filter "XsdSchemaAnalyzerTests"
```

### Filtrar por nome de teste

```bash
dotnet test --filter "Given_RequiredAttribute_Should_IncludeInAttributes"
```

### Executar com output detalhado

```bash
dotnet test tests/SemanaIA.ServiceInvoice.UnitTests/ -v detailed --logger "console;verbosity=detailed"
```

---

## Organizacao do Codigo de Teste

```
tests/
├── SemanaIA.ServiceInvoice.UnitTests/
│   ├── SchemaEngine/                    # Testes da engine de schema
│   │   ├── XsdSchemaAnalyzerTests.cs
│   │   ├── XsdSchemaAnalyzerAttributeTests.cs
│   │   ├── XsdSchemaAnalyzerVersionTests.cs
│   │   ├── SchemaBasedXmlSerializerTests.cs
│   │   ├── SchemaBasedXmlSerializerAttributeTests.cs
│   │   ├── SchemaBasedXmlSerializerXmlStructureTests.cs
│   │   ├── ServiceInvoiceSchemaDataBinderTests.cs
│   │   ├── SchemaSerializationPipelineTests.cs
│   │   ├── SendXsdSelectorTests.cs
│   │   ├── TypedRuleResolverTests.cs
│   │   ├── TypedRuleIntegrationTests.cs
│   │   ├── ProviderRuleValidatorTests.cs
│   │   ├── RuleConditionTests.cs
│   │   ├── ValidationDiagnosticEnricherTests.cs
│   │   ├── OperationalStatusTests.cs
│   │   ├── GissonlineSchemaGenerationTests.cs
│   │   ├── GenerateNacionalArtifactsTests.cs
│   │   ├── BaselineComparisonAnalyzerTests.cs
│   │   └── SchemaCodeGeneratorTests.cs
│   ├── ProviderManagement/              # Testes de gestao de providers
│   │   ├── Application/
│   │   │   └── ProviderManagementServiceTests.cs
│   │   └── ManagedProviderTests.cs
│   ├── Mappers/                         # Testes de mapeamento
│   │   └── NfseRequestToDpsDocumentModelMapperTests.cs
│   ├── Manual/                          # Testes do serializer manual
│   │   ├── NationalDpsManualSerializerTests.cs
│   │   └── IbsCbsManualBuilderTests.cs
│   └── Snapshots/                       # Testes de snapshot (golden master)
│       └── GoldenMasterTests.cs
│
└── SemanaIA.ServiceInvoice.IntegrationsTests/
    ├── NfseEndpointIntegrationTests.cs
    ├── ProviderManagementEndpointTests.cs
    ├── ProviderEndToEndFlowTests.cs
    ├── ProviderFullLoadTests.cs
    └── ProviderOnboardingLoadTests.cs
```
