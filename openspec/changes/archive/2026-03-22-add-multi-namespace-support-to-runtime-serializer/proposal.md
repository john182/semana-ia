## Why

O runtime serializer atual assume um único namespace por schema, mas providers como GISSOnline definem tipos em um namespace (`tipos-v2_04.xsd`) e o envelope em outro (`enviar-lote-rps-envio-v2_04.xsd`). Isso faz com que elementos sejam emitidos no namespace errado, gerando XML inválido contra o XSD. Com o suporte a anonymous inline types já concluído (ABRASF avançou para PASS), multi-namespace é o próximo bloqueio estrutural para expandir o runtime a mais providers.

## What Changes

- Estender `SchemaModel` para preservar o namespace de origem de cada `SchemaComplexType` e `SchemaElement`
- Ajustar `XsdSchemaAnalyzer` para capturar e propagar namespace por tipo/elemento durante a análise multi-XSD
- Ajustar `SchemaBasedXmlSerializer` para emitir cada elemento no namespace correto (não apenas o `TargetNamespace` do root)
- Ajustar `SchemaDocument` para expor mapeamento de namespaces disponíveis no schema
- Expandir testes de validação XSD para cobrir cenários multi-namespace
- Atualizar relatório sumarizado por provider com status de multi-namespace

## Capabilities

### New Capabilities

_Nenhuma capability nova — a mudança estende capabilities existentes._

### Modified Capabilities

- `nfse-xsd-generation-engine`: Adicionar suporte a múltiplos namespaces na análise de schema e no modelo canônico (SchemaDocument, SchemaComplexType, SchemaElement devem preservar namespace de origem)
- `nfse-runtime-xml-serializer`: Ajustar serialização runtime para emitir elementos no namespace correto conforme capturado do XSD, em vez de usar namespace único do root

## Impact

- **SchemaModel.cs**: Novos campos de namespace em `SchemaComplexType` e/ou `SchemaElement`
- **XsdSchemaAnalyzer.cs**: Captura de `QualifiedName.Namespace` durante análise de tipos globais e elementos
- **SchemaBasedXmlSerializer.cs**: Resolução de namespace por elemento ao emitir `XElement`
- **SchemaDocument**: Possível exposição de namespace map para lookup durante serialização
- **AllProvidersXsdValidationSummaryTests.cs**: Atualização de expectativas para GISSOnline (de FAIL para PASS ou progresso)
- **runtime-xsd-validation-summary.md**: Atualização do relatório
- **Providers estáveis**: Nacional e ABRASF devem continuar PASS (regressão zero)
- **Providers beneficiados**: GISSOnline é o principal candidato a avançar; ISSNet e Paulistana podem se beneficiar parcialmente
