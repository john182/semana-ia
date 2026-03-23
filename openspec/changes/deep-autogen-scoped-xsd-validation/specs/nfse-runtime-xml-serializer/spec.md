## MODIFIED Requirements

### Requirement: XSD validation of output

O serializer MUST validar o XML gerado apenas contra o XSD de envio do provider (selecionado via `SendXsdSelector`) e seus tipos auxiliares referenciados via include/import, **excluindo** schemas de response, consulta e cancelamento. O `ValidateXmlAgainstXsd` MUST receber o path do XSD de envio selecionado (não o diretório inteiro) e usar `XmlSchemaSet` para resolver includes/imports automaticamente.

#### Scenario: Valid XML passes validation against send schema only
- **WHEN** o XML gerado é estruturalmente correto
- **THEN** `SerializationResult.IsValid` é true e `ValidationErrors` é vazio
- **AND** a validação foi executada apenas contra o schema de envio e tipos auxiliares

#### Scenario: Invalid XML reports validation errors
- **WHEN** o XML gerado viola uma restrição do XSD de envio
- **THEN** `SerializationResult.ValidationErrors` contém a descrição do erro

#### Scenario: Response schemas excluded from validation
- **WHEN** o diretório do provider contém XSDs de response, consulta e cancelamento além do envio
- **THEN** a validação carrega apenas o XSD de envio selecionado pelo `SendXsdSelector`
- **AND** tipos de response não interferem na validação

#### Scenario: Send XSD with includes resolves automatically
- **WHEN** o XSD de envio referencia tipos via `<xs:include>` ou `<xs:import>`
- **THEN** o `XmlSchemaSet` resolve automaticamente os includes/imports do mesmo diretório
- **AND** a validação cobre o schema completo de envio com seus tipos auxiliares

#### Scenario: Fallback when SendXsdSelector cannot determine send XSD
- **WHEN** o `SendXsdSelector` não consegue identificar o XSD de envio (ambíguo ou ausente)
- **THEN** a validação usa o comportamento atual (carregar todos os `*.xsd`) e registra warning no resultado

### Requirement: Scoped validation in onboarding check

O `ProviderOnboardingValidator` no check `XsdValid` MUST usar validação scoped ao XSD de envio, consistente com a validação do serializer.

#### Scenario: XsdValid check uses send schema only
- **WHEN** o validator executa o check `XsdValid`
- **THEN** compila e valida apenas contra o XSD de envio e seus auxiliares
- **AND** schemas de response não causam falha no check

#### Scenario: XsdValid fallback on ambiguous selection
- **WHEN** o `SendXsdSelector` retorna resultado ambíguo
- **THEN** o check usa todos os XSDs (comportamento atual) e inclui warning no `Details`
