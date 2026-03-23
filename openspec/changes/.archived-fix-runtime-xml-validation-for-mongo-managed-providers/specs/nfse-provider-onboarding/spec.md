## MODIFIED Requirements

### Requirement: Provider onboarding validation

O `ProviderOnboardingValidator` MUST incluir verificação de atributos required no check `RuntimeProducible`. Quando o XML produzido é validado contra XSD e falha por atributo missing, o diagnóstico MUST indicar qual atributo é required e em qual complexType.

#### Scenario: Attribute validation in onboarding check
- **WHEN** o validator executa `RuntimeProducible` e o XML produzido não tem atributo required `Id`
- **THEN** o diagnóstico inclui informação sobre o atributo missing

#### Scenario: Successful validation with attributes
- **WHEN** o validator executa `RuntimeProducible` e todos os atributos required estão presentes
- **THEN** o check passa sem erros de atributos
