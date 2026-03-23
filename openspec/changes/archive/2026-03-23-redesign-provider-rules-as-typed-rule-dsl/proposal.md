## Why

O modelo atual de rules de provider usa strings livres para representar comportamento crítico: bindings como `"IssuedOn | format:yyyy-MM-ddTHH:mm:sszzz"`, condicionais como `"when": "provider.taxRegime == SimplesNacional"`, e enums como dicionários abertos `{ "WithinCity": "1" }`. Isso obriga o suporte a conhecer a semântica interna da engine, oferece zero proteção contra typo, não valida referências a campos inexistentes e não documenta o efeito de cada regra. Para a API de gestão de providers funcionar de verdade para o suporte, a configuração de rules precisa ser tipada, validável e auto-documentada.

## What Changes

- Substituir o modelo aberto de rules (`Dictionary<string, string>` bindings, `Dictionary<string, Dictionary<string, string>>` enums, `ConditionalRule` com string livre `emitWhen`) por uma DSL tipada com contratos fechados
- Criar tipos de regra distintos: `BindingRule`, `DefaultRule`, `EnumMappingRule`, `ConditionalEmissionRule`, `ChoiceRule`, `FormattingRule` (tipada)
- Criar enums/catálogos para: `RuleSourceField` (campos do domínio disponíveis), `RuleTargetField` (campos do schema alvo), `ComparisonOperator` (Equals, NotEquals, GreaterThan, IsNull, HasValue, etc.), `RuleAction` (Emit, Skip, UseDefault, MapEnum, Format)
- Criar modelo de expressão condicional composta: `RuleCondition` com suporte a `AND`/`OR` e operadores controlados
- Criar endpoints de catálogo: `GET /api/v1/rules/sources`, `GET /api/v1/rules/targets/{providerId}`, `GET /api/v1/rules/operators`, `GET /api/v1/rules/actions`
- Atualizar a engine para interpretar a nova DSL em runtime
- Validar semanticamente as rules ao salvar/atualizar provider
- Documentar cada tipo de regra no Swagger com exemplos reais

## Capabilities

### New Capabilities
- `provider-rule-dsl`: DSL tipada para configuração de rules de provider, com tipos de regra distintos, expressões condicionais compostas, catálogos de sources/targets/operators/actions, e validação semântica.
- `provider-rule-catalog-api`: Endpoints de catálogo para o suporte consultar campos de domínio disponíveis, campos de schema alvo por provider, operadores válidos e ações possíveis.

### Modified Capabilities
- `nfse-runtime-xml-serializer`: O serializer passa a interpretar a nova DSL tipada ao invés de strings livres para bindings, condicionais e enum mappings.
- `nfse-provider-config-generation`: O `ProviderConfigGenerator` passa a gerar rules no formato da nova DSL tipada.

## Impact

- **Domínio**: Novos tipos `ProviderRule`, `BindingRule`, `DefaultRule`, `EnumMappingRule`, `ConditionalEmissionRule`, `ChoiceRule`. Enums `RuleSourceField`, `ComparisonOperator`, `RuleAction`. Record `RuleCondition`.
- **XmlGeneration**: `ServiceInvoiceSchemaDataBinder` e `SchemaBasedXmlSerializer` interpretam a nova DSL. `ProviderRuleResolver` adaptado. `ProviderConfigGenerator` gera no novo formato.
- **API**: Novos endpoints de catálogo. DTOs de rules atualizados. Swagger com exemplos de cada tipo de regra.
- **Infraestrutura**: `EngineProviderValidator` valida rules semanticamente. `MongoProviderRepository` persiste o novo formato.
- **Remoção legado**: `ProviderProfile.Bindings/Enums/Defaults/Conditionals/Formatting`, `ProviderRuleResolver`, `ConditionalRule` e `ManagedProvider.RulesJson` são removidos. Providers filesystem migram de `base-rules.json` para `rules.json` no novo formato.
- **Testes**: Testes para cada tipo de regra, validação semântica, interpretação runtime, endpoints de catálogo.
