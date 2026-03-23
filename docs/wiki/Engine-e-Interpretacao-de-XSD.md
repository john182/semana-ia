# Engine e Interpretacao de XSD

## Visao geral

O motor de geracao de XML da NFSe opera a partir de schemas XSD reais dos providers.
Em vez de usar mapeamentos hardcoded, a engine le o XSD, constroi um modelo intermediario
(`SchemaDocument`) e usa esse modelo para serializar XML dinamicamente.

O fluxo principal e:

```
XSD files  -->  XsdSchemaAnalyzer  -->  SchemaDocument  -->  SchemaBasedXmlSerializer  -->  XML
```

---

## XsdSchemaAnalyzer

A classe `XsdSchemaAnalyzer` e o ponto de entrada para interpretacao de schemas.
Ela recebe o caminho de um arquivo `.xsd`, carrega todos os XSDs do mesmo diretorio
em um `XmlSchemaSet`, compila o schema set e produz um `SchemaDocument`.

**Responsabilidades:**

- Carregar e compilar todos os arquivos `*.xsd` do diretorio do provider
- Identificar o `TargetNamespace` principal (ignorando `xmldsig#`)
- Extrair o `RootElementName` do primeiro elemento global
- Analisar todos os `ComplexType` globais e inline
- Extrair restrictions (pattern, minLength, maxLength, enumerations, totalDigits)
- Detectar o atributo `versao` no elemento raiz
- Construir o `NamespaceMap` com prefixos declarados ou gerados (`ns1`, `ns2`, ...)
- Extrair `SchemaAttribute` de cada ComplexType (ignorando atributos de infraestrutura como `xmlns`, `xsi:`, `xml:`)

---

## SchemaModel

O modelo intermediario e composto por records imutaveis:

### SchemaDocument

| Propriedade          | Tipo                              | Descricao                                         |
|----------------------|-----------------------------------|----------------------------------------------------|
| TargetNamespace      | `string`                          | Namespace principal do schema                      |
| RootElementName      | `string`                          | Nome do elemento raiz (ex: `DPS`, `EnviarLoteRpsEnvio`) |
| ComplexTypes         | `List<SchemaComplexType>`         | Todos os tipos complexos encontrados               |
| RootInlineType       | `SchemaComplexType?`              | Tipo inline do elemento raiz, quando anonimo       |
| NamespaceMap         | `Dictionary<string, string>?`     | Mapa prefixo -> URI de namespace                   |
| RootVersionAttribute | `string?`                         | Valor fixo/default do atributo `versao` na raiz    |

### SchemaComplexType

| Propriedade | Tipo                       | Descricao                              |
|-------------|----------------------------|----------------------------------------|
| Name        | `string`                   | Nome do tipo (ou `_anon_ElementName`)  |
| Elements    | `List<SchemaElement>`      | Elementos filhos                       |
| Annotation  | `string?`                  | Documentacao do XSD                    |
| Namespace   | `string?`                  | Namespace onde o tipo foi definido     |
| Attributes  | `List<SchemaAttribute>?`   | Atributos XML do tipo complexo         |

### SchemaElement

| Propriedade | Tipo                           | Descricao                                  |
|-------------|--------------------------------|--------------------------------------------|
| Name        | `string`                       | Nome do elemento                           |
| TypeName    | `string`                       | Nome do tipo referenciado                  |
| IsRequired  | `bool`                         | `MinOccurs > 0` e nao e choice             |
| MinOccurs   | `int`                          | Ocorrencia minima                          |
| MaxOccurs   | `int`                          | Ocorrencia maxima (`-1` = unbounded)       |
| IsChoice    | `bool`                         | Pertence a um grupo `xs:choice`            |
| ChoiceGroup | `string?`                      | Identificador do grupo (ex: `choice_1`)    |
| Restriction | `SchemaSimpleTypeRestriction?` | Restricoes do tipo simples                 |
| InlineType  | `SchemaComplexType?`           | Tipo complexo anonimo inline               |

### SchemaAttribute

| Propriedade | Tipo     | Descricao                    |
|-------------|----------|------------------------------|
| Name        | `string` | Nome do atributo             |
| TypeName    | `string` | Tipo do atributo             |
| IsRequired  | `bool`   | `use="required"` no schema   |

### SchemaSimpleTypeRestriction

Captura facets de `xs:restriction`: `Pattern`, `MinLength`, `MaxLength`, `Enumerations` e `BaseType`.

---

## Tipos inline e o prefixo `_anon_`

Quando um elemento raiz ou filho possui um tipo complexo anonimo (sem `name` no XSD),
o analyzer gera um nome sintetico com o prefixo `_anon_` seguido do nome do elemento.

Exemplo: o elemento `<DPS>` com tipo inline produz `_anon_DPS`.
Isso permite que o serializer encontre o tipo pelo nome no `typeMap`.

---

## Multi-namespace e NamespaceMap

Providers como o nacional possuem multiplos namespaces no mesmo schema set.
O `BuildNamespaceMap` percorre todos os schemas compilados e monta um dicionario
`prefix -> namespaceUri`:

- Usa o prefixo declarado no XSD quando disponivel
- Gera prefixos `ns1`, `ns2`, ... quando nao ha prefixo declarado
- Ignora `xmldsig#` e namespaces vazios

O serializer adiciona as declaracoes `xmlns:prefix` no elemento raiz e
usa o namespace do tipo (nao do elemento) para emitir conteudo filho.

---

## SendXsdSelector

Providers frequentemente possuem multiplos arquivos XSD (envio, retorno, tipos, cancelamento, etc.).
O `SendXsdSelector` identifica qual e o XSD de envio usando heuristicas:

**Prioridades de selecao:**

| Prioridade | Patterns                                                                |
|------------|-------------------------------------------------------------------------|
| 1 (Exata)  | `servico_enviar`, `enviar`, `envio_lote`                               |
| 2 (Forte)  | `RecepcionarLoteRps`, `GerarNfse`, `EnviarLoteRps`, `DPS_`, `Pedido`  |
| 3 (Fallback)| `schema_nfse`, `nfse_`                                                |

**Patterns de exclusao:** `resposta`, `retorno`, `consulta`, `cancelar`, `tipos_`, `cabecalho`,
`TiposNFe`, `CompNfse`, `ConsultarNfse`, `SubstituirNfse`, entre outros.

**Override:** Quando o `ProviderProfile.PrimaryXsdFile` esta definido, o selector usa
diretamente esse arquivo, sem aplicar heuristicas.

**Resultado:** `XsdSelectionResult` com `SelectedFile`, `IsAmbiguous`, `Candidates` e `Reason`.

---

## Como o serializer usa o modelo

O `SchemaBasedXmlSerializer` recebe o `SchemaDocument`, um dicionario de dados
(`Dictionary<string, object?>`) e um `IProviderRuleResolver`, e produz XML:

1. **Localiza o root type** no `typeMap` pelo `rootComplexTypeName` (com fallback para `RootInlineType`)
2. **Cria o elemento raiz** com o namespace e adiciona declaracoes de namespace adicionais
3. **Emite o atributo `versao`** no root, se o tipo raiz o declara
4. **`BuildComplexTypeContent`**: itera cada `SchemaElement` do tipo:
   - Se e `choice`, delega para `EmitChoiceGroup` que seleciona o branch com dados
   - Caso contrario, chama `EmitElement`
5. **`EmitElement`**: decide entre elemento complexo e simples:
   - **Complexo**: cria `XElement`, emite atributos via `EmitAttributes`, e recursa via `BuildComplexTypeContent`
   - **Simples**: resolve valor do dicionario de dados, aplica `ApplyFormatting` (digitsOnly, padLeft, maxLength)
   - Se nao ha valor, tenta `resolver.ResolveDefault`; se obrigatorio e sem default, registra erro
6. **Elementos opcionais com apenas campos genericos** (`GenericReusableFields` como CNPJ, CEP, xNome)
   sao suprimidos para evitar emitir containers vazios de significado

---

## Links relacionados

- [Arquitetura](Arquitetura.md)
- [Validacao Automatica](Validacao-Automatica.md)
- [Regras de Provider](Regras-de-Provider.md)
