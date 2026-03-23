## ADDED Requirements

### Requirement: Validation diagnostic enrichment with pending fields

O `ValidationDiagnosticEnricher` MUST analisar erros de serialização e validação XSD e produzir um relatório enriquecido contendo: campos pendentes de mapeamento, sugestões de binding inferidas do `CommonFieldMappingDictionary`, e contexto de domínio para cada campo não mapeado.

#### Scenario: Pending fields listed with suggestions
- **WHEN** a serialização falha com `InputError` para campos obrigatórios sem valor
- **THEN** o diagnóstico enriquecido lista cada campo pendente com o path completo no schema e, quando disponível, uma sugestão de mapeamento baseada no `CommonFieldMappingDictionary`

#### Scenario: Suggestion based on field name similarity
- **WHEN** um campo pendente tem nome similar a uma entrada do `CommonFieldMappingDictionary` (e.g., "NumeroRps" → "Numero" → "Number")
- **THEN** o diagnóstico inclui sugestão de binding com score de confiança (exact, partial, none)

#### Scenario: No suggestion for unknown field
- **WHEN** um campo pendente não tem correspondência no dicionário nem similaridade suficiente
- **THEN** o diagnóstico lista o campo como "manual mapping required" sem sugestão falsa

### Requirement: Enriched diagnostic in onboarding report

O `ProviderOnboardingValidator` MUST integrar o `ValidationDiagnosticEnricher` no check `RuntimeProducible`, incluindo campos pendentes e sugestões no `OnboardingCheck.Details` quando a serialização falha.

#### Scenario: Onboarding report shows pending fields with suggestions
- **WHEN** o validator executa `RuntimeProducible` e há campos sem mapeamento
- **THEN** o `OnboardingCheck.Details` inclui lista de campos pendentes com sugestões formatadas

#### Scenario: Onboarding report with all fields mapped
- **WHEN** o validator executa `RuntimeProducible` e todos os campos estão mapeados
- **THEN** o diagnóstico enriquecido não é incluído (comportamento atual preservado)

### Requirement: Enriched diagnostic in API validation response

O endpoint de validação MUST retornar campos pendentes e sugestões no `ValidationResponse` quando a validação falha, permitindo que o operador saiba exatamente quais campos configurar.

#### Scenario: API returns pending fields in validation failure
- **WHEN** o endpoint `/api/providers/{name}/validate` retorna falha
- **THEN** o response inclui seção `pendingFields` com path, sugestão e confiança para cada campo pendente

#### Scenario: API returns clean response on success
- **WHEN** o endpoint `/api/providers/{name}/validate` retorna sucesso
- **THEN** o response não inclui seção `pendingFields`
