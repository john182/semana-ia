# Tasks: implement-ibscbs-gap-from-xsd-and-production-rules

## 1. Expandir modelo de domínio IbsCbs

- [x] 1.1 Criar `IbsCbsModels.cs` em `Domain/Models/` com enums: `IbsCbsPurpose` (Regular=1, Replacement=2, Complementary=3), `IbsCbsOperationType`, `IbsCbsDestinationIndicator` (SameAsBuyer=1, DifferentFromBuyer=2), `IbsCbsReimbursementType`
- [x] 1.2 Expandir `IbsCbs` com campos: `Purpose`, `IsDonation`, `PersonalUse`, `OperationIndicator`, `OperationType`, `DestinationIndicator`, `SituationCode`, `Basis`, `ReimbursedResuppliedAmount`, `IbsCbsDeductionReductionAmount`
- [x] 1.3 Criar sub-modelos: `IbsCbsRelatedDocs` (Items: List<string>), `IbsCbsGovernmentPurchase` (EntityType, OperationType), `IbsCbsRegularTaxation` (SituationCode, ClassCode), `IbsCbsRecipient` (Person reutilizado)
- [x] 1.4 Criar sub-modelos de reembolso: `IbsCbsThirdPartyReimbursements` (Documents: List<IbsCbsReimbursementDocument>), `IbsCbsReimbursementDocument` (OtherNationalDfe, OtherFiscalDoc, Supplier, IssueDate, AccrualOn, ReimbursementType, Amount), `IbsCbsDfeNacional` (DfeType, DfeTypeText, DfeKey), `IbsCbsFiscalDoc` (IssuerCityCode, FiscalDocNumber, FiscalDocDescription)
- [x] 1.5 Associar novos campos em `IbsCbs`: `RelatedDocs`, `GovernmentPurchase`, `RegularTaxation`, `ThirdPartyReimbursements`, `Recipient` (Person?), usar `RealEstate` existente em `DpsDocument`

## 2. Implementar IbsCbsManualBuilder

- [x] 2.1 Criar `IbsCbsManualBuilder` em `XmlGeneration/Manual/` com método público `Build(DpsDocument) → Action<dynamic>?`
- [x] 2.2 Implementar `WriteHeader`: finNFSe (int Purpose), indFinal (PersonalUse → "1"/"0"), cIndOp (OperationIndicator PadLeft 6), tpOper (opcional), gRefNFSe (RelatedDocs.Items foreach refNFSe, max 99), tpEnteGov (GovernmentPurchase.EntityType), indDest (int DestinationIndicator)
- [x] 2.3 Implementar `WriteDestination`: emitir `<dest>` quando DestinationIndicator=DifferentFromBuyer e Recipient não null, com choice CNPJ/CPF/NIF/NaoNIF + xNome + end (reutilizar BuildEndereco do serializer) + email
- [x] 2.4 Implementar `WriteImovel`: emitir `<imovel>` quando RealEstate não null, com inscImobFisc (opcional) + choice cCIB/end (reutilizar BuildEnderecoSimples)
- [x] 2.5 Implementar `WriteValores`: emitir `<valores>` com gReeRepRes (documentos de reembolso) condicional + trib→gIBSCBS obrigatório
- [x] 2.6 Implementar `WriteThirdPartyReimbursements`: emitir `<gReeRepRes>` com foreach documentos, cada um com choice dFeNacional/docFiscalOutro + fornec (CNPJ/CPF/NIF/cNaoNIF + xNome) + dtEmiDoc + dtCompDoc + tpReeRepRes (D2) + vlrReeRepRes
- [x] 2.7 Implementar `WriteTribIbsCbs`: emitir `<trib><gIBSCBS>` com CST (SituationCode ?? ClassCode[..3], PadLeft 3), cClassTrib (ClassCode PadLeft 6), gTribRegular condicional (CSTReg + cClassTribReg)

## 3. Integrar no serializer

- [x] 3.1 Alterar `NationalDpsManualSerializer.BuildIbsCbs` para delegar para `IbsCbsManualBuilder.Build(doc)`
- [x] 3.2 Garantir que `BuildEndereco` e `BuildEnderecoSimples` são acessíveis pelo builder (extrair para método estático ou passar como delegate)

## 4. Expandir mapper

- [x] 4.1 Criar `IbsCbsRequest` DTO em `Api/Requests/Groups/` com todos os campos do IBSCBS do contrato OpenAPI
- [x] 4.2 Alterar mapper: deserializar `request.IbsCbs` (object?) para `IbsCbsRequest` via JsonSerializer, converter para `IbsCbs` de domínio com `MapIbsCbs`
- [x] 4.3 Mapear sub-modelos: RelatedDocs, GovernmentPurchase, RegularTaxation, ThirdPartyReimbursements, Recipient

## 5. Testes

- [x] 5.1 Criar `IbsCbsManualBuilderTests` em `tests/.../Manual/` seguindo skills de teste (Given_Should, Shouldly, Arrange/Act/Assert, helpers no final)
- [x] 5.2 Criar `IbsCbsTestFixture` com cenários `CreateMinimal()` e `CreateComplete()`
- [x] 5.3 Criar `IbsCbsDocumentBuilder` com métodos fluentes: `WithDestination()`, `WithRealEstate()`, `WithThirdPartyReimbursements()`, `WithRegularTaxation()`, `WithRelatedDocs()`, `WithGovernmentPurchase()`
- [x] 5.4 Teste cenário mínimo: CST + cClassTrib, sem dest/imovel/gReeRepRes — com validação XSD
- [x] 5.5 Teste com destinatário: CNPJ + xNome + end — com validação XSD
- [x] 5.6 Teste com imóvel (cCIB) — com validação XSD
- [x] 5.7 Teste com reembolsos de terceiros (dFeNacional + fornec) — com validação XSD
- [x] 5.8 Teste com tributação regular (gTribRegular) — com validação XSD
- [x] 5.9 Teste com gRefNFSe (NFS-e referenciadas) — com validação XSD
- [x] 5.10 Teste IbsCbs null → sem bloco IBSCBS — com validação XSD

## 6. Build e validação

- [x] 6.1 `dotnet build` sem erros
- [x] 6.2 `dotnet test` com todos os testes passando (existentes + novos)