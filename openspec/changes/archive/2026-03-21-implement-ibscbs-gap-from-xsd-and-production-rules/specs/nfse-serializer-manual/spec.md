# Delta Spec: nfse-serializer-manual

## MODIFIED Requirements

### Requirement: IBSCBS conditional inclusion

O serializer MUST gerar `<IBSCBS>` (tipo `TCRTCInfoIBSCBS`) completo quando `IbsCbs.ClassCode` não é null. O bloco MUST conter, na ordem do XSD: `finNFSe` (obrigatório), `indFinal` (obrigatório), `cIndOp` (obrigatório), `tpOper` (opcional), `gRefNFSe` (opcional), `tpEnteGov` (opcional), `indDest` (obrigatório), `dest` (opcional), `imovel` (opcional), `valores` (obrigatório com `trib→gIBSCBS`).

#### Scenario: IBSCBS minimal (CST + cClassTrib only)
- **WHEN** IbsCbs com ClassCode="000001", Purpose=Regular, PersonalUse=false, OperationIndicator="1005011", DestinationIndicator=SameAsBuyer
- **THEN** o XML contém `<IBSCBS>` com `<finNFSe>`, `<indFinal>0</indFinal>`, `<cIndOp>1005011</cIndOp>`, `<indDest>`, `<valores><trib><gIBSCBS><CST>000</CST><cClassTrib>000001</cClassTrib></gIBSCBS></trib></valores>` e sem `<dest>`, `<imovel>`, `<gRefNFSe>`, `<tpOper>`, `<tpEnteGov>`

#### Scenario: IBSCBS with destination (recipient)
- **WHEN** IbsCbs com DestinationIndicator=DifferentFromBuyer e Recipient preenchido com CNPJ e endereço
- **THEN** o XML contém `<dest>` com `<CNPJ>`, `<xNome>`, `<end>` dentro de `<IBSCBS>`

#### Scenario: IBSCBS with real estate (imovel)
- **WHEN** IbsCbs com RealEstate preenchido com CibCode
- **THEN** o XML contém `<imovel><cCIB>...</cCIB></imovel>` dentro de `<IBSCBS>`

#### Scenario: IBSCBS with third-party reimbursements
- **WHEN** IbsCbs com ThirdPartyReimbursements contendo documentos com dFeNacional e fornecedor
- **THEN** o XML contém `<valores><gReeRepRes><documentos>...<dFeNacional>...<fornec>...</fornec>...</documentos></gReeRepRes><trib>...</trib></valores>`

#### Scenario: IBSCBS with regular taxation
- **WHEN** IbsCbs com RegularTaxation preenchido com ClassCode e SituationCode
- **THEN** o XML contém `<gIBSCBS><CST>...</CST><cClassTrib>...</cClassTrib><gTribRegular><CSTReg>...</CSTReg><cClassTribReg>...</cClassTribReg></gTribRegular></gIBSCBS>`

#### Scenario: IBSCBS with referenced NFS-e
- **WHEN** IbsCbs com RelatedDocs contendo 2 chaves de NFS-e referenciadas
- **THEN** o XML contém `<gRefNFSe><refNFSe>...</refNFSe><refNFSe>...</refNFSe></gRefNFSe>`

#### Scenario: IBSCBS with government purchase
- **WHEN** IbsCbs com GovernmentPurchase.EntityType preenchido
- **THEN** o XML contém `<tpEnteGov>...</tpEnteGov>` dentro de `<IBSCBS>`

#### Scenario: IBSCBS null
- **WHEN** IbsCbs é null ou ClassCode é null
- **THEN** o XML não contém `<IBSCBS>`
