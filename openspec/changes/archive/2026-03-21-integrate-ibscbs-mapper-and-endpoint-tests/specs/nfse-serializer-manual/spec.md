# Delta Spec: nfse-serializer-manual

## MODIFIED Requirements

### Requirement: Mapper completo request-to-domain

O mapper MUST converter o campo `IbsCbs` do request para o modelo de domínio `IbsCbs` tipado, incluindo: `Purpose`, `PersonalUse`, `OperationIndicator`, `ClassCode`, `SituationCode`, `DestinationIndicator`, `Basis`, `ReimbursedResuppliedAmount`, `DeductionReductionAmount`, `RelatedDocs`, `GovernmentPurchase`, `RegularTaxation`, `ThirdPartyReimbursements` (com documentos, fornecedores, datas e tipos), `Recipient` (Person com CNPJ/CPF/NIF + endereço), `RealEstate`.

#### Scenario: Mapping IBSCBS minimal
- **WHEN** o request contém IbsCbs com classCode="000001" e operationIndicator="100501"
- **THEN** o DpsDocument.IbsCbs é tipado com ClassCode="000001", Purpose=Regular, OperationIndicator="100501"

#### Scenario: Mapping IBSCBS with destination
- **WHEN** o request contém IbsCbs com destinationIndicator="DifferentFromBuyer" e recipient preenchido
- **THEN** o DpsDocument.IbsCbs.Recipient é uma Person com dados mapeados e IbsCbs.DestinationIndicator=DifferentFromBuyer

#### Scenario: Mapping IBSCBS with third-party reimbursements
- **WHEN** o request contém IbsCbs com thirdPartyReimbursements.documents
- **THEN** o DpsDocument.IbsCbs.ThirdPartyReimbursements.Documents contém os documentos mapeados com tipos, fornecedores, datas e valores

#### Scenario: Mapping IBSCBS null
- **WHEN** o request não contém IbsCbs (null)
- **THEN** o DpsDocument.IbsCbs é null

### Requirement: Testes de integração do endpoint

O projeto MUST ter testes de integração que enviem JSON com IBSCBS ao endpoint e validem que o XML de saída contém os blocos IBSCBS.

#### Scenario: Integration test with IBSCBS
- **WHEN** um JSON com IBSCBS é enviado via POST ao endpoint
- **THEN** o response é 200 OK com XML contendo `<IBSCBS>` com `<finNFSe>`, `<indDest>`, `<gIBSCBS>`

#### Scenario: Integration test without IBSCBS
- **WHEN** um JSON sem IBSCBS é enviado via POST ao endpoint
- **THEN** o response é 200 OK com XML sem `<IBSCBS>`
