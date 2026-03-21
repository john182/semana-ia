# Delta Spec: nfse-serializer-build-generation

## ADDED Requirements

### Requirement: Provider-based XSD organization

O projeto MUST organizar schemas por provider em `providers/{provider}/xsd/`, separando dados de código e permitindo expansão sem alteração de projetos C#.

#### Scenario: Add new provider schemas
- **WHEN** um novo provider é adicionado
- **THEN** basta criar `providers/{novo-provider}/xsd/` e `providers/{novo-provider}/rules/base-rules.json` sem alterar código C#
