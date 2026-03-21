# Tasks: integrate-manual-serializer-endpoint

## 1. Expandir o mapper

- [x] 1.1 Expandir `NfseRequestToDpsDocumentModelMapper.Map()` para mapear `CityServiceCode`, `AdditionalInformation` e `Location` (quando presente) para `DpsDocument`
- [x] 1.2 Expandir mapeamento de `Values`: `DiscountUnconditionedAmount`, `DiscountConditionedAmount`, `IssRate`, `ImmunityType`, `RetentionType`, `CstPisCofins`, `PisCofinsBaseTax`, `PisRate`, `CofinsRate`, `PisAmount`, `PisAmountWithheld`, `CofinsAmount`, `CofinsAmountWithheld`, `InssAmountWithheld`, `IrAmountWithheld`, `CsllAmountWithheld`
- [x] 1.3 Mapear `Intermediary` (PartyRequest? → Person?) com Name, FederalTaxNumber, Address, Email, PhoneNumber, MunicipalTaxNumber, Caepf, NoTaxIdReason
- [x] 1.4 Mapear `Borrower` expandido: Email, PhoneNumber, MunicipalTaxNumber, Caepf, NoTaxIdReason (campos que existem em PartyRequest mas não eram mapeados)
- [x] 1.5 Mapear grupos opcionais: `ForeignTrade`, `Lease`, `Construction` (com WorkId), `ActivityEvent`, `AdditionalInformationGroup` (com Items), `Deduction`, `Benefit`, `Suspension`, `ApproximateTotals` (com TaxTier fed/est/mun), `IbsCbs`
- [x] 1.6 Extrair métodos privados por grupo no mapper para manter legibilidade: `MapAddress`, `MapPerson`, `MapValues`, `MapLocation`, etc.

## 2. Trocar serializer no pipeline

- [x] 2.1 Alterar `GenerateNfseXmlUseCase` para depender de `NationalDpsManualSerializer` ao invés de `NationalNfseXmlSerializer`
- [x] 2.2 Atualizar `Program.cs`: registrar `NationalDpsManualSerializer` no DI e remover registro de `NationalNfseXmlSerializer`

## 3. Testes unitários do mapper

- [x] 3.1 Criar `NfseRequestToDpsDocumentModelMapperTests` em `tests/SemanaIA.ServiceInvoice.UnitTests/Mappers/`
- [x] 3.2 Criar `NfseGenerateXmlRequestBuilder` para massa de teste do request (cenário mínimo válido por padrão, métodos fluentes de domínio)
- [x] 3.3 Teste cenário mínimo: request mínimo → DpsDocument com Provider, Borrower, Service, Values preenchidos e grupos opcionais null
- [x] 3.4 Teste cenário completo: request completo → DpsDocument com todos os grupos preenchidos
- [x] 3.5 Teste mapeamento TaxationType: string "Export" → enum TaxationType.Export
- [x] 3.6 Teste mapeamento Intermediary presente → Person com dados corretos
- [x] 3.7 Teste mapeamento Intermediary ausente → null
- [x] 3.8 Teste mapeamento ForeignTrade presente → ForeignTrade com campos corretos
- [x] 3.9 Teste mapeamento ApproximateTotals com tiers → ApproximateTotals com Federal/State/Municipal

## 4. Testes de integração do endpoint

- [x] 4.1 Configurar `SemanaIA.ServiceInvoice.IntegrationsTests.csproj`: adicionar referência ao projeto Api, `Microsoft.AspNetCore.Mvc.Testing`, `Shouldly`
- [x] 4.2 Remover `UnitTest1.cs` placeholder
- [x] 4.3 Criar `NfseEndpointIntegrationTests` usando `WebApplicationFactory<Program>`
- [x] 4.4 Teste request mínimo: POST com JSON mínimo → 200 OK, response contém Xml, RootElement="DPS"
- [x] 4.5 Teste request completo: POST com JSON completo → 200 OK, XML contém blocos expandidos (interm, tribFed, comExt)

## 5. Build e validação

- [x] 5.1 `dotnet build` sem erros
- [x] 5.2 `dotnet test` com todos os testes passando (unit + integration)