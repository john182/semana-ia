## MODIFIED Requirements

### Requirement: Auto-generate provider configuration from schema

O `ProviderConfigGenerator` MUST analisar o schema de um provider e gerar automaticamente um `suggested-rules.json` com: rootComplexTypeName, rootElementName, bindings inferidos, wrapperBindings quando aplicável, bindingPathPrefix quando aplicável, e formatting rules inferidas de restrictions do XSD. O `DetectEnvelopePattern` MUST resolver a árvore ABRASF em profundidade arbitrária, navegando recursivamente pela cadeia `EnviarLoteRpsEnvio > LoteRps > ListaRps > Rps > InfRps` (ou equivalente) até encontrar o nó que contém os campos de dados folha. O `dataPathPrefix` MUST refletir o path completo até o nó de dados real, não apenas 2 níveis.

#### Scenario: Generate config for ABRASF-based provider
- **WHEN** o gerador processa um provider com schema ABRASF
- **THEN** gera `suggested-rules.json` com rootComplexTypeName, rootElementName, e bindings inferidos por correspondência de nome

#### Scenario: Generate config for envelope schema
- **WHEN** o gerador processa um provider cujo schema tem envelope (root → wrapper → dados)
- **THEN** gera bindingPathPrefix e wrapperBindings para os elementos wrapper obrigatórios

#### Scenario: Deep walk for ABRASF envelope tree
- **WHEN** o gerador processa schema ABRASF com árvore `EnviarLoteRpsEnvio > LoteRps > ListaRps > Rps > InfDeclaracaoPrestacaoServico`
- **THEN** o `bindingPathPrefix` gerado é `LoteRps.ListaRps.Rps.InfDeclaracaoPrestacaoServico` (ou equivalente até o nó de dados)
- **AND** bindings são gerados para campos dentro de `InfDeclaracaoPrestacaoServico` e seus sub-tipos

#### Scenario: Deep walk for providers with 3+ nesting levels
- **WHEN** o gerador processa schema com mais de 3 níveis de nesting antes dos campos folha
- **THEN** o `DetectEnvelopePattern` navega recursivamente todos os níveis até encontrar o nó de dados
- **AND** o `dataPathPrefix` reflete o caminho completo

#### Scenario: Infer formatting from XSD restrictions
- **WHEN** um elemento do schema tem restriction com maxLength ou pattern numérico
- **THEN** gera formatting rule correspondente (maxLength, padLeft, digitsOnly)

#### Scenario: Mark unmapped fields
- **WHEN** um elemento obrigatório do schema não tem correspondência no modelo de domínio
- **THEN** o binding é gerado com valor `"TODO: manual mapping required"` e o relatório indica o campo como pendente

#### Scenario: Never overwrite existing config
- **WHEN** o provider já tem `base-rules.json`
- **THEN** a config gerada vai para `providers/{name}/generated/suggested-rules.json`, sem sobrescrever

#### Scenario: Shallow envelope preserved for flat schemas
- **WHEN** o gerador processa schema com envelope de apenas 2 níveis (e.g., ISSNet)
- **THEN** o comportamento atual é preservado sem regressão
