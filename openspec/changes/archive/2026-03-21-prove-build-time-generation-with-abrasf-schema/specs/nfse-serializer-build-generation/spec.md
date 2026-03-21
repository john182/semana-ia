# Delta Spec: nfse-serializer-build-generation

## MODIFIED Requirements

### Requirement: Build command for regeneration

O projeto MUST expor um mecanismo de build (runner ou MSBuild target) que regenera artefatos a partir dos XSDs de cada provider. A execução MUST ser reproduzível e determinística.

#### Scenario: Runner generates artifacts for provider
- **WHEN** o runner é executado com provider "abrasf"
- **THEN** records e builder skeleton são gerados em `providers/abrasf/generated/` a partir dos XSDs
