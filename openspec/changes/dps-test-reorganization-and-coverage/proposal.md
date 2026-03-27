## Why

Os testes de DPS estão desorganizados: testes da engine (SchemaEngine) e testes de providers manuais (Manual/Nacional) coexistem sem separação clara de responsabilidade. Nem todos os providers possuem cobertura equivalente — nacional tem cobertura ampla, mas providers como abrasf, paulistana, simpliss e webiss têm cobertura limitada ou parcial. As variações de preenchimento do `DpsDocument` já estão mapeadas no `DpsDocumentTestFixture.FillingVariations()` (25 cenários), mas não são exercitadas de forma consistente por todos os providers. Além disso, testes de integração (E2E) estão em arquivos monolíticos sem separação engine/provider.

A reorganização é necessária agora porque o projeto está em fase de expansão de providers e a base de testes atual não escala — novos providers não têm um caminho claro para serem adicionados com cobertura completa.

## What Changes

- **Reorganizar estrutura de pastas** dos testes unitários para separar explicitamente `Engine/` e `Providers/<ProviderName>/`
- **Reorganizar testes de integração** seguindo o mesmo critério de separação engine vs providers
- **Criar testes de DPS por provider** para os providers sem cobertura ou com cobertura insuficiente (abrasf, paulistana, simpliss, webiss)
- **Padronizar validação por schema**: todos os testes de provider devem usar `ShouldBeValidAgainstDpsSchema` (nacional) ou `ShouldBeValidAgainstProviderSchema` (ABRASF-based) obrigatoriamente
- **Ampliar cobertura das variações de `DpsDocument`**: exercitar `FillingVariations()` para cada provider, além de cenários provider-specific
- **Mover helpers/builders compartilhados** para local acessível por ambos os projetos de teste (unit e integration)
- **Padronizar nomenclatura** de classes, métodos e arquivos de teste

## Capabilities

### New Capabilities
- `dps-test-structure-reorganization`: Reorganização da estrutura de pastas e namespaces dos testes unitários e de integração, separando engine de providers
- `dps-provider-test-coverage`: Criação de suítes de teste por provider com validação obrigatória de schema e cobertura das variações de `DpsDocument`

### Modified Capabilities
- `nfse-provider-test-coverage`: Ampliação dos requisitos existentes para cobrir todos os 7 providers (não apenas ISSNet e GISSOnline) e exigir reorganização estrutural

## Impact

- **Testes unitários**: `tests/SemanaIA.ServiceInvoice.UnitTests/` — reorganização de pastas `Manual/`, `SchemaEngine/`, criação de `Providers/<name>/`
- **Testes de integração**: `tests/SemanaIA.ServiceInvoice.IntegrationsTests/` — separação de testes E2E por provider
- **Helpers compartilhados**: `DpsDocumentBuilder`, `DpsDocumentTestFixture`, `XsdValidationHelper` — podem precisar mover para projeto compartilhado ou manter referência cruzada
- **Nenhuma alteração em código de produção** — apenas testes e organização
- **Providers afetados**: nacional, abrasf, gissonline, issnet, paulistana, simpliss, webiss
