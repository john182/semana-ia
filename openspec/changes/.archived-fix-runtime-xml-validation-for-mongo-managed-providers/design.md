## Context

O pipeline schema engine produz XML para providers gerenciados via MongoDB, mas o XML não passa XSD validation. O fallback manual (nacional/XBuilder) funciona. O gap está na "última milha": analyzer não captura atributos, serializer não respeita sequence order, sub-estruturas opcionais são emitidas com dados parciais, e o sample generator produz dados que não satisfazem patterns do XSD.

Código afetado: `XsdSchemaAnalyzer`, `SchemaModel`, `SchemaBasedXmlSerializer`, `ProviderConfigGenerator`, `ProviderSampleDocumentGenerator`, `ServiceInvoiceSchemaDataBinder`.

## Goals / Non-Goals

**Goals:**
- XSD analyzer captura `xs:attribute` (required/optional) e expõe no SchemaModel
- Serializer emite atributos required antes dos elementos filhos
- Serializer emite elementos na ordem exata do `xs:sequence`
- Serializer não entra em sub-estruturas opcionais sem dados suficientes
- Auto-gen cria rules para atributos required automaticamente
- Sample generator produz dados aderentes a patterns, minLength, maxLength e tipos do XSD
- Providers gerenciados via Mongo geram XML XSD-valid no endpoint real
- Testes E2E de integração desbloqueados (remover Skip)
- Fallback manual nacional sem regressão

**Non-Goals:**
- Resolver 100% dos cenários de negócio de todos os 48 providers
- Substituir o serializer manual nacional
- UI administrativa
- Refatoração ampla fora do necessário para fechar o gap

## Decisions

### 1. SchemaAttribute no modelo

**Decisão**: Adicionar `record SchemaAttribute(string Name, string TypeName, bool IsRequired)` e `List<SchemaAttribute> Attributes` ao `SchemaComplexType`.

**Razão**: O `xs:attribute` é fundamentalmente diferente de `xs:element` — é emitido como XML attribute (`<elem attr="value">`), não como child element. Precisa de modelo separado.

**Captura**: O `XsdSchemaAnalyzer` itera `XmlSchemaComplexType.AttributeUses` (via `Values`) e extrai nome, tipo e `use="required"`.

### 2. Emissão de atributos no serializer

**Decisão**: No `BuildComplexTypeContent`, após criar o `XElement`, iterar `complexType.Attributes` e emitir cada atributo required. O valor vem de `data["{path}.@{attributeName}"]` — o mesmo padrão que já existe para `@Id`.

**Razão**: Reutiliza o padrão existente (`@Id`) para todos os atributos, sem criar mecanismo novo.

### 3. Garantia de ordem do xs:sequence

**Decisão**: O serializer já emite na ordem de `SchemaComplexType.Elements` (que vem do analyzer na ordem do schema). O problema real é que o `data dictionary` não influencia a ordem — o serializer itera os elements do schema e busca dados. Se o dado existe, emite. Se não, pula (ou erro). A ordem já é correta **pela iteração do schema**, não pelo dicionário.

**Investigação necessária**: Confirmar se o bug de ordem é real (o erro XSD `"expected 'verAplic'"` antes de `serie` era porque `verAplic` não estava sendo emitido, não porque `serie` estava fora de ordem). Com `verAplic` no dicionário (já adicionado), a ordem deve estar correta.

### 4. Skip de sub-estruturas opcionais com dados parciais

**Decisão**: No `EmitElement`, quando um complex element é **opcional** (`!element.IsRequired`) e `hasChildData` é true, verificar se os dados são "substanciais" — ou seja, se pelo menos um campo **direto** (não herdado de mapeamento genérico) tem valor. Se os únicos dados vêm de campos comuns mapeados pelo `CommonFieldMappingDictionary` que aparecem em múltiplos contextos (como `cMun` que existe em todo endereço), não considerar como "substancial".

**Alternativa mais simples**: Não emitir complex elements opcionais que têm **apenas** dados de campos comuns recursivos (endereço). Usar heurística: se o path do dado contém mais de 3 segmentos e o parent é opcional, skip.

**Alternativa escolhida**: A mais pragmática — o serializer verifica se o parent optional element tem dados **específicos** (que não são apenas `cMun`, `CEP`, `xLgr`, `nro`, `xBairro` repetidos de endereço genérico). Se só tem dados genéricos de endereço, skip.

### 5. Sample data aderente ao schema

**Decisão**: O `ProviderSampleDocumentGenerator` consulta as `SchemaElement.Restriction` para inferir valores válidos:
- Se tem `Pattern` → gerar valor que satisfaça o pattern (ex: `TSSerieDPS` → `"00001"`)
- Se tem `MinLength`/`MaxLength` → gerar string com tamanho correto
- Se tem `Enumerations` → usar o primeiro valor
- Fallback: usar os dummy values existentes

**O schema precisa ser passado ao generator**: Novo overload `Generate(ProviderProfile profile, SchemaDocument? schema)`.

### 6. Auto-gen de rules para atributos required

**Decisão**: No `ProviderConfigGenerator.GenerateFromXsdFiles`, após gerar rules para elementos, iterar `complexType.Attributes` e gerar rules para atributos required. Para `Id` em `infDPS` → `BuildId`. Para outros atributos → `const:` com valor inferido ou `TODO`.

## Risks / Trade-offs

**[Risk] Heurística de skip de sub-estruturas opcionais pode ser frágil**
→ Mitigation: Lista de campos "genéricos" de endereço é finita e conhecida. Testes de regressão para todos os providers.

**[Risk] Captura de atributos pode trazer atributos indesejados (namespace, schemaLocation)**
→ Mitigation: Filtrar atributos de infraestrutura XML (xmlns, xsi) na extração.

**[Risk] Sample generator com schema pode ficar complexo**
→ Mitigation: Usar pattern matching simples, não regex engine completa. Fallback para dummy values quando pattern é muito complexo.

**[Trade-off] Schema passado ao sample generator aumenta acoplamento**
→ O generator já depende do profile. Adicionar schema como parâmetro opcional é acoplamento mínimo.
