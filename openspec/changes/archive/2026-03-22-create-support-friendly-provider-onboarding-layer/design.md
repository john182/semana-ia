## Context

O `XsdSchemaAnalyzer` já extrai do schema: complexTypes com nome e namespace, elementos com tipo/obrigatoriedade/choice/sequence, restrictions (pattern, length, enumerations), inline types e namespace map. O `ProviderOnboardingValidator` já valida 5 checks e classifica gaps. O que falta é usar o output do analyzer para gerar automaticamente uma configuração inicial de provider.

Hoje o suporte precisa criar manualmente um `base-rules.json` com bindings como `"infDPS.tpAmb": "Environment"` — isso exige conhecimento do schema E do modelo de domínio. O objetivo é que a engine gere esse arquivo automaticamente a partir do schema, e o suporte apenas revise e ajuste.

## Goals / Non-Goals

**Goals:**
- Engine gera `base-rules.json` automaticamente a partir do schema
- Bindings são inferidos por correspondência de nome entre schema elements e DpsDocument properties
- Formatting rules são inferidas a partir de XSD restrictions (padLeft de maxLength, pattern)
- WrapperBindings e bindingPathPrefix são inferidos quando o schema tem envelope
- Sample DpsDocument mínimo é gerado por provider para validação
- Status operacional por provider: SupportReady / SupportConfigOnly / NeedsEngineering
- Paulistana e Simpliss configurados com config gerada
- Diagnóstico acionável no relatório de onboarding

**Non-Goals:**
- Inferência perfeita de 100% dos bindings (inferência é best-effort, suporte ajusta o resto)
- Geração de conditionals complexas (ficam como gaps para dev)
- Substituição do baseline manual nacional
- Auto-geração de enums (mapeamento domínio→valor numérico é decisão de negócio)

## Decisions

### 1. ProviderConfigGenerator usando SchemaModel + reflexão do DpsDocument

**Decisão:** O gerador analisa o schema com `XsdSchemaAnalyzer`, depois faz correspondência de nomes entre elementos do schema e propriedades do `DpsDocument` usando reflexão. Quando há match (case-insensitive), gera um binding automaticamente.

**Estratégia de matching:**
- Match direto por nome: `tpAmb` → `DpsDocument` não tem `tpAmb` → sem binding (precisa de mapeamento manual)
- Match por propriedade conhecida: usar um dicionário de mapeamentos comuns (ex: `CNPJ` → `Provider.Cnpj`, `CPF` → `Provider.Cpf`)
- Fields obrigatórios sem match → marcados como `TODO: manual mapping required`

**Alternativa considerada:** Inferir puramente por estrutura hierárquica.

**Racional:** A correspondência por nome + dicionário de campos comuns cobre a maioria dos campos fiscais (CNPJ, CPF, IM, valores, datas). O dicionário é estático e mantido no código.

### 2. Inferência de wrapperBindings e bindingPathPrefix

**Decisão:** Quando o root element tem inline type com um único filho complexo que por sua vez contém os elementos de dados (padrão envelope como ISSNet), o gerador infere automaticamente o `bindingPathPrefix` e gera `wrapperBindings` para os campos obrigatórios do wrapper.

**Heurística:** Se o caminho do schema root até o primeiro `TCDPS` ou tipo equivalente tem profundidade > 1, é um envelope.

### 3. Inferência de formatting a partir de restrictions

**Decisão:** Quando um `SchemaElement` tem `Restriction` com `MaxLength`, gerar regra `maxLength`. Quando tem `Pattern` com padrão numérico fixo (ex: `[0-9]{14}`), gerar `padLeft` + `digitsOnly`. Quando tem `MinLength` = `MaxLength`, gerar `padLeft` com o comprimento fixo.

### 4. OperationalStatus como enum no OnboardingReport

**Decisão:** Adicionar enum `OperationalStatus { SupportReady, SupportConfigOnly, NeedsEngineering }` ao `OnboardingReport`. A classificação é calculada automaticamente:
- `SupportReady`: todos os checks passam (schema + bindings + runtime + XSD)
- `SupportConfigOnly`: schema e análise ok, falta apenas configuração (bindings, formatting)
- `NeedsEngineering`: falha em schema loading, analysis, ou gaps que o suporte não pode resolver

### 5. ProviderSampleDocumentGenerator

**Decisão:** Gera um `DpsDocument` mínimo usando os bindings do provider para determinar quais campos são necessários, preenchendo com valores fictícios válidos (CNPJ dummy, data atual, valores mínimos). Substitui o `CreateMinimalSampleDocument` hardcoded atual.

### 6. Config gerada como ponto de partida, não como verdade final

**Decisão:** A config gerada é um draft que o suporte revisa. Fields com `TODO:` indicam o que precisa de ajuste manual. O gerador NUNCA sobrescreve um `base-rules.json` existente — gera em `providers/{name}/generated/suggested-rules.json`.

### 7. Endpoint de onboarding self-service via API

**Decisão:** Criar `POST /api/v1/providers/onboard` que recebe nome do provider, XSD files (multipart upload) e lista de códigos de município. O endpoint orquestra todo o fluxo: criação de pastas → salvamento de XSDs → análise do schema → geração de config → validação → retorno do OnboardingReport com status operacional e diagnóstico.

**Complementado por:**
- `GET /api/v1/providers` — lista providers com status operacional
- `GET /api/v1/providers/{name}/status` — diagnóstico completo de um provider

**Arquitetura Onion:**
- Domain: `IProviderOnboardingService` (interface do contrato)
- Application: `OnboardProviderUseCase` (orquestração)
- Infrastructure: `ProviderOnboardingService` (implementação que usa a engine)
- API: `ProviderOnboardingController` (endpoints HTTP)

**Alternativa considerada:** CLI tool para onboarding.

**Racional:** O suporte já usa a API para emissão de NFS-e. Manter o onboarding na mesma API é mais natural e não exige acesso ao servidor/filesystem. O upload de XSDs via multipart é padrão HTTP e funciona em qualquer client (Postman, curl, frontend futuro).

## Risks / Trade-offs

- **[Risco] Matching de nomes pode gerar bindings errados** → Mitigation: bindings gerados vão para `suggested-rules.json`, não para `base-rules.json`. O suporte revisa antes de usar.

- **[Risco] Paulistana tem schema muito diferente** → Mitigation: se a geração não produzir config válida, o status operacional será `NeedsEngineering` e o relatório explicará o motivo.

- **[Trade-off] Dicionário de campos comuns é hardcoded** → Mais simples e previsível do que inferência semântica. Cobre 80% dos campos fiscais brasileiros. Extensível adicionando entradas.
