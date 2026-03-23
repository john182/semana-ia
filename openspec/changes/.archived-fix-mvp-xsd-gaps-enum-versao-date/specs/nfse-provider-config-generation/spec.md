## MODIFIED Requirements

### Requirement: Auto-generate provider configuration from schema

O auto-gen MUST gerar rules que produzem valores numéricos para campos enum do domínio (`TaxRegime`, `SpecialTaxRegime`, `RetentionType`, `TaxationType`). O mapeamento no `CommonFieldMappingDictionary` MUST produzir binding que, ao resolver via `TypedRuleResolver`, emite o inteiro (não o nome do enum).

#### Scenario: opSimpNac emits integer
- **WHEN** o campo `opSimpNac` é resolvido para `Provider.TaxRegime = SimplesNacional`
- **THEN** o XML emite `<opSimpNac>3</opSimpNac>` (inteiro), não `<opSimpNac>SimplesNacional</opSimpNac>`

#### Scenario: DataEmissao uses date format for ABRASF
- **WHEN** o campo `DataEmissao` é mapeado no dicionário
- **THEN** o formato é `yyyy-MM-dd` (xs:date), não `yyyy-MM-ddTHH:mm:sszzz`
