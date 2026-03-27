# fix(engine): 6 providers com schema não carregável ou XML null — engine deve rejeitar no onboarding

**Labels:** bug, priority:critical, engine, onboarding
**Milestone:** Production Ready

## Problema

6 providers têm XSD na pasta `data/` mas a engine não consegue:
- Carregar o schema (`PrepareProvider` retorna null) — 1 provider
- Gerar XML a partir do schema carregado (`GenerateXml` retorna null) — 5 providers

O problema principal é que esses providers são aceitos no onboarding sem validação de que a engine realmente consegue processá-los. O usuário cadastra o provider, tudo parece ok, mas na hora de gerar XML a engine retorna null silenciosamente.

## Providers afetados (6)

| Provider | Falha | Provável causa |
|----------|-------|----------------|
| dsfnet | Schema preparation null | `SendXsdSelector` não encontra XSD de envio válido |
| national | XML generation null | Schema Nacional (DPS) usa estrutura diferente dos ABRASF |
| paulistana | XML generation null | Schema proprietário incompatível com pipeline ABRASF |
| bhiss | XML generation null | Schema sem root complex type detectável |
| goiania | XML generation null | Schema sem root complex type detectável |
| betha | XML generation null | Schema incompatível com pipeline atual |

## Causa raiz

1. `XsdSchemaAnalyzer.Analyze()` não detecta root type em schemas com estruturas não-convencionais
2. `SendXsdSelector.Select()` não encontra XSD de envio quando a convenção de nomes difere
3. `ProviderConfigGenerator.GenerateConfig()` falha silenciosamente quando o schema não se encaixa no modelo esperado
4. **O onboarding não valida se a engine consegue gerar XML** antes de marcar o provider como "Ready"

## Correção esperada

### Curto prazo (engine)
1. `XsdSchemaAnalyzer` deve suportar mais variações de root type (schemas com múltiplos root elements, inline types aninhados)
2. `SendXsdSelector` deve ter fallback para schemas com nomes não-convencionais

### Médio prazo (onboarding)
3. O fluxo de onboarding (`ProviderOnboardingValidator`) deve incluir um check de **geração de XML de teste**: criar um DpsDocument mínimo, serializar via engine, e validar contra o XSD. Se falhar → status `NeedsEngineering`, não `Ready`.
4. Esse check já existe parcialmente (`RuntimeProducible`), mas não é executado para todos os providers.

## Testes que validam a correção

- `AllDataProvidersFillingVariationsTests.Given_DpsMinimo_Should_GerarXmlValidoParaProvider(providerName: "national")`
- `AllDataProvidersFillingVariationsTests.Given_DpsMinimo_Should_GerarXmlValidoParaProvider(providerName: "dsfnet")`
- (e paulistana, bhiss, goiania, betha)

## Impacto

6 providers (12.5% do total). Nenhum cenário DPS funciona. Falha silenciosa que o cross-cutting test antigo escondia.

## Arquivos a investigar

- `src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/XsdSchemaAnalyzer.cs`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/SendXsdSelector.cs`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/ProviderConfigGenerator.cs`
- `src/SemanaIA.ServiceInvoice.XmlGeneration/SchemaEngine/ProviderOnboardingValidator.cs`
- `src/SemanaIA.ServiceInvoice.Infrastructure/Validation/EngineProviderValidator.cs`
