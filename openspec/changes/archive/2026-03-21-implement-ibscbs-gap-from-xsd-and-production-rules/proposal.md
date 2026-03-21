# Change: implement-ibscbs-gap-from-xsd-and-production-rules

## Why

O bloco `<IBSCBS>` é o gap #1 identificado no relatório de cobertura XSD. Atualmente o serializer manual emite apenas `<cClass>` como placeholder. Com a reforma tributária, o DPS é rejeitado quando `ClassCode` está presente mas a estrutura IBSCBS está incompleta. O código de produção (`IbsCbsNationalBuilder`) já implementa a geração completa: header (`finNFSe`, `indFinal`, `cIndOp`, `tpOper`, `gRefNFSe`, `tpEnteGov`, `indDest`), destinatário (`dest`), imóvel (`imovel`), valores (`gReeRepRes` com documentos de reembolso) e tributos (`gIBSCBS` com `CST`, `cClassTrib`, `gTribRegular`).

## What Changes

- Expandir o modelo de domínio `IbsCbs` com todos os campos necessários: `Purpose`, `IsDonation`, `PersonalUse`, `OperationIndicator`, `OperationType`, `DestinationIndicator`, `SituationCode`, `RelatedDocs`, `GovernmentPurchase`, `RegularTaxation`, `ThirdPartyReimbursements`, `Recipient`, `RealEstate` (reutilizando modelo existente).
- Substituir o `BuildIbsCbs` placeholder no `NationalDpsManualSerializer` por uma implementação completa que reproduza o comportamento de `IbsCbsNationalBuilder` de produção.
- Expandir o mapper para converter os campos IBSCBS do request DTO para o domínio expandido.
- Criar testes unitários para o builder IBSCBS com cenários: mínimo (apenas CST + cClassTrib), com destinatário, com imóvel, com reembolsos de terceiros, com tributação regular.
- Validar XML gerado contra XSD em cada teste.

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-serializer-manual`: Bloco IBSCBS completo no serializer manual, alinhado com produção e XSD.

## Impact

- **Domain**: `IbsCbs` expandido de 1 campo para ~15 campos + sub-modelos (`IbsCbsRelatedDocs`, `IbsCbsGovernmentPurchase`, `IbsCbsRegularTaxation`, `IbsCbsThirdPartyReimbursement`, `IbsCbsReimbursementDocument`).
- **XmlGeneration/Manual**: `BuildIbsCbs` reescrito com ~150 linhas, métodos privados `WriteHeader`, `WriteDestination`, `WriteImovel`, `WriteValores`, `WriteThirdPartyReimbursements`.
- **Api/Mappers**: Mapper expandido para converter IBSCBS do request (que já é `object?`) para o domínio tipado.
- **Tests**: Novos testes de serialização IBSCBS com validação XSD.
- **Nenhuma alteração** em DTOs de request, Swagger, controller ou testes de integração existentes.