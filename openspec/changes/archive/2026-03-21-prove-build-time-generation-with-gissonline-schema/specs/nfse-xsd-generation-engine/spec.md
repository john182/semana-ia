# Delta Spec: nfse-xsd-generation-engine

## ADDED Requirements

### Requirement: GISSOnline provider onboarding

O `SchemaGenerationRunner` MUST ser capaz de processar o provider GISSOnline sem alterações no código da engine, confirmando que o onboarding é data-driven.

#### Scenario: Analyze GISSOnline schema
- **WHEN** o runner é executado para o provider "gissonline"
- **THEN** produz SchemaDocument com complexTypes do GISSOnline e artefatos em `providers/gissonline/generated/`

#### Scenario: Validate generated XML against GISSOnline XSD
- **WHEN** XML mínimo é construído de acordo com o schema do GISSOnline
- **THEN** o XML é válido contra os XSDs do provider
