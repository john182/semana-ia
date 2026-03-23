## Context

A engine de NFS-e schema-driven já suporta schema analysis, runtime serializer baseado em XSD, choice/sequence, anonymous inline types, multi-namespace, multi-level path binding e provider onboarding workflow. Um diagnóstico consolidado de 48 providers identificou que 22 produzem XML válido contra XSD e 26 falham por 6 causas raiz genéricas e objetivas.

Os componentes afetados são:
- `SendXsdSelector` — seleção do XSD de envio
- `XsdSchemaAnalyzer` — análise de schema e namespace
- `SchemaBasedXmlSerializer` — serialização runtime e atributos
- `CommonFieldMappingDictionary` — mapeamento campo→domínio
- `ProviderConfigGenerator` — geração automática de config/bindings
- Diretórios `providers/*/xsd/` — XSDs de dependência

O baseline funcional é o provider `nacional` e o XSD é a fonte de verdade estrutural.

## Goals / Non-Goals

**Goals:**
- Elevar a cobertura de providers com XML válido para 48/48 PASS
- Resolver os 6 gaps identificados de forma genérica (sem hardcode por provider)
- Manter a engine inferindo o máximo antes de depender de configuração manual
- Gerar resumo sumarizado por provider com status de cada etapa
- Documentar explicitamente gaps remanescentes por provider

**Non-Goals:**
- Novas features enterprise não relacionadas aos 6 fixes
- Refatorações amplas fora do necessário
- Suporte a cenários profundos de negócio fora dos 6 itens
- Hardcode específico por provider quando o problema for genérico

## Decisions

### 1. Root element de envio — Expandir `SendXsdSelector` com exclusão de `CompNfse`

**Decisão**: Adicionar `CompNfse`, `compNfse`, `nfse_v` (schemas de resposta/composição) à lista `ExcludePatterns` e adicionar padrões como `enviarLoteRps`, `RecepcionarLoteRps`, `GerarNfse` aos `SendPatterns` com prioridade adequada.

**Alternativa considerada**: Usar `PrimaryXsdFile` override por provider. Descartada porque o problema é genérico — vários providers têm o mesmo padrão de nomes.

**Rationale**: A engine já tem o mecanismo de prioridade/exclusão; basta expandir os padrões para cobrir as convenções dos providers proprietários (ABRASF, GISSOnline, etc.).

### 2. Atributo `versao` — Inferir do XSD automaticamente

**Decisão**: No `XsdSchemaAnalyzer`, ao processar o root element, verificar se o `XmlSchemaComplexType` possui um atributo `versao` declarado. Se existir, extrair o valor fixo/default do schema. Expor como `SchemaDocument.RootVersionAttribute` (nullable). O `ProviderConfigGenerator` usará esse valor automaticamente ao invés do `DefaultVersion = "1.01"`.

**Alternativa considerada**: Ler o valor de `ProviderProfile.Version` sempre. Descartada porque isso exige configuração manual e o XSD já contém a informação.

**Rationale**: O atributo `versao` é declarado no XSD com `fixed` ou `default` value. Inferir evita configuração manual e mantém o XSD como fonte de verdade.

### 3. CommonFieldMappingDictionary — Expandir com campos proprietários recorrentes

**Decisão**: Adicionar mapeamentos para campos recorrentes encontrados nos providers ABRASF/proprietários:
- `tpRps` → `const:1` (RPS)
- `StatusRps` → `const:1` (Normal)
- `DataEmissaoRps` / `DataEmissao` → `IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz`
- `NumeroRps` → `Number`
- `SerieRps` → `Series`
- `OptanteSimplesNacional` → `Provider.TaxRegime`
- `MunicipioIncidencia` → `Service.MunicipalityCode`
- `NaturezaOperacao` → `const:1`
- `RegimeEspecialTributacao` → `Provider.SpecialTaxRegime`
- `ValorIss` → `Values.IssAmount`
- `ValorDeducoes` → `const:0.00`
- `OutrasInformacoes` → `const:`
- `CodigoCnae` → `Service.CnaeCode`

**Rationale**: Esses campos aparecem em múltiplos providers com o mesmo significado semântico. Centralizar no dicionário evita TODOs manuais e permite onboarding automático.

### 4. ProviderConfigGenerator no teste genérico — Integrar detecção automática

**Decisão**: Nos testes de validação genérica (como `ProviderFullLoadTests` e `ExternalProviderXmlGenerationTests`), quando o provider não possui `base-rules.json`, usar o `ProviderConfigGenerator` para gerar config em runtime. Isso inclui detecção de envelope, geração de `wrapperBindings` e `bindingPathPrefix`.

**Alternativa considerada**: Exigir `base-rules.json` para todos os providers. Descartada porque o objetivo é onboarding automático sem configuração manual.

**Rationale**: O `ProviderConfigGenerator` já sabe detectar envelope e gerar bindings. Usá-lo no fluxo de teste genérico permite validar providers que não têm rules manuais.

### 5. XSDs de dependência — Incluir schemas compartilhados faltantes

**Decisão**: Para providers que falham por schema incompleto (tipos compartilhados, xmldsig), incluir os XSDs de dependência no diretório `providers/{provider}/xsd/`. O `XsdSchemaAnalyzer.LoadSchemaSet` já carrega todos os `.xsd` do diretório, então basta colocar os arquivos lá.

**Alternativa considerada**: Resolver imports/includes via URL ou diretório compartilhado. Descartada por complexidade e porque o mecanismo atual (carregar todos os `.xsd` do diretório) já funciona.

**Rationale**: Abordagem pragmática — o schema set precisa de todos os tipos para compilar. Copiar os XSDs de dependência é a forma mais simples e funciona com o mecanismo existente.

### 6. Namespace de root inline type — Ajustar resolução

**Decisão**: No `XsdSchemaAnalyzer`, ao capturar o `rootInlineType`, garantir que o namespace usado seja o `targetNamespace` do schema que declara o root element, não o namespace do tipo. No `SchemaBasedXmlSerializer`, quando o root type é inline, usar o namespace do schema document e não o do tipo.

**Alternativa considerada**: Adicionar um campo `RootNamespace` separado. Descartada por ser redundante — o `SchemaDocument.TargetNamespace` já é a informação correta.

**Rationale**: Schemas proprietários podem declarar root elements com inline types sem namespace explícito no tipo. O namespace correto é sempre o do schema que declara o elemento.

## Risks / Trade-offs

- **[Risk] XSDs de dependência não disponíveis localmente** → Mitigation: Documentar explicitamente como pendência OPS/dados. A engine não falha silenciosamente — reporta erro de compilação de schema.

- **[Risk] Novos padrões de envio em providers futuros** → Mitigation: `SendXsdSelector` permanece extensível via `SendPatterns`/`ExcludePatterns`. O `PrimaryXsdFile` override continua como escape valve.

- **[Risk] Campos proprietários com semântica diferente do mapeamento genérico** → Mitigation: O `base-rules.json` manual sempre tem precedência sobre o dicionário comum. O dicionário é apenas o ponto de partida para onboarding automático.

- **[Risk] Versão do schema pode não ter valor fixo/default no XSD** → Mitigation: Se o atributo `versao` existe mas não tem valor inferível, manter o `DefaultVersion` como fallback. Logar warning no diagnóstico.

- **[Trade-off] Copiar XSDs de dependência vs. resolver imports** → Aceitável: A abordagem de cópia é mais simples, previsível e já funciona com o mecanismo existente. O custo é espaço em disco (negligível).
