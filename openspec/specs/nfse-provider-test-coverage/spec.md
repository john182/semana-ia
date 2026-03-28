## MODIFIED Requirements

### Requirement: Provider XML generation test coverage with XSD validation

Cada provider configurado MUST ter uma suite de testes que valide a geração XML end-to-end com validação contra XSD do provider. Os testes MUST usar `ShouldBeValidAgainstProviderSchema(xsdDir)` para providers ABRASF e `ShouldBeValidAgainstDpsSchema()` para nacional. A cobertura MUST se estender a todos os 7 providers (nacional, abrasf, gissonline, issnet, paulistana, simpliss, webiss), não apenas aos MVP providers.

#### Scenario: ISSNet minimal XML generation with XSD validation
- **WHEN** um DpsDocument mínimo é serializado via pipeline para o provider ISSNet
- **THEN** o XML gerado contém o root element correto (`EnviarLoteDpsEnvio` ou equivalente)
- **AND** o XML é válido contra o XSD do provider (`ShouldBeValidAgainstProviderSchema`)

#### Scenario: ISSNet complete XML generation with all bindings
- **WHEN** um DpsDocument completo (com borrower, service, values, location) é serializado para ISSNet
- **THEN** o XML contém todos os bindings configurados no `rules.json`
- **AND** o XML é válido contra o XSD do provider

#### Scenario: ISSNet envelope structure validation
- **WHEN** o XML é gerado para ISSNet
- **THEN** a estrutura de envelope (wrapper elements) corresponde ao esperado pelo XSD
- **AND** atributos `versao` e `Id` estão nos elementos corretos

#### Scenario: GISSOnline minimal XML generation with XSD validation
- **WHEN** um DpsDocument mínimo é serializado via pipeline para o provider GISSOnline
- **THEN** o XML gerado contém root `EnviarLoteRpsEnvio` com `LoteRps > ListaRps > Rps`
- **AND** o XML é válido contra o XSD do provider (`ShouldBeValidAgainstProviderSchema`)

#### Scenario: GISSOnline complete XML generation with all bindings
- **WHEN** um DpsDocument completo é serializado para GISSOnline
- **THEN** o XML contém bindings ABRASF (Competencia, ValorServicos, ItemListaServico, etc.)
- **AND** campos obrigatórios v2.04 (trib, IBSCBS) estão presentes
- **AND** o XML é válido contra o XSD do provider

#### Scenario: GISSOnline envelope wrapper bindings
- **WHEN** o XML é gerado para GISSOnline
- **THEN** `LoteRps` contém `versao="2.04"`, `NumeroLote`, `Prestador`, `QuantidadeRps`
- **AND** root element NÃO contém atributo `versao`

#### Scenario: Abrasf filling variations coverage
- **WHEN** múltiplas variações de DpsDocument são serializadas para Abrasf
- **THEN** cada variação MUST gerar XML válido contra o XSD do provider

#### Scenario: Paulistana filling variations coverage
- **WHEN** múltiplas variações de DpsDocument são serializadas para Paulistana
- **THEN** cada variação MUST gerar XML válido contra o XSD do provider

#### Scenario: Simpliss filling variations coverage
- **WHEN** múltiplas variações de DpsDocument são serializadas para Simpliss
- **THEN** cada variação MUST gerar XML válido contra o XSD do provider

#### Scenario: Webiss filling variations coverage
- **WHEN** múltiplas variações de DpsDocument são serializadas para Webiss
- **THEN** cada variação MUST gerar XML válido contra o XSD do provider

#### Scenario: Nacional filling variations coverage
- **WHEN** múltiplas variações de DpsDocument são serializadas para Nacional
- **THEN** cada variação MUST gerar XML válido contra DPS schema (`ShouldBeValidAgainstDpsSchema`)

### Requirement: Provider test isolation

Testes de cada provider MUST ser independentes e não compartilhar estado mutável. Cada suite de testes de provider MUST poder executar isoladamente.

#### Scenario: Provider tests run independently
- **WHEN** testes de um provider específico são executados sem testes de outros providers
- **THEN** todos passam sem dependência de ordem ou estado compartilhado

#### Scenario: Provider tests run in parallel
- **WHEN** testes de múltiplos providers executam em paralelo (xUnit default)
- **THEN** não há race condition ou conflito de recursos
