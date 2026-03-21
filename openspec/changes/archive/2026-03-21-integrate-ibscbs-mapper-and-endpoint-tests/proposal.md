# Change: integrate-ibscbs-mapper-and-endpoint-tests

## Why

O builder IBSCBS (`IbsCbsManualBuilder`) e o modelo de domínio expandido já estão implementados e validados com testes XSD. Porém o fluxo ponta a ponta ainda não funciona: o mapper ignora o conteúdo real do `request.IbsCbs` (hardcoded `ClassCode = "000001"`) e o campo no DTO é `object?` sem tipagem. Um request JSON com IBSCBS completo gera XML com IBSCBS mínimo, sem destinatário, imóvel, reembolsos ou tributação regular.

## What Changes

- Criar `IbsCbsRequest` DTO tipado em `Api/Requests/Groups/` com todos os campos do contrato OpenAPI/YAML para IBSCBS.
- Alterar `NfseGenerateXmlRequest.IbsCbs` de `object?` para `IbsCbsRequest?` (tipagem forte).
- Expandir o mapper com `MapIbsCbs` que converte `IbsCbsRequest` para `IbsCbs` de domínio, incluindo sub-modelos (RelatedDocs, GovernmentPurchase, RegularTaxation, ThirdPartyReimbursements, Recipient, RealEstate).
- Criar testes unitários do mapper para cenários IBSCBS (mínimo, com destinatário, com reembolsos, null).
- Expandir testes de integração do endpoint com request contendo IBSCBS e validar que o XML de saída contém os blocos esperados.

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-serializer-manual`: Fluxo ponta a ponta IBSCBS completo (Request → Mapper → Domínio → Serializer → XML).

## Impact

- **Api/Requests**: Novo `IbsCbsRequest` DTO + sub-DTOs. `NfseGenerateXmlRequest.IbsCbs` muda de `object?` para `IbsCbsRequest?`.
- **Api/Mappers**: `MapIbsCbs` + métodos privados por sub-grupo.
- **Swagger**: Exemplos existentes já incluem IBSCBS no YAML — o Swagger passará a mostrar o schema tipado automaticamente.
- **Tests/UnitTests**: Novos testes de mapper IBSCBS.
- **Tests/IntegrationTests**: Novo teste de endpoint com IBSCBS.
- **Nenhuma alteração** no serializer, builder ou modelo de domínio.
