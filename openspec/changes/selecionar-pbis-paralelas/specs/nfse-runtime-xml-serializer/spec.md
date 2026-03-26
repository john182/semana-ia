## MODIFIED Requirements

### Requirement: Runtime XML serialization from SchemaModel

O `SchemaBasedXmlSerializer` MUST receber `SchemaDocument`, dados de entrada (dicionário) e `IProviderRuleResolver`, e produzir XML real respeitando a estrutura do schema. Cada elemento MUST ser emitido no namespace correto conforme o `SchemaComplexType.Namespace` do tipo que o define. Se o tipo não possuir namespace explícito, MUST usar fallback para `SchemaDocument.TargetNamespace`. O root element MUST declarar todos os namespaces do `SchemaDocument.NamespaceMap` como `xmlns:prefix` attributes.

**Adição:** Quando o provider utiliza padrão ABRASF, o serializer MUST detectar e gerar o envelope de envio correto (`EnviarLoteRpsSincrono` ou equivalente), encapsulando o lote RPS dentro da estrutura esperada pelo web service ABRASF. A detecção MUST ser baseada na presença de XSD de envio (`*envio*.xsd`, `*EnviarLote*.xsd`) na pasta do provider.

#### Scenario: Serialize minimal DPS for nacional provider
- **WHEN** o serializer recebe SchemaModel do nacional + dados mínimos (tpAmb, dhEmi, serie, nDPS, etc.)
- **THEN** produz XML com `<DPS>` contendo `<infDPS>` com elementos na ordem do schema
- **AND** o XML é válido contra o XSD do provider nacional
- **AND** todos os elementos estão no namespace único do nacional

#### Scenario: Serialize multi-namespace XML for GISSOnline
- **WHEN** o serializer recebe SchemaModel do GISSOnline + dados mínimos
- **THEN** produz XML com elementos do envelope no namespace `enviar-lote-rps-envio` e elementos de tipos no namespace `tipos`
- **AND** o root element declara ambos os namespaces
- **AND** o XML é válido contra os XSDs do GISSOnline

#### Scenario: ABRASF envelope detection and generation
- **WHEN** o provider possui XSD de envio ABRASF na pasta `providers/{name}/xsd/`
- **THEN** o serializer detecta automaticamente o padrão ABRASF
- **AND** gera o envelope correto encapsulando o lote RPS
- **AND** o XML resultante é válido contra o XSD de envio do provider

#### Scenario: Non-ABRASF provider ignores envelope detection
- **WHEN** o provider não possui XSD de envio ABRASF (ex: nacional)
- **THEN** o serializer mantém comportamento atual sem envelope adicional

#### Scenario: Serialize with choice resolution
- **WHEN** os dados contêm CNPJ para o prestador (choice CNPJ/CPF/NIF/cNaoNIF)
- **THEN** o serializer emite apenas `<CNPJ>` e omite os demais elementos do choice

#### Scenario: Serialize with optional elements omitted
- **WHEN** um elemento opcional não tem valor no dicionário de entrada
- **THEN** o elemento é omitido do XML gerado

#### Scenario: Required element missing produces error
- **WHEN** um elemento obrigatório não tem valor no dicionário
- **THEN** o serializer retorna `SerializationResult` com `IsValid=false` e erro tipado `InputError`
