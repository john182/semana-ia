# Tasks: runtime-schema-based-xml-serializer

## 1. Modelos de resultado e erro

- [x] 1.1 Criar `SerializationErrorKind` enum: InputError, RuleError, SchemaError, InternalError
- [x] 1.2 Criar `SerializationError` record: Kind, Field, Message, Details
- [x] 1.3 Criar `SerializationResult` record: Xml (string?), IsValid (bool), Errors (List<SerializationError>), ValidationErrors (List<string>)

## 2. SchemaBasedXmlSerializer — core

- [x] 2.1 Criar `SchemaBasedXmlSerializer` com método `Serialize(SchemaDocument schema, Dictionary<string, object?> data, IProviderRuleResolver resolver, string rootElementName) → SerializationResult`
- [x] 2.2 Implementar travessia da árvore de complexTypes via recursão: para cada complexType referenciado por um elemento, resolver seus sub-elementos
- [x] 2.3 Implementar emissão de sequence: emitir elementos na ordem definida pelo schema
- [x] 2.4 Implementar resolução de choice: verificar quais elementos do choice têm valor no dicionário, emitir o primeiro encontrado. Se nenhum e choice obrigatório → InputError
- [x] 2.5 Implementar emissão de elementos simples: buscar valor no dicionário por path notation (ex: `infDPS.tpAmb`), aplicar formatação do resolver se existir
- [x] 2.6 Implementar tratamento de obrigatoriedade: se elemento required sem valor e sem default no resolver → InputError
- [x] 2.7 Implementar aplicação de defaults do resolver: se elemento sem valor mas com default → usar default
- [x] 2.8 Implementar aplicação de formatação: se elemento com regra de formatting no resolver → aplicar padLeft, digitsOnly, etc.
- [x] 2.9 Implementar emissão de atributos: se o complexType raiz tem atributos (ex: `versao`, `xmlns`, `Id`) → emitir via XBuilder
- [x] 2.10 Gerar `SerializationResult` com XML string + erros coletados

## 3. Validação XSD integrada

- [x] 3.1 Criar método `ValidateAgainstXsd(string xml, string providerXsdDir) → List<string>` no serializer ou como utility
- [x] 3.2 Integrar validação no `Serialize`: após geração, validar XML contra XSDs do provider. Popular `ValidationErrors` no resultado
- [x] 3.3 Se XML é null (erros de input impediram geração) → não validar, retornar resultado com erros apenas

## 4. Centralizar validação XSD em extension method

- [x] 4.1 Criar `ProviderXsdValidationExtensions` como extension method Shouldly: `ShouldBeValidAgainstProviderSchema(this string xml, string providerName)`
- [x] 4.2 Carregar XSDs de `providers/{provider}/xsd/` automaticamente, com DtdProcessing.Parse para xmldsig
- [x] 4.3 Suportar múltiplos namespaces por provider (ex: GISSOnline com tipos + envio)
- [x] 4.4 Refatorar testes existentes para usar o extension method centralizado (remover helpers duplicados)

## 5. ProviderOnboardingValidator

- [x] 5.1 Criar `ProviderOnboardingValidator` com método `Validate(string providerName, string providersBaseDir) → OnboardingReport`
- [x] 5.2 O validator analisa o schema, gera dicionário de dados com defaults/dummy values para todos os elementos obrigatórios
- [x] 5.3 Chama `SchemaBasedXmlSerializer.Serialize` com esses dados
- [x] 5.4 Valida XML gerado contra XSD
- [x] 5.5 Retorna `OnboardingReport` com: provider, status por complexType, erros, sugestões de campos faltantes no base-rules.json

## 6. Testes — serializer runtime

- [x] 6.1 Criar `SchemaBasedXmlSerializerTests`
- [x] 6.2 Teste: Given_NacionalMinimalData_Should_ProduceValidXml (dados mínimos → XML válido contra XSD nacional)
- [x] 6.3 Teste: Given_ChoiceData_Should_EmitOnlySelectedElement (CNPJ presente → emite CNPJ, omite CPF/NIF/cNaoNIF)
- [x] 6.4 Teste: Given_OptionalElementAbsent_Should_OmitFromXml
- [x] 6.5 Teste: Given_RequiredElementMissing_Should_ReturnInputError
- [x] 6.6 Teste: Given_FormattingRule_Should_ApplyToValue (cTribNac padLeft 6)
- [x] 6.7 Teste: Given_DefaultFromResolver_Should_UseWhenValueAbsent (tpEmit default 1)

## 7. Testes — validação XSD por provider

- [x] 7.1 Teste: Given_NacionalRuntimeXml_Should_PassXsdValidation (usando extension method centralizado)
- [x] 7.2 Teste: Given_AbrasfRuntimeXml_Should_PassXsdValidation
- [x] 7.3 Teste: Given_GissonlineRuntimeXml_Should_PassXsdValidation
- [x] 7.4 Teste: Given_InvalidRuntimeXml_Should_ReportValidationErrors

## 8. Testes — onboarding validator

- [x] 8.1 Criar `ProviderOnboardingValidatorTests`
- [x] 8.2 Teste: Given_NacionalProvider_Should_ProduceOnboardingReport
- [x] 8.3 Teste: Given_AbrasfProvider_Should_ProduceOnboardingReport
- [x] 8.4 Teste: Given_GissonlineProvider_Should_ProduceOnboardingReport

## 9. Comparação com baseline manual

- [x] 9.1 Gerar XML do provider nacional via serializer runtime com dados equivalentes ao golden master mínimo
- [x] 9.2 Comparar output com `tests/.../Snapshots/minimal-dps.xml` e documentar divergências

## 10. Build e validação

- [x] 10.1 `dotnet build` sem erros
- [x] 10.2 `dotnet test` com todos os testes passando
