# Tasks: nfse-openapi-sync-expand-dtos

> Ordem de execução recomendada: T-01 → T-02 → T-03 → T-04 → T-05 → T-06 → T-07 → T-08 → T-09 → T-10 → T-11

---

## T-01 — Extrair AddressRequest para arquivo próprio

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/Requests/Groups/AddressRequest.cs`

- Criar o arquivo e mover as classes `LocationRequest`, `AddressRequest` e `CityRequest` de `NfseGenerateXmlRequest.cs` para ele.
- Manter `namespace SemanaIA.ServiceInvoice.Api.Requests`.
- Remover as três classes do arquivo original.

---

## T-02 — Criar PartyRequest

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/Requests/Groups/PartyRequest.cs`

Criar a classe `PartyRequest` com todos os campos de `partyDefinition` do YAML:

```csharp
public class PartyRequest
{
    public string? Type { get; set; }               // enum: Undefined | NaturalPerson | LegalEntity
    public string? Name { get; set; }
    public long? FederalTaxNumber { get; set; }     // CNPJ, CPF ou NIF
    public string? MunicipalTaxNumber { get; set; }
    public string? StateTaxNumber { get; set; }
    public string? TaxRegime { get; set; }          // enum: Isento | MicroempreendedorIndividual | SimplesNacional | LucroPresumido | LucroReal
    public string? Caepf { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? NoTaxIdReason { get; set; }      // enum: NotInformedOriginal | Exempted | NotRequired
    public AddressRequest? Address { get; set; }
}
```

---

## T-03 — Adaptar BorrowerRequest e adicionar Intermediary / Recipient

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/Requests/NfseGenerateXmlRequest.cs`

- `BorrowerRequest` passa a herdar de `PartyRequest` (mantém retrocompatibilidade com o mapper: `Name`, `FederalTaxNumber` e `Address` continuam acessíveis).
- Adicionar em `NfseGenerateXmlRequest`:
  - `public PartyRequest? Intermediary { get; set; }`
  - `public PartyRequest? Recipient { get; set; }`

---

## T-04 — Completar campos escalares raiz

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/Requests/NfseGenerateXmlRequest.cs`

Adicionar na classe `NfseGenerateXmlRequest` os campos faltantes (todos opcionais):

```csharp
public string? CityServiceCode { get; set; }
public string? CnaeCode { get; set; }
public string? NcmCode { get; set; }
public decimal? PaidAmount { get; set; }
public DateOnly? AccrualOn { get; set; }
public string? CstPisCofins { get; set; }
public decimal? DeductionsAmount { get; set; }
public decimal? DiscountUnconditionedAmount { get; set; }
public decimal? DiscountConditionedAmount { get; set; }
public decimal? IrAmountWithheld { get; set; }
public decimal? PisCofinsBaseTax { get; set; }
public decimal? PisRate { get; set; }
public decimal? PisAmount { get; set; }
public decimal? PisAmountWithheld { get; set; }
public decimal? CofinsRate { get; set; }
public decimal? CofinsAmount { get; set; }
public decimal? CofinsAmountWithheld { get; set; }
public decimal? CsllAmountWithheld { get; set; }
public decimal? InssAmountWithheld { get; set; }
public decimal? IssRate { get; set; }
public decimal? IssTaxAmount { get; set; }
public decimal? IssAmountWithheld { get; set; }
public decimal? IpiRate { get; set; }
public decimal? IpiAmount { get; set; }
public decimal? OthersAmountWithheld { get; set; }
public string? AdditionalInformation { get; set; }
public bool? IsEarlyInstallmentPayment { get; set; }
public string? ImmunityType { get; set; }
public string? RetentionType { get; set; }
public object? IbsCbs { get; set; }  // placeholder — change futura dedicada
```

---

## T-05 — Criar grupos complexos

Criar um arquivo por grupo em `src/SemanaIA.ServiceInvoice.Api/Requests/Groups/`.

### T-05a — ActivityEventRequest.cs
```csharp
public class ActivityEventRequest
{
    public string? Name { get; set; }
    public DateTimeOffset? BeginOn { get; set; }
    public DateTimeOffset? EndOn { get; set; }
    public string? Code { get; set; }
    public AddressRequest? Address { get; set; }
}
```

### T-05b — ReferenceSubstitutionRequest.cs
```csharp
public class ReferenceSubstitutionRequest
{
    public string? Id { get; set; }
    // enum: SnOut | SnIn | ImmunityAddRetro | ImmunityRemoveRetro | RejectionBuyerOrIntermediary | Other
    public string? Reason { get; set; }
    public string? ReasonText { get; set; }
}
```

### T-05c — LeaseRequest.cs
```csharp
public class LeaseRequest
{
    // enum: Lease | Sublease | Leasehold | RightOfWay | UsePermission
    public string? Category { get; set; }
    // enum: Railway | Road | Poles | Cables | Pipelines | Conduits
    public string? ObjectType { get; set; }
    public decimal? TotalLength { get; set; }
    public int? PolesCount { get; set; }
}
```

### T-05d — ConstructionRequest.cs
```csharp
public class ConstructionRequest
{
    public string? PropertyFiscalRegistration { get; set; }
    public ConstructionWorkIdRequest? WorkId { get; set; }
    public string? CibCode { get; set; }
    public AddressRequest? SiteAddress { get; set; }
}

public class ConstructionWorkIdRequest
{
    // enum: "bra.cno" | "bra.cei"
    public string? Scheme { get; set; }
    public string? Value { get; set; }
}
```

### T-05e — RealEstateRequest.cs
```csharp
public class RealEstateRequest
{
    public string? PropertyFiscalRegistration { get; set; }
    public string? CibCode { get; set; }
    public AddressRequest? SiteAddress { get; set; }
}
```

### T-05f — ForeignTradeRequest.cs
```csharp
public class ForeignTradeRequest
{
    // enum: Unknown | CrossBorder | ConsumptionInBrazil | TemporaryPersonnel | ConsumptionAbroad
    public string? ServiceMode { get; set; }
    // enum: NoLink | Controlled | Controller | Affiliate | HeadOffice | Branch | OtherLink
    public string? RelationShip { get; set; }
    public string? Currency { get; set; }
    public decimal? ServiceAmountInCurrency { get; set; }
    // enum: Unknown | None | Acc | Ace | BndesEximPostShipServices | BndesEximPreShipServices | Fge | ProexEqualization | ProexFinancing
    public string? SupportMechanismProvider { get; set; }
    // enum: Unknown | None | PublicAdminAndInternationalRep | ... | Zpe  (ver YAML para lista completa)
    public string? SupportMechanismReceiver { get; set; }
    // enum: Unknown | No | LinkedImportDeclaration | LinkedExportDeclaration
    public string? TemporaryGoods { get; set; }
    public string? ImportDeclaration { get; set; }
    public string? ExportRegistration { get; set; }
    public bool? MdicDelivery { get; set; }
}
```

### T-05g — DeductionRequest.cs
```csharp
public class DeductionRequest
{
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public List<DeductionDocumentRequest>? Documents { get; set; }
}

public class DeductionDocumentRequest
{
    public string? NfseKey { get; set; }
    public string? NfeKey { get; set; }
    public MunicipalElectronicDocRequest? MunicipalElectronic { get; set; }
    public NonElectronicDocRequest? NonElectronic { get; set; }
    public string? OtherFiscalId { get; set; }
    public string? OtherDocId { get; set; }
    // enum: FoodAndBeverages | Materials | ConsortiumPassThrough | HealthPlanPassThrough | Services | Subcontracting | other
    public string? DeductionType { get; set; }
    public string? OtherDeductionDescription { get; set; }
    public DateOnly? IssueDate { get; set; }
    public decimal? DeductibleTotal { get; set; }
    public decimal? UsedAmount { get; set; }
    public PartyRequest? Supplier { get; set; }
}

public class MunicipalElectronicDocRequest
{
    public string? CityCode { get; set; }
    public string? Number { get; set; }
    public string? VerificationCode { get; set; }
}

public class NonElectronicDocRequest
{
    public string? Number { get; set; }
    public string? Model { get; set; }
    public string? Series { get; set; }
}
```

### T-05h — BenefitRequest.cs
```csharp
public class BenefitRequest
{
    public string? Id { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Rate { get; set; }
}
```

### T-05i — SuspensionRequest.cs
```csharp
public class SuspensionRequest
{
    // enum: Judicial | Administrative
    public string? Reason { get; set; }
    public string? ProcessNumber { get; set; }
}
```

### T-05j — ApproximateTaxRequest.cs
```csharp
public class ApproximateTaxRequest
{
    public string? Source { get; set; }
    public string? Version { get; set; }
    public decimal? TotalRate { get; set; }
    public decimal? TotalAmount { get; set; }
}
```

### T-05k — ApproximateTotalsRequest.cs
```csharp
public class ApproximateTotalsRequest
{
    public ApproximateTaxTierRequest? Federal { get; set; }
    public ApproximateTaxTierRequest? State { get; set; }
    public ApproximateTaxTierRequest? Municipal { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
}

public class ApproximateTaxTierRequest
{
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
}
```

### T-05l — AdditionalInformationGroupRequest.cs
```csharp
public class AdditionalInformationGroupRequest
{
    public string? ResponsibilityDocumentIdentifier { get; set; }
    public string? ReferencedDocument { get; set; }
    public string? Order { get; set; }
    public List<AdditionalInformationItemRequest>? Items { get; set; }
    public string? OtherInformation { get; set; }
}

public class AdditionalInformationItemRequest
{
    public string? Item { get; set; }
}
```

### T-05m — ServiceAmountDetailsRequest.cs
```csharp
public class ServiceAmountDetailsRequest
{
    public decimal? InitialChargedAmount { get; set; }
    public decimal? FinalChargedAmount { get; set; }
    public decimal? FineAmount { get; set; }
    public decimal? InterestAmount { get; set; }
}
```

---

## T-06 — Associar grupos na raiz do request

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/Requests/NfseGenerateXmlRequest.cs`

Adicionar as propriedades de grupo em `NfseGenerateXmlRequest`:

```csharp
public ActivityEventRequest? ActivityEvent { get; set; }
public ReferenceSubstitutionRequest? ReferenceSubstitution { get; set; }
public LeaseRequest? Lease { get; set; }
public ConstructionRequest? Construction { get; set; }
public RealEstateRequest? RealEstate { get; set; }
public ForeignTradeRequest? ForeignTrade { get; set; }
public DeductionRequest? Deduction { get; set; }
public BenefitRequest? Benefit { get; set; }
public SuspensionRequest? Suspension { get; set; }
public ApproximateTaxRequest? ApproximateTax { get; set; }
public ApproximateTotalsRequest? ApproximateTotals { get; set; }
public AdditionalInformationGroupRequest? AdditionalInformationGroup { get; set; }
public ServiceAmountDetailsRequest? ServiceAmountDetails { get; set; }
```

---

## T-07 — Criar NfseRequestExamplesFactory

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/Swagger/Examples/NfseRequestExamplesFactory.cs`

- Método estático `MinimumExample()`: retorna `object` anônimo com os dados do exemplo `MinimumExample` do YAML.
- Método estático `IntermediateExample()`: retorna `object` anônimo do exemplo `IntermediateExample` do YAML.
- Método estático `CompleteExample()`: retorna `object` anônimo do exemplo `CompleteExample` do YAML (sem o bloco `ibsCbs` — escopo desta change).

---

## T-08 — Criar NfseExamplesOperationFilter

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/Swagger/Filters/NfseExamplesOperationFilter.cs`

- Implementar `IOperationFilter` do Swashbuckle.
- No método `Apply`: verificar se `context.ApiDescription.HttpMethod == "POST"` e path contém `nfse/xml`.
- Adicionar três entradas em `operation.RequestBody.Content["application/json"].Examples`:
  ```
  "MinimumExample"      → Summary: "Exemplo Mínimo"
  "IntermediateExample" → Summary: "Exemplo Intermediário"
  "CompleteExample"     → Summary: "Exemplo Completo"
  ```
- Usar `new OpenApiRawString(JsonSerializer.Serialize(NfseRequestExamplesFactory.MinimumExample()))` como `Value`.

---

## T-09 — Configurar .csproj e Program.cs

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/SemanaIA.ServiceInvoice.Api.csproj`

Adicionar dentro de `<PropertyGroup>`:
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>
```

**Arquivo:** `src/SemanaIA.ServiceInvoice.Api/Program.cs`

Atualizar `AddSwaggerGen`:
```csharp
builder.Services.AddSwaggerGen(o =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    o.IncludeXmlComments(xmlPath);
    o.OperationFilter<NfseExamplesOperationFilter>();
});
```

---

## T-10 — Adicionar XML doc comments nos campos críticos

**Arquivos:** `Requests/NfseGenerateXmlRequest.cs` e `Requests/Groups/PartyRequest.cs`

Anotar com `<summary>` (descrições copiadas do YAML):

- `TaxationType` — lista de valores e nota sobre ISS vs. IBS/CBS
- `ImmunityType` — "usar apenas quando taxationType = Immune"
- `RetentionType` — os três valores e comportamento automático vs. manual
- `DeductionsAmount` — nota sobre simples vs. estruturado (grupo `deduction`)
- `PartyRequest.FederalTaxNumber` — "CNPJ, CPF ou NIF (tpCNPJ, tpCPF, tpNIF)"

---

## T-11 — Verificar e validar

- `dotnet build` sem erros de compilação.
- `dotnet run` → abrir `/swagger` → confirmar:
  - Os três exemplos aparecem no dropdown de exemplos da operação POST.
  - Os 14 grupos são visíveis no schema do request.
  - Os campos escalares expandidos aparecem na documentação.
- Confirmar que `NfseRequestToDpsDocumentModelMapper` compila sem alterações.
- Confirmar que o endpoint `/api/v1/nfse/xml` ainda responde corretamente com o payload mínimo.

