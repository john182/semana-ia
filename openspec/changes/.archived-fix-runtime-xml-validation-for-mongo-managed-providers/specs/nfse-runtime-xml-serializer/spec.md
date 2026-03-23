## MODIFIED Requirements

### Requirement: Runtime XML serialization from SchemaModel

O `SchemaBasedXmlSerializer` MUST emitir atributos required do `SchemaComplexType.Attributes` em cada `XElement` gerado, além dos elementos filhos. O valor de cada atributo MUST ser resolvido a partir de `data["{path}.@{attributeName}"]`. Se o atributo é required e não há valor no data dictionary, MUST registrar `SerializationError` com `Kind=InputError`.

#### Scenario: Serialize with required attribute
- **WHEN** o serializer processa complexType com atributo required `Id` e dados contêm `infDPS.@Id`
- **THEN** o XML emitido contém `<infDPS Id="valor">...</infDPS>`

#### Scenario: Required attribute missing
- **WHEN** atributo required sem valor no data dictionary
- **THEN** `SerializationError` com `InputError` é registrado

### Requirement: Sequence order preservation

O serializer MUST emitir elementos na ordem exata definida pelo `SchemaComplexType.Elements`, que reflete a ordem do `xs:sequence` no XSD. O data dictionary NÃO influencia a ordem de emissão.

#### Scenario: Elements emitted in schema order
- **WHEN** o schema define sequência `tpAmb, dhEmi, verAplic, serie, nDPS`
- **THEN** o XML emitido respeita essa ordem exata

### Requirement: Optional structure skip with partial data

O serializer MUST NOT emitir sub-estruturas opcionais (`element.IsRequired == false`) quando os únicos dados disponíveis são de campos genéricos herdados do `CommonFieldMappingDictionary` que se repetem em múltiplos contextos (como `cMun`, `CEP`, `xLgr` de endereços). Uma sub-estrutura opcional SÓ deve ser emitida quando há dados **específicos** para aquele contexto.

#### Scenario: Optional intermediary skipped when only address data exists
- **WHEN** o element `interm` é opcional e os únicos dados são `interm.end.endNac.cMun` (mapeado genericamente)
- **THEN** o element `interm` NÃO é emitido

#### Scenario: Optional intermediary emitted when specific data exists
- **WHEN** o element `interm` é opcional e os dados incluem `interm.CNPJ` (dado específico do intermediário)
- **THEN** o element `interm` é emitido com seus filhos
