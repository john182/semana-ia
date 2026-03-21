# Change: implement-advanced-ibscbs-subblocks

## Why

O builder IBSCBS emite `gIBSCBS` com `CST`, `cClassTrib` e `gTribRegular`, mas o XSD do DPS também prevê `gDif` (diferimento) como bloco opcional dentro de `gIBSCBS`. O bloco `gDif` contém 3 campos obrigatórios (`pDifUF`, `pDifMun`, `pDifCBS`) que definem os percentuais de diferimento para IBS estadual, IBS municipal e CBS. Este bloco é relevante para operações com diferimento fiscal e está ausente tanto no serializer manual quanto no modelo de domínio.

Nota: Os sub-blocos `ibs`/`cbs` com rates/amounts/deferments do contrato OpenAPI/YAML **não existem no XSD do DPS** — são campos do contrato HTTP que não são serializados no XML. Apenas `gDif` (percentuais declarados pelo emitente) é parte do DPS.

## What Changes

- Criar modelo de domínio `IbsCbsDeferment` com `StateDefermentRate`, `MunicipalDefermentRate`, `CbsDefermentRate`.
- Adicionar `Deferment` (IbsCbsDeferment?) ao modelo `IbsCbs`.
- Expandir `IbsCbsManualBuilder` para emitir `<gDif>` dentro de `<gIBSCBS>` quando `Deferment` presente.
- Criar DTO `IbsCbsDefermentRequest` e expandir `IbsCbsRequest`.
- Expandir mapper para converter deferment do request para domínio.
- Criar testes com validação XSD.

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-serializer-manual`: Bloco `gDif` adicionado ao IBSCBS no serializer manual.

## Impact

- **Domain**: +1 classe `IbsCbsDeferment`, +1 campo em `IbsCbs`.
- **XmlGeneration/Manual**: ~10 linhas adicionais no `IbsCbsManualBuilder`.
- **Api/Requests**: +1 sub-DTO `IbsCbsDefermentRequest`.
- **Api/Mappers**: +1 método privado `MapDeferment`.
- **Tests**: +2 testes (com deferment, sem deferment).
- Change pequena e focada.
