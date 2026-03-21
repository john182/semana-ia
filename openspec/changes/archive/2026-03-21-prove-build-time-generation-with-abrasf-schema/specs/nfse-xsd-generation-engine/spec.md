# Delta Spec: nfse-xsd-generation-engine

## MODIFIED Requirements

### Requirement: XSD analysis with include/import resolution

O `XsdSchemaAnalyzer` MUST capturar SimpleType restrictions (enumerações, patterns, min/maxLength, totalDigits) e associá-las aos elementos do SchemaModel, permitindo que o gerador infira regras do XSD ao invés de depender exclusivamente de rules externas.

#### Scenario: Capture enumerations from SimpleType
- **WHEN** o analyzer processa um XSD com SimpleType contendo enumerações
- **THEN** o SchemaElement correspondente contém `Restriction` com `Enumerations` populado

#### Scenario: Capture pattern from SimpleType
- **WHEN** o analyzer processa um XSD com SimpleType contendo pattern restriction
- **THEN** o SchemaElement correspondente contém `Restriction` com `Pattern` populado

## ADDED Requirements

### Requirement: Multi-provider schema analysis

O `XsdSchemaAnalyzer` MUST ser capaz de analisar schemas de diferentes providers (nacional, ABRASF) produzindo SchemaDocuments válidos para cada um, sem hardcodar lógica específica de provider.

#### Scenario: Analyze ABRASF schema
- **WHEN** o analyzer recebe o XSD do ABRASF (wne_model_xsd_nota_fiscal_abrasf.xsd)
- **THEN** produz um SchemaDocument com namespace `http://www.abrasf.org.br/nfse.xsd` e complexTypes do ABRASF

### Requirement: Build-time generation

O projeto MUST ter um mecanismo de geração em tempo de build que produz artefatos C# a partir dos XSDs de qualquer provider, sem exigir que os artefatos gerados sejam commitados.

#### Scenario: Build generates ABRASF artifacts
- **WHEN** o build do projeto é executado
- **THEN** artefatos C# são gerados para o provider ABRASF em `providers/abrasf/generated/`

#### Scenario: Generated artifacts not versioned
- **WHEN** artefatos são gerados em `providers/{provider}/generated/`
- **THEN** essa pasta está no `.gitignore` e não aparece no controle de versão

### Requirement: Generated XML validation against XSD

O XML produzido por um builder gerado MUST ser validado contra os XSDs do provider. A validação MUST cobrir choice groups, sequences, obrigatoriedade e restrições de SimpleType.

#### Scenario: Choice group emits exactly one element
- **WHEN** um builder gerado processa um complexType com choice (CNPJ/CPF/NIF/cNaoNIF)
- **THEN** o XML resultante contém exatamente um elemento do choice e é válido contra o XSD

#### Scenario: Sequence order respected
- **WHEN** um builder gerado emite elementos de um sequence
- **THEN** os elementos aparecem na ordem definida pelo XSD e a validação não falha

#### Scenario: Required elements present
- **WHEN** um builder gerado emite XML para um complexType com elementos obrigatórios
- **THEN** todos os elementos obrigatórios estão presentes e o XML é válido

#### Scenario: SimpleType restrictions respected
- **WHEN** um builder gerado emite um valor para um elemento com pattern/length/enumeration
- **THEN** o valor respeita a restrição e o XML é válido contra o XSD
