# Design: nfse-openapi-sync-expand-dtos

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.Api/
  Requests/
    NfseGenerateXmlRequest.cs              ← raiz expandida (campos escalares + referências aos grupos)
    Groups/
      AddressRequest.cs                    ← LocationRequest, AddressRequest, CityRequest (extraídos)
      PartyRequest.cs                      ← partyDefinition completo + enums PartyType / TaxRegime / NoTaxIdReason
      ActivityEventRequest.cs
      ReferenceSubstitutionRequest.cs
      LeaseRequest.cs
      ConstructionRequest.cs
      RealEstateRequest.cs
      ForeignTradeRequest.cs
      DeductionRequest.cs                  ← inclui DeductionDocumentRequest
      BenefitRequest.cs
      SuspensionRequest.cs
      ApproximateTaxRequest.cs
      ApproximateTotalsRequest.cs
      AdditionalInformationGroupRequest.cs
      ServiceAmountDetailsRequest.cs
  Swagger/
    Filters/
      NfseExamplesOperationFilter.cs       ← IOperationFilter (Swashbuckle)
    Examples/
      NfseRequestExamplesFactory.cs        ← métodos estáticos retornando object anônimo serializado
```

## Decisões técnicas

### DTOs

- Classes simples com `{ get; set; }`. Sem records — mutabilidade necessária para model binding do ASP.NET.
- Campos opcionais no YAML → nullable (`?`) no C#.
- Enums C# espelham os valores do YAML em PascalCase (ex: `TaxationType.WithinCity`).
- `BorrowerRequest` herda de `PartyRequest` para manter retrocompatibilidade com o mapper existente.
- `Intermediary` e `Recipient` tipados como `PartyRequest?` na raiz.
- `IbsCbs` exposto como `object?` placeholder sem classe própria — change futura dedicada.

### Exemplos no Swagger

- `NfseRequestExamplesFactory`: métodos estáticos retornando `object` anônimo — sem dependência de tipo serializado, fácil de manter em paralelo ao YAML.
- `NfseExamplesOperationFilter`: localiza a operação POST pelo caminho `/api/v1/nfse/xml` e adiciona os três exemplos usando `OpenApiRawString` via `System.Text.Json.JsonSerializer.Serialize(...)`.
- Registro: `builder.Services.AddSwaggerGen(o => o.OperationFilter<NfseExamplesOperationFilter>())`.

### XML doc comments

- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` + `<NoWarn>$(NoWarn);1591</NoWarn>` no `.csproj`.
- `c.IncludeXmlComments(xmlPath)` no `AddSwaggerGen`.
- `<summary>` nos campos de semântica crítica: `TaxationType`, `ImmunityType`, `RetentionType`, `DeductionsAmount`, `FederalTaxNumber`.

### Compatibilidade com mapper existente

`NfseRequestToDpsDocumentModelMapper` usa apenas os campos já existentes em `NfseGenerateXmlRequest` + `BorrowerRequest`.
A expansão é 100% aditiva — todos os novos campos são opcionais. O mapper não quebra e não precisa ser alterado nesta change.

## Mapeamento YAML → C#

| Schema YAML                  | Classe C#                            | Usado em (campo raiz)                          |
|------------------------------|--------------------------------------|------------------------------------------------|
| `partyDefinition`            | `PartyRequest`                       | `borrower`, `intermediary`, `recipient`, `supplier` em dedução |
| `addressDefinition`          | `AddressRequest` / `LocationRequest` | `location`, `address` dentro de party          |
| `activityEvent`              | `ActivityEventRequest`               | `activityEvent`                                |
| `ReferenceSubstitution`      | `ReferenceSubstitutionRequest`       | `referenceSubstitution`                        |
| `lease`                      | `LeaseRequest`                       | `lease`                                        |
| `construction`               | `ConstructionRequest`                | `construction`                                 |
| `realEstate`                 | `RealEstateRequest`                  | `realEstate`                                   |
| `foreignTrade`               | `ForeignTradeRequest`                | `foreignTrade`                                 |
| `deduction`                  | `DeductionRequest`                   | `deduction`                                    |
| `deductionDocument`          | `DeductionDocumentRequest`           | `deduction.documents[]`                        |
| `benefit`                    | `BenefitRequest`                     | `benefit`                                      |
| `suspension`                 | `SuspensionRequest`                  | `suspension`                                   |
| `approximateTax`             | `ApproximateTaxRequest`              | `approximateTax`                               |
| `approximateTotals`          | `ApproximateTotalsRequest`           | `approximateTotals`                            |
| `additionalInformationGroup` | `AdditionalInformationGroupRequest`  | `additionalInformationGroup`                   |
| `serviceAmountDefinitions`   | `ServiceAmountDetailsRequest`        | `serviceAmountDetails`                         |
| `ibsCbs`                     | `object?` (placeholder)              | `ibsCbs`                                       |

## Campos raiz faltantes a adicionar em NfseGenerateXmlRequest

| Campo YAML                      | Tipo C#          |
|---------------------------------|------------------|
| `cityServiceCode`               | `string?`        |
| `cnaeCode`                      | `string?`        |
| `ncmCode`                       | `string?`        |
| `paidAmount`                    | `decimal?`       |
| `accrualOn`                     | `DateOnly?`      |
| `cstPisCofins`                  | `string?`        |
| `deductionsAmount`              | `decimal?`       |
| `discountUnconditionedAmount`   | `decimal?`       |
| `discountConditionedAmount`     | `decimal?`       |
| `irAmountWithheld`              | `decimal?`       |
| `pisCofinsBaseTax`              | `decimal?`       |
| `pisRate`                       | `decimal?`       |
| `pisAmount`                     | `decimal?`       |
| `pisAmountWithheld`             | `decimal?`       |
| `cofinsRate`                    | `decimal?`       |
| `cofinsAmount`                  | `decimal?`       |
| `cofinsAmountWithheld`          | `decimal?`       |
| `csllAmountWithheld`            | `decimal?`       |
| `inssAmountWithheld`            | `decimal?`       |
| `issRate`                       | `decimal?`       |
| `issTaxAmount`                  | `decimal?`       |
| `issAmountWithheld`             | `decimal?`       |
| `ipiRate`                       | `decimal?`       |
| `ipiAmount`                     | `decimal?`       |
| `othersAmountWithheld`          | `decimal?`       |
| `additionalInformation`         | `string?`        |
| `isEarlyInstallmentPayment`     | `bool?`          |
| `immunityType`                  | `string?`        |
| `retentionType`                 | `string?`        |
| `ibsCbs`                        | `object?`        |

