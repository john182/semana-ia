## 1. Domain — Modelo de Regras Tipado

- [x] 1.1 Criar enum `RuleType` (Binding, Default, EnumMapping, ConditionalEmission, Choice, Formatting)
- [x] 1.2 Criar enum `ComparisonOperator` (Equals, NotEquals, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual, IsNull, HasValue, Contains, In)
- [x] 1.3 Criar enum `LogicalOperator` (And, Or)
- [x] 1.4 Criar enum `RuleAction` (Emit, Skip, UseDefault, MapEnum, Format)
- [x] 1.5 Criar modelo `RuleCondition` (field, operator, value para leaf; logicalOperator + conditions para composite)
- [x] 1.6 Criar modelo `ProviderRule` com type discriminator e campos específicos por tipo (target, source, sourceType, constantValue, format, fallbackValue, mappings, defaultMapping, condition, action, choiceField, options, digitsOnly, padLeft, padChar, maxLength, trim)
- [x] 1.7 Criar catálogo `RuleSourceFieldCatalog` gerado por reflexão de `DpsDocument` com nome, tipo, path e descrição
- [x] 1.8 Testes unitários para `RuleCondition` (avaliação AND/OR/nested, leaf com cada operador: Equals, NotEquals, GreaterThan, IsNull, HasValue, In)

## 2. Validação Semântica

- [x] 2.1 Criar `ProviderRuleValidator` que valida: source fields existem no catálogo, target fields existem no schema XSD do provider, operators válidos para o tipo de campo, enum mappings referenciam valores válidos do enum do domínio, condições compostas estruturalmente válidas
- [x] 2.2 Integrar `ProviderRuleValidator` no fluxo de save/update do `ProviderManagementService` (provider com rule inválida fica Blocked com motivo claro)
- [x] 2.3 Testes unitários para validação: source inválido, target inválido, operator inválido, condição malformada (sem field), enum mapping com chave inválida, condição referenciando campo inexistente, regra sem target, regra com type desconhecido

## 3. Engine — Interpretação da DSL

- [x] 3.1 Criar `TypedRuleResolver` implementando `IProviderRuleResolver` que interpreta `List<ProviderRule>`
- [x] 3.2 Implementar resolução de binding: source do domínio → valor formatado no target
- [x] 3.3 Implementar resolução de binding constante: sourceType=constant → constantValue no target
- [x] 3.4 Implementar resolução de default: source do domínio com fallback quando null
- [x] 3.5 Implementar resolução de enum mapping: valor do domínio → lookup no mappings → valor do provider, com defaultMapping como fallback
- [x] 3.6 Implementar avaliação de conditional emission: avaliar condição composta (AND/OR recursivo) → emit ou skip o target
- [x] 3.7 Implementar resolução de choice: campo discriminador → selecionar branch → emitir element com source e formatting
- [x] 3.8 Implementar resolução de formatting: digitsOnly, padLeft, padChar, maxLength, trim
- [x] 3.9 Integrar `TypedRuleResolver` no `ServiceInvoiceSchemaDataBinder` e `SchemaBasedXmlSerializer`
- [x] 3.10 Testes unitários para cada tipo de resolução com dados reais do DpsDocument

## 4. Config Generator — Saída Tipada

- [x] 4.1 Atualizar `ProviderConfigGenerator` para gerar `List<ProviderRule>` usando o novo formato tipado
- [x] 4.2 Cada campo mapeado via CommonFieldMappingDictionary vira um ProviderRule tipado (binding, com formatting quando inferido do XSD)
- [x] 4.3 Testes unitários para geração tipada: verificar que cada rule gerada tem type correto, source/target preenchidos, formatting inferido do XSD

## 5. Remoção do Formato Legado

- [x] 5.1 Remover `ProviderProfile.Bindings`, `ProviderProfile.Enums`, `ProviderProfile.Defaults`, `ProviderProfile.Conditionals` e `ProviderProfile.Formatting` — substituídos por `List<ProviderRule>`
- [x] 5.2 Remover `ProviderRuleResolver` legado (substituído pelo `TypedRuleResolver`)
- [x] 5.3 Remover `ConditionalRule` (substituído por `ProviderRule` tipo `conditionalEmission`)
- [x] 5.4 Remover `ManagedProvider.RulesJson` — substituído por `List<ProviderRule> Rules`
- [x] 5.5 Atualizar `ServiceInvoiceSchemaDataBinder` para usar apenas `TypedRuleResolver`
- [x] 5.6 Atualizar `ProviderOnboardingService` e `EngineProviderValidator` para usar typed rules
- [x] 5.7 Atualizar testes existentes que dependiam do formato legado
- [x] 5.8 Remover `base-rules.json` dos providers filesystem — substituir por `rules.json` no novo formato

## 6. API — Catálogos e Documentação

- [x] 6.1 Criar `RuleCatalogController` com endpoints: `GET /api/v1/rules/sources`, `GET /api/v1/rules/targets/{providerId}`, `GET /api/v1/rules/operators`, `GET /api/v1/rules/actions`, `GET /api/v1/rules/types`
- [x] 6.2 Criar DTOs de response para cada catálogo com nome, tipo, descrição em português, e exemplos de uso
- [x] 6.3 Endpoint `/sources` retorna campos do DpsDocument com path hierárquico, tipo C# e descrição funcional
- [x] 6.4 Endpoint `/targets/{providerId}` retorna paths XML extraídos do XSD do provider
- [x] 6.5 Endpoint `/types` retorna cada tipo de regra com descrição, campos obrigatórios e exemplo JSON completo de preenchimento
- [x] 6.6 Atualizar DTOs de create/update provider para aceitar `List<ProviderRule>` no campo `rules`
- [x] 6.7 Adicionar exemplos Swagger detalhados para cada tipo de regra no endpoint de create/update provider
- [ ] 6.8 Testes de integração para cada endpoint de catálogo (sources retorna campos, targets retorna paths do schema, types retorna exemplos)

## 7. Persistência

- [x] 7.1 Atualizar `ProviderDocument` e `MongoProviderRepository` para persistir `List<ProviderRule>` no campo `rules`
- [x] 7.2 Atualizar `ManagedProvider` para suportar `List<ProviderRule>` como campo de domínio (substituindo `RulesJson`)
- [ ] 7.3 Testes de integração para persistência: salvar provider com typed rules, recuperar, verificar que rules estão intactas

## 8. Testes de Integração e Validação XSD

- [x] 8.1 Teste de integração: configurar provider nacional com typed rules completas → gerar XML mínimo → validar contra DPS_v1.01.xsd
- [ ] 8.2 Teste de integração: configurar provider ABRASF com typed rules (binding, enumMapping, choice CPF/CNPJ, formatting) → gerar XML completo → validar contra XSD ABRASF
- [x] 8.3 Teste de integração: conditional emission (tpImunidade) → gerar XML com TaxationType=Immune e verificar presença da tag → gerar XML com TaxationType=WithinCity e verificar ausência da tag → ambos validar contra XSD
- [x] 8.4 Teste de integração: choice CPF/CNPJ com pessoa jurídica → verificar XML contém CNPJ e não CPF → com pessoa física → verificar XML contém CPF e não CNPJ → ambos validar contra XSD
- [x] 8.5 Teste de integração: enum mapping com todos os valores de TaxationType → verificar que cada valor produz o mapeamento correto no XML
- [ ] 8.6 Teste end-to-end via API: POST provider com typed rules via endpoint → validar → ativar → POST nfse/xml → verificar XML gerado usa as typed rules
