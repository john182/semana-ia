# Change: bind-serviceinvoice-to-runtime-schema-serializer

## Why

O `SchemaBasedXmlSerializer` gera XML válido a partir de `Dictionary<string, object?>` + `SchemaModel` + `ProviderRuleResolver`. Porém o fluxo real da aplicação opera com `DpsDocument` (domínio) — que vem do request API mapeado e/ou do documento persistido no Mongo. Não existe a camada que conecta o domínio ao serializer runtime.

Sem essa camada, o serializer runtime é uma prova de conceito isolada — funciona com dicionários montados manualmente nos testes, mas não com dados reais da aplicação. Para que a POC prove a viabilidade completa da migração manual → engine, esse binding precisa existir.

### Narrativa da POC

Esta change completa a história incremental da POC:

1. **Fase 1**: Projeto existente com serializer manual mínimo (XBuilder + DpsDocument)
2. **Fase 2**: Expansão manual — novos campos, blocos (IBSCBS, tributos, deduções), mapper, testes, golden masters
3. **Fase 3**: Consolidação do manual como baseline — cobertura XSD 74%/92%, validação contra schema
4. **Fase 4**: Engine de análise — XsdSchemaAnalyzer, SchemaModel, ProviderRuleResolver, providers por pasta
5. **Fase 5**: Serializer runtime — `SchemaBasedXmlSerializer` gera XML a partir de dados + schema
6. **Fase 6** (esta change): Binding — conecta o domínio real `DpsDocument` ao serializer runtime, provando que a evolução manual → engine é viável sem reescrita

## What Changes

- Criar `ServiceInvoiceSchemaDataBinder` que converte `DpsDocument` em `Dictionary<string, object?>` com paths compatíveis com o `SchemaModel`.
- O binder usa `ProviderRuleResolver` para aplicar transformações específicas do provider (enums, formatting, condicionais).
- O binder resolve a lógica de mapeamento: propriedades C# do domínio → paths do schema (ex: `Provider.Cnpj` → `infDPS.prest.CNPJ`).
- O mapeamento é configurável por provider via seção `bindings` no `base-rules.json`, permitindo que diferentes providers mapeiem os mesmos dados do domínio para paths diferentes no schema.
- Criar `SchemaSerializationPipeline` que orquestra: binder → serializer → validação XSD.
- Criar testes do fluxo completo: `DpsDocument` → binder → serializer → XML → validação XSD.
- Comparar output do pipeline com golden master do baseline manual.
- Documentar a narrativa da evolução incremental da POC.

## Capabilities

### New Capabilities

_(nenhuma nova spec)_

### Modified Capabilities

- `nfse-runtime-xml-serializer`: Binding entre domínio e serializer runtime. Pipeline completo.

## Impact

- **SchemaEngine**: Novo `ServiceInvoiceSchemaDataBinder`, `SchemaSerializationPipeline`.
- **Providers/rules**: Expandir `base-rules.json` do nacional com seção `bindings` (mapeamento domínio → schema).
- **Tests**: Testes do binder + pipeline end-to-end + comparação com golden master.
- **Docs**: Narrativa da evolução da POC.
- **Zero alteração** no serializer manual, endpoint ou domínio existente.
