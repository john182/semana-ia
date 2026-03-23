## Why

A engine de NFS-e já provou schema analysis, runtime serializer, choice/sequence, anonymous inline types, multi-namespace, multi-level path binding e provider onboarding workflow. Um diagnóstico consolidado dos 48 providers existentes identificou que 22 produzem XML válido e 26 ainda falham por 6 causas raiz objetivas e genéricas. Resolver esses 6 gaps eleva a cobertura para 48/48 PASS sem introduzir hardcode por provider, mantendo a engine genérica e o onboarding automatizado.

## What Changes

- **Root element de envio**: Ajustar `SendXsdSelector` para priorizar corretamente o root element de envio/lote, evitando selecionar `CompNfse` ou outros elementos de resposta/consulta como raiz.
- **Atributo `versao` automático**: Inferir automaticamente o atributo `versao` do schema XSD quando ele é exigido no root element, sem depender de configuração manual no `ProviderProfile`.
- **CommonFieldMappingDictionary expandido**: Adicionar campos obrigatórios recorrentes encontrados nos providers proprietários (`tpRps`, `StatusRps`, `DataEmissaoRps`, `OptanteSN`, `IssRetido`, `ItemListaServico`, entre outros).
- **ProviderConfigGenerator no teste genérico**: Integrar o `ProviderConfigGenerator` no fluxo de teste/validação genérico para que a detecção de envelope e geração de `wrapperBindings`/`bindingPathPrefix` sejam automáticas em todos os providers.
- **XSDs de dependência**: Incluir XSDs de dependência faltantes (tipos compartilhados, xmldsig, etc.) para providers que hoje falham por schema incompleto na carga do `XmlSchemaSet`.
- **Namespace de root inline type**: Ajustar a resolução de namespace quando o root element possui inline complex type em schemas proprietários, garantindo que o namespace correto seja propagado.

## Capabilities

### New Capabilities
- `send-xsd-root-prioritization`: Lógica de priorização inteligente do root element de envio, com exclusão de elementos de resposta/consulta e suporte a padrões proprietários.
- `version-attribute-inference`: Inferência automática do atributo `versao` a partir da definição XSD do root element, sem configuração manual.
- `provider-validation-suite`: Suite de validação consolidada que executa todos os providers e gera resumo sumarizado por provider com status de cada etapa.

### Modified Capabilities
- `nfse-provider-config-generation`: Expandir `CommonFieldMappingDictionary` com campos recorrentes de providers proprietários e integrar detecção de envelope no fluxo de teste genérico.
- `nfse-runtime-xml-serializer`: Ajustar resolução de namespace para root inline type em schemas proprietários.
- `nfse-xsd-generation-engine`: Incluir resolução de XSDs de dependência faltantes na carga do schema set.

## Impact

- **Código**: `SendXsdSelector`, `XsdSchemaAnalyzer`, `SchemaBasedXmlSerializer`, `CommonFieldMappingDictionary`, `ProviderConfigGenerator`, `ProviderOnboardingValidator`, testes de validação.
- **Providers**: Todos os 48 providers na pasta `providers/` serão revalidados; providers que hoje falham devem avançar para PASS.
- **Testes**: Nova suite de validação consolidada com resumo por provider; testes unitários para cada fix individual.
- **Dados**: Alguns providers podem depender de XSDs externos não disponíveis localmente — esses serão explicitamente documentados como pendência OPS/dados.
