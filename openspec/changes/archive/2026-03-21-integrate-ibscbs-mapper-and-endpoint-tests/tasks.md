# Tasks: integrate-ibscbs-mapper-and-endpoint-tests

## 1. Criar IbsCbsRequest DTO

- [x] 1.1 Criar `IbsCbsRequest` em `Api/Requests/Groups/IbsCbsRequest.cs` com campos: `Purpose` (string?), `IsDonation` (bool?), `PersonalUse` (bool?), `OperationIndicator` (string?), `DestinationIndicator` (string?), `SituationCode` (string?), `ClassCode` (string?), `Basis` (decimal?), `ReimbursedResuppliedAmount` (decimal?), `IbsCbsDeductionReductionAmount` (decimal?), `OperationType` (string?)
- [x] 1.2 Criar sub-DTOs: `IbsCbsRelatedDocsRequest` (Items: List<string>?), `IbsCbsGovernmentPurchaseRequest` (EntityType: string?), `IbsCbsRegularTaxationRequest` (SituationCode, ClassCode), `IbsCbsRealEstateRequest` (PropertyFiscalRegistration, CibCode, SiteAddress: AddressRequest?)
- [x] 1.3 Criar sub-DTOs de reembolso: `IbsCbsThirdPartyReimbursementsRequest` (Documents: List<IbsCbsReimbursementDocumentRequest>?), `IbsCbsReimbursementDocumentRequest` (OtherNationalDfe, OtherFiscalDoc, Supplier: PartyRequest?, IssueDate, AccrualOn, ReimbursementType, Amount), `IbsCbsDfeNacionalRequest` (DfeType, DfeTypeText, DfeKey), `IbsCbsFiscalDocRequest` (IssuerCityCode, FiscalDocNumber, FiscalDocDescription)
- [x] 1.4 Adicionar `Recipient` (PartyRequest?) ao `IbsCbsRequest`
- [x] 1.5 Alterar `NfseGenerateXmlRequest.IbsCbs` de `object?` para `IbsCbsRequest?`

## 2. Expandir mapper

- [x] 2.1 Criar `MapIbsCbs(IbsCbsRequest?)` no mapper retornando `IbsCbs?` de domínio
- [x] 2.2 Mapear campos escalares: Purpose (string→enum), PersonalUse, OperationIndicator, ClassCode, SituationCode, DestinationIndicator (string→enum), Basis, ReimbursedResuppliedAmount, DeductionReductionAmount, OperationType (string→enum)
- [x] 2.3 Mapear RelatedDocs, GovernmentPurchase (EntityType string→enum), RegularTaxation
- [x] 2.4 Mapear ThirdPartyReimbursements com documentos (DfeNacional, FiscalDoc, Supplier→Person, datas, ReimbursementType string→enum, Amount)
- [x] 2.5 Mapear Recipient (PartyRequest→Person via MapPerson existente)
- [x] 2.6 Mapear RealEstate (IbsCbsRealEstateRequest→RealEstate)
- [x] 2.7 Remover hardcode `new IbsCbs { ClassCode = "000001" }` e substituir pela chamada a `MapIbsCbs`

## 3. Testes unitários do mapper IBSCBS

- [x] 3.1 Adicionar `WithIbsCbs()` e `WithIbsCbsFull()` ao `NfseGenerateXmlRequestBuilder`
- [x] 3.2 Teste: IBSCBS mínimo → ClassCode, Purpose, OperationIndicator mapeados
- [x] 3.3 Teste: IBSCBS com destinatário → Recipient mapeado como Person
- [x] 3.4 Teste: IBSCBS com reembolsos → ThirdPartyReimbursements.Documents mapeados
- [x] 3.5 Teste: IBSCBS null → IbsCbs null no domínio

## 4. Testes de integração

- [x] 4.1 Teste: POST com JSON contendo IBSCBS → 200 OK, XML contém `<IBSCBS>`, `<finNFSe>`, `<gIBSCBS>`
- [x] 4.2 Teste: POST sem IBSCBS → 200 OK, XML não contém `<IBSCBS>` (já coberto pelo teste mínimo existente)

## 5. Build e validação

- [x] 5.1 `dotnet build` sem erros
- [x] 5.2 `dotnet test` com todos os testes passando
