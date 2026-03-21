# Delta Spec: nfse-serializer-manual

## ADDED Requirements

### Requirement: Mapper completo request-to-domain

O mapper MUST converter todos os campos do `NfseGenerateXmlRequest` expandido para o `DpsDocument` do domínio, incluindo campos escalares do request (valores fiscais, descontos, alíquotas), `Intermediary` (quando presente), `Location` (quando presente), e todos os grupos opcionais: `ForeignTrade`, `Lease`, `Construction`, `ActivityEvent`, `AdditionalInformationGroup`, `Deduction`, `Benefit`, `Suspension`, `ApproximateTotals`, `IbsCbs`.

#### Scenario: Mapping minimal request
- **WHEN** um request mínimo é mapeado (apenas campos obrigatórios)
- **THEN** o DpsDocument resultante contém Provider, Borrower, Service e Values preenchidos, e todos os grupos opcionais são null

#### Scenario: Mapping complete request with all groups
- **WHEN** um request completo é mapeado com intermediário, comércio exterior, evento, informações complementares e todos os valores fiscais
- **THEN** o DpsDocument resultante contém todos os grupos opcionais preenchidos com os valores do request

#### Scenario: Mapping TaxationType string to enum
- **WHEN** o request contém TaxationType="Export"
- **THEN** o DpsDocument.Values.TaxationType é TaxationType.Export

#### Scenario: Mapping intermediary when present
- **WHEN** o request contém Intermediary preenchido
- **THEN** o DpsDocument.Intermediary é uma Person com Name, FederalTaxNumber e Address mapeados

#### Scenario: Mapping intermediary when absent
- **WHEN** o request não contém Intermediary (null)
- **THEN** o DpsDocument.Intermediary é null

### Requirement: Endpoint usa serializer manual

O endpoint POST `/api/v1/nfse/xml` MUST usar `NationalDpsManualSerializer` para gerar o XML, substituindo o serializer antigo no pipeline.

#### Scenario: Endpoint generates XML with manual serializer
- **WHEN** um request válido é enviado ao endpoint
- **THEN** o response contém XML gerado pelo NationalDpsManualSerializer com RootElement="DPS" e versao="1.01"

#### Scenario: Endpoint returns expanded XML for complete request
- **WHEN** um request completo com intermediário e tributos federais é enviado
- **THEN** o XML retornado contém os blocos `<interm>`, `<tribFed>` e demais grupos mapeados

### Requirement: Testes unitários do mapper

O projeto MUST ter testes unitários para o mapper cobrindo cenários mínimo, completo, mapeamento de enum, presença/ausência de grupos opcionais.

#### Scenario: Mapper test coverage
- **WHEN** os testes unitários do mapper são executados
- **THEN** todos os cenários de mapeamento (mínimo, completo, intermediário, valores fiscais, grupos opcionais) são cobertos

### Requirement: Testes de integração do endpoint

O projeto MUST ter testes de integração usando `WebApplicationFactory` que enviem JSON ao endpoint e validem o response (status code, presença de XML, RootElement).

#### Scenario: Integration test minimal request
- **WHEN** um JSON mínimo válido é enviado via POST ao endpoint
- **THEN** o response é 200 OK com XML válido no body

#### Scenario: Integration test complete request
- **WHEN** um JSON completo é enviado via POST ao endpoint
- **THEN** o response é 200 OK com XML contendo todos os blocos expandidos