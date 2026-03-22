## ADDED Requirements

### Requirement: Auto-generate provider configuration from schema

O `ProviderConfigGenerator` MUST analisar o schema de um provider e gerar automaticamente um `suggested-rules.json` com: rootComplexTypeName, rootElementName, bindings inferidos, wrapperBindings quando aplicável, bindingPathPrefix quando aplicável, e formatting rules inferidas de restrictions do XSD.

#### Scenario: Generate config for ABRASF-based provider
- **WHEN** o gerador processa um provider com schema ABRASF
- **THEN** gera `suggested-rules.json` com rootComplexTypeName, rootElementName, e bindings inferidos por correspondência de nome

#### Scenario: Generate config for envelope schema
- **WHEN** o gerador processa um provider cujo schema tem envelope (root → wrapper → dados)
- **THEN** gera bindingPathPrefix e wrapperBindings para os elementos wrapper obrigatórios

#### Scenario: Infer formatting from XSD restrictions
- **WHEN** um elemento do schema tem restriction com maxLength ou pattern numérico
- **THEN** gera formatting rule correspondente (maxLength, padLeft, digitsOnly)

#### Scenario: Mark unmapped fields
- **WHEN** um elemento obrigatório do schema não tem correspondência no modelo de domínio
- **THEN** o binding é gerado com valor `"TODO: manual mapping required"` e o relatório indica o campo como pendente

#### Scenario: Never overwrite existing config
- **WHEN** o provider já tem `base-rules.json`
- **THEN** a config gerada vai para `providers/{name}/generated/suggested-rules.json`, sem sobrescrever

### Requirement: Common field mapping dictionary

O gerador MUST usar um dicionário de mapeamentos comuns entre nomes de campos do schema e propriedades do `DpsDocument` para inferir bindings. O dicionário MUST cobrir campos fiscais comuns: CNPJ, CPF, IM, valores, datas, códigos de serviço e município.

#### Scenario: Map common fiscal fields
- **WHEN** o schema contém elemento `CNPJ` dentro de um prestador
- **THEN** o gerador mapeia para `Provider.Cnpj`

#### Scenario: Unknown field gets TODO
- **WHEN** o schema contém elemento sem correspondência no dicionário
- **THEN** o binding é marcado como TODO

### Requirement: Sample document generation per provider

O `ProviderSampleDocumentGenerator` MUST produzir um `DpsDocument` mínimo válido para um provider, usando bindings do provider para determinar quais campos são necessários e preenchendo com valores fictícios válidos.

#### Scenario: Generate sample for nacional
- **WHEN** o gerador recebe provider "nacional"
- **THEN** produz DpsDocument com Provider.Cnpj, Environment, IssuedOn e demais campos obrigatórios preenchidos

#### Scenario: Generate sample for ISSNet
- **WHEN** o gerador recebe provider "issnet"
- **THEN** produz DpsDocument incluindo Provider.MunicipalTaxNumber (obrigatório para ISSNet)
