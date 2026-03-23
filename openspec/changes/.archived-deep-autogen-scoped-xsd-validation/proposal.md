## Why

A auto-geração de regras, a validação XSD e o diagnóstico de falhas possuem gaps que limitam o onboarding prático de providers ABRASF e variantes. Além disso, não existe uma bateria de testes E2E que cubra o ciclo completo de vida de um provider (discovery → auto-gen rules → validate → serialize → XSD pass) para **todos** os providers da pasta `providers/`. Isso significa que ao adicionar um novo provider, não há garantia automatizada de que ele vai funcionar end-to-end.

1. **Tree walk superficial**: `ProviderConfigGenerator.DetectEnvelopePattern` desce apenas 2 níveis (root → envelope child → data container) mas schemas ABRASF têm árvore mais profunda (`EnviarLoteRpsEnvio > LoteRps > ListaRps > Rps > InfDeclaracaoPrestacaoServico`). Campos dentro de `InfRps` e sub-tipos não são alcançados, resultando em regras incompletas e muitos TODOs manuais.

2. **Validação XSD sem escopo**: `ValidateXmlAgainstXsd` carrega **todos** os `*.xsd` do diretório (incluindo response, consulta, cancelamento), causando conflitos de tipos duplicados e falsos positivos. Deveria validar apenas contra o XSD de envio e seus tipos auxiliares referenciados.

3. **Diagnóstico opaco em falha de validação**: Quando `validate` falha, os erros são mensagens brutas do .NET sem contexto de domínio. Não há indicação de quais campos estão pendentes de mapeamento, nem sugestões de binding baseadas no `CommonFieldMappingDictionary`.

4. **Ausência de testes E2E do ciclo completo por provider**: Os testes existentes (`AllProvidersXsdValidationSummaryTests`) cobrem serialização+XSD por provider individualmente, e os `ProviderFullLoadTests` cobrem providers da pasta `data/` via API. Mas nenhum teste valida o ciclo completo **sem dependência de API/MongoDB**: discovery de todos os providers da pasta `providers/` → auto-geração de rules → validação → serialização → XSD pass. Sem isso, ativar um novo provider é um processo frágil e manual.

## What Changes

- **Deep tree walk na auto-geração**: `DetectEnvelopePattern` e `WalkSchemaTree` passam a resolver a árvore completa do envelope ABRASF, identificando o nó de dados real (e.g., `InfDeclaracaoPrestacaoServico`) independentemente da profundidade de nesting.
- **Validação XSD scoped ao envio**: `ValidateXmlAgainstXsd` passa a usar `SendXsdSelector` para identificar o XSD principal de envio e carrega apenas esse XSD (com includes/imports resolvidos automaticamente pelo `XmlSchemaSet`), excluindo schemas de response/consulta.
- **Diagnóstico enriquecido com sugestões**: Quando a validação ou serialização falha, o relatório inclui campos pendentes com sugestões de mapeamento inferidas do `CommonFieldMappingDictionary` e da similaridade de nome com campos do domínio.
- **Bateria E2E para todos os providers**: Suite de testes unitários (sem API/MongoDB) que, para cada provider em `providers/`, executa o ciclo completo: discovery → XSD selection → schema analysis → auto-gen rules → data binding com sample document → serialização → validação XSD. Cada provider produz um resultado classificado e o relatório final garante que nenhum provider regride ao adicionar um novo.

## Capabilities

### New Capabilities
- `enriched-validation-diagnostics`: Diagnóstico de falha de validação com campos pendentes, sugestões de binding e contexto de domínio para orientar o operador.
- `provider-lifecycle-e2e-tests`: Bateria de testes E2E do ciclo completo de vida do provider, cobrindo todos os providers da pasta `providers/` sem dependência de API ou banco de dados, garantindo que um novo provider pode ser ativado com confiança.

### Modified Capabilities
- `nfse-provider-config-generation`: Deep tree walk no envelope ABRASF para gerar bindings completos até o nível de `InfRps` e sub-tipos.
- `nfse-runtime-xml-serializer`: Validação XSD scoped ao root element de envio, sem carregar schemas de response/consulta/cancelamento.

## Impact

- **ProviderConfigGenerator.cs**: Refatoração de `DetectEnvelopePattern` e `WalkSchemaTree` para suportar profundidade arbitrária.
- **SchemaBasedXmlSerializer.cs**: `ValidateXmlAgainstXsd` refatorado para receber XSD de envio selecionado em vez de carregar `*.xsd` do diretório.
- **ProviderOnboardingValidator.cs**: Integração com diagnóstico enriquecido no report.
- **Novo componente**: `ValidationDiagnosticEnricher` para gerar sugestões de campos pendentes.
- **Nova suite de testes**: `ProviderLifecycleEndToEndTests` — testes unitários parametrizados por provider que validam o ciclo completo discovery → auto-gen → serialize → XSD, usando diretamente as classes do engine (sem HTTP/MongoDB).
- **Testes existentes**: Ajuste em testes de validação XSD que assumiam schema set completo. Novos testes para deep walk e sugestões.
- **Providers afetados**: Todos os 7 providers (nacional, abrasf, gissonline, issnet, paulistana, simpliss, webiss) cobertos pela suite E2E.
