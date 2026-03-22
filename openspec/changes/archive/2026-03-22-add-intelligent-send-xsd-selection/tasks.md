## 1. SendXsdSelector

- [x] 1.1 Criar `SendXsdSelector` no projeto XmlGeneration/SchemaEngine com heurísticas de nome para identificar XSD de envio (enviar*, servico_enviar*, DPS*, Pedido*, schema_nfse*, nfse*)
- [x] 1.2 Implementar suporte a override via `ProviderProfile.PrimaryXsdFile`
- [x] 1.3 Implementar diagnóstico de seleção ambígua com lista de candidatos
- [x] 1.4 Adicionar propriedade `PrimaryXsdFile` (string?) ao `ProviderProfile` com `[JsonPropertyName("primaryXsdFile")]`

## 2. Ajustar XsdSchemaAnalyzer

- [x] 2.1 Modificar `LoadSchemaSet()` para carregar apenas o XSD recebido (não todos os `*.xsd` do diretório), deixando o `XmlSchemaSet` resolver includes/imports automaticamente
- [x] 2.2 Garantir que includes/imports relativos continuam funcionando (Nacional usa `xs:include`, GISSOnline usa `xsd:import`)
- [x] 2.3 Garantir zero regressão para os 6 providers base (Nacional, ABRASF, GISSOnline, ISSNet, Paulistana, Simpliss)

## 3. Integrar seletor nos consumidores

- [x] 3.1 Ajustar `SchemaSerializationPipeline` para usar `SendXsdSelector` ao determinar qual XSD analisar
- [x] 3.2 Ajustar `ProviderConfigGenerator` para usar `SendXsdSelector`
- [x] 3.3 Ajustar `ProviderOnboardingValidator` para reportar qual XSD foi selecionado e se houve ambiguidade
- [x] 3.4 Ajustar `ProviderOnboardingService` (Infrastructure) para propagar seleção correta no onboarding

## 4. Testes

- [x] 4.1 Criar testes unitários para `SendXsdSelector`: seleção por padrão de nome, override, ambiguidade, diretório com único XSD
- [x] 4.2 Criar testes para `XsdSchemaAnalyzer` com loading seletivo: provider com múltiplos XSDs no diretório não gera conflito
- [x] 4.3 Garantir que testes existentes de todos os providers continuam passando (zero regressão)
- [x] 4.4 Re-executar load test com providers externos para validar que providers antes falhando agora passam

## 5. Relatório e documentação

- [x] 5.1 Atualizar relatório sumarizado por provider com XSD selecionado
- [x] 5.2 Documentar gaps remanescentes para providers onde a seleção ainda é ambígua
