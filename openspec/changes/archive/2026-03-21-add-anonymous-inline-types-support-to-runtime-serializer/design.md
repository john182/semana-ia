# Design: add-anonymous-inline-types-support-to-runtime-serializer

## Context

Os XSDs dos providers ABRASF, GISSOnline, ISSNet e Paulistana usam inline types extensivamente. Exemplo do ABRASF:

```xml
<xsd:element name="ListaRps" minOccurs="1" maxOccurs="1">
  <xsd:complexType>
    <xsd:sequence>
      <xsd:element name="Rps" type="tcDeclaracaoPrestacaoServico" maxOccurs="unbounded"/>
    </xsd:sequence>
  </xsd:complexType>
</xsd:element>
```

O `XsdSchemaAnalyzer` resolve `ListaRps` como `TypeName = "complex"` (fallback) porque `el.SchemaTypeName.Name` é vazio para inline types. O serializer não encontra "complex" no `typeMap` e trata `ListaRps` como simples → falha.

## Goals / Non-Goals

**Goals:**

- `SchemaElement` com `InlineType?: SchemaComplexType` para tipos inline
- `XsdSchemaAnalyzer` extrai inline types recursivamente
- `SchemaBasedXmlSerializer` usa `InlineType` quando disponível
- Runtime XML validado contra XSD para ABRASF, GISSOnline, ISSNet (e análise para Paulistana)
- Relatório sumarizado de 5 providers

**Non-Goals:**

- Suporte a todos os cenários de negócio profundos
- Substituição do baseline manual

## Decisions

### D-01 — InlineType como propriedade do SchemaElement

**Decisão**: Adicionar `InlineType?: SchemaComplexType` ao `SchemaElement` record. Quando o analyzer encontra um elemento com `ElementSchemaType` que é `XmlSchemaComplexType` sem nome, cria um `SchemaComplexType` com nome `_anon_{elementName}` e o atribui ao `InlineType`.
**Razão**: Preserva a imutabilidade do record. O serializer verifica `InlineType` antes de `typeMap`.

### D-02 — Serializer prioriza InlineType sobre typeMap

**Decisão**: Em `EmitElement`, se `element.InlineType is not null`, usar seus sub-elementos diretamente. Se `InlineType` é null, buscar no `typeMap` pelo `TypeName`.
**Razão**: InlineType é mais específico e local ao elemento.

### D-03 — Root elements também resolvidos como inline types

**Decisão**: Para root elements como `EnviarLoteRpsEnvio` (que são inline), o analyzer registra o inline type no `SchemaDocument.RootInlineType`. O serializer usa esse tipo se `rootComplexTypeName` não for encontrado no `typeMap`.
**Razão**: Resolve o bloqueio de providers onde o root é inline.

## Estrutura

Alterações em 3 arquivos de produção + 1 teste + relatório.
