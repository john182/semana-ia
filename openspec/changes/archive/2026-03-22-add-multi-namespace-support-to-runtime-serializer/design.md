## Context

O runtime serializer (`SchemaBasedXmlSerializer`) e o analisador de schema (`XsdSchemaAnalyzer`) atualmente tratam o schema como single-namespace. O `SchemaDocument.TargetNamespace` é um único string, e todos os elementos emitidos usam `XNamespace ns = schema.TargetNamespace`.

Providers como GISSOnline definem dois XSDs com namespaces distintos:
- `http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd` (envelope)
- `http://www.giss.com.br/tipos-v2_04.xsd` (tipos compartilhados)

Quando o serializer emite elementos do tipo `tcLoteRps` (definido no namespace `tipos`), ele usa o namespace do envelope, gerando XML que falha na validação XSD.

O `XmlSchemaSet` do .NET já resolve cross-namespace references automaticamente, mas o `XsdSchemaAnalyzer` descarta a informação de namespace ao construir o `SchemaModel`.

## Goals / Non-Goals

**Goals:**
- Preservar namespace de origem por `SchemaComplexType` no `SchemaModel`
- Emitir elementos no namespace correto durante serialização runtime
- Declarar namespace prefixes necessários no root element do XML
- GISSOnline avançar de FAIL para runtime XML validável
- Nacional e ABRASF continuarem PASS (zero regressão)

**Non-Goals:**
- Suporte a namespace qualificado por atributo (attributeFormDefault)
- Resolução de conflitos de nome entre namespaces diferentes
- Onboarding completo de business logic para providers multi-namespace
- Suporte a mais de 2-3 namespaces simultâneos nesta fase

## Decisions

### 1. Namespace como propriedade de SchemaComplexType

**Decisão:** Adicionar `string? Namespace` ao record `SchemaComplexType`.

**Alternativa considerada:** Adicionar namespace a cada `SchemaElement` individualmente.

**Racional:** O namespace é propriedade do tipo, não do elemento. Um elemento herda o namespace do seu tipo definidor. Isso é mais fiel ao modelo XSD e evita redundância. O `SchemaElement` já tem `TypeName` que referencia o tipo — o namespace vem do tipo resolvido.

### 2. NamespaceMap no SchemaDocument

**Decisão:** Adicionar `Dictionary<string, string> NamespaceMap` ao `SchemaDocument`, mapeando prefixo → namespace URI.

**Alternativa considerada:** Lista simples de namespace URIs.

**Racional:** O serializer precisa declarar `xmlns:prefix` no root element. O mapeamento prefixo→URI permite emitir declarações corretas e resolver o namespace de cada tipo durante serialização.

### 3. Resolução de namespace no serializer via typeMap

**Decisão:** Ao emitir um elemento cujo tipo é um `SchemaComplexType`, usar o `Namespace` do tipo (se presente) em vez do namespace root. Se o tipo não tem namespace explícito, fazer fallback ao `TargetNamespace` do documento.

**Alternativa considerada:** Lookup separado de namespace por elemento path.

**Racional:** Aproveita a estrutura existente do `typeMap` (Dictionary<string, SchemaComplexType>). Ao encontrar o tipo no map, o namespace já está disponível como propriedade do tipo.

### 4. Declaração de namespaces no root element

**Decisão:** Iterar `SchemaDocument.NamespaceMap` e adicionar `XAttribute` com `XNamespace.Xmlns + prefix` no root element, antes de emitir elementos filhos.

**Racional:** O XSD exige que todos os namespaces usados estejam declarados. Declarar no root é o padrão mais comum e evita repetição em elementos internos.

### 5. Captura de namespace no XsdSchemaAnalyzer

**Decisão:** Usar `XmlSchemaType.QualifiedName.Namespace` ao iterar `schemaSet.GlobalTypes.Values` para capturar o namespace de cada tipo. Para inline types, herdar o namespace do schema pai.

**Racional:** O `XmlSchemaSet` já resolve namespaces automaticamente. O `QualifiedName` é a fonte autoritativa.

## Risks / Trade-offs

- **[Risco] Regressão em Nacional/ABRASF** → Mitigation: Nacional e ABRASF usam single-namespace. O fallback para `TargetNamespace` quando `SchemaComplexType.Namespace` é null garante compatibilidade total.

- **[Risco] Conflitos de nome entre namespaces** → Mitigation: Fora de escopo nesta fase. O `typeMap` usa nome simples como chave. Se dois tipos com mesmo nome existirem em namespaces diferentes, apenas o último será registrado. Documentar como gap.

- **[Risco] Namespace de elementos simples** → Mitigation: Elementos simples (leaf nodes) usam o namespace do complexType pai. Isso é o comportamento padrão do `elementFormDefault="qualified"`.

- **[Trade-off] Namespace no tipo vs no elemento** → Adicionar namespace apenas ao tipo simplifica o modelo mas pode ser insuficiente para cenários onde um elemento referencia tipo de namespace diferente do seu pai. Aceitar essa limitação nesta fase.
