## ADDED Requirements

### Requirement: Exclude response/composition schemas from send XSD selection
The `SendXsdSelector` SHALL exclude XSD files whose names match response or composition patterns (`CompNfse`, `compNfse`, `nfse_v` sem sufixo de envio) from the candidate list, preventing incorrect root element selection.

#### Scenario: Provider with CompNfse and envio XSD
- **WHEN** a provider directory contains both `compNfse_v01.xsd` and `servico_enviar_lote_rps_envio_v01.xsd`
- **THEN** the selector SHALL exclude `compNfse_v01.xsd` and select the envio XSD

#### Scenario: Provider with only response schemas after exclusion
- **WHEN** all XSD files in a provider directory match exclusion patterns
- **THEN** the selector SHALL return no selection with reason indicating all files were excluded

### Requirement: Recognize ABRASF send patterns
The `SendXsdSelector` SHALL recognize ABRASF-standard send patterns (`enviarLoteRps`, `RecepcionarLoteRps`, `GerarNfse`) as strong candidates for send XSD selection.

#### Scenario: Provider with RecepcionarLoteRpsEnvio XSD
- **WHEN** a provider directory contains `servico_enviar_lote_rps_envio.xsd` among multiple XSDs
- **THEN** the selector SHALL select it with priority matching exact send patterns

#### Scenario: Provider with GerarNfse XSD
- **WHEN** a provider directory contains `GerarNfseEnvio.xsd` among multiple XSDs
- **THEN** the selector SHALL select it as a strong candidate

### Requirement: Prioritize envio/lote over generic NFS-e schemas
The `SendXsdSelector` SHALL always prefer an XSD whose name contains envio or lote patterns over generic NFS-e schema files when both are present.

#### Scenario: Envio XSD alongside generic schema
- **WHEN** a provider directory contains `nfse.xsd` and `enviarLoteRpsEnvio.xsd`
- **THEN** the selector SHALL select `enviarLoteRpsEnvio.xsd` with higher priority
