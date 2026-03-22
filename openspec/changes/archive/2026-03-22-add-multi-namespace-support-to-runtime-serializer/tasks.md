## 1. Modelo canônico — namespace no SchemaModel

- [x] 1.1 Adicionar propriedade `Namespace` (string?) ao record `SchemaComplexType` em `SchemaModel.cs`
- [x] 1.2 Adicionar propriedade `NamespaceMap` (Dictionary<string, string>) ao record `SchemaDocument` em `SchemaModel.cs`

## 2. Analisador de schema — captura de namespace

- [x] 2.1 Ajustar `XsdSchemaAnalyzer.Analyze()` para capturar `QualifiedName.Namespace` de cada `XmlSchemaComplexType` global e propagar para `SchemaComplexType.Namespace`
- [x] 2.2 Ajustar `XsdSchemaAnalyzer.Analyze()` para herdar namespace do schema pai em inline types (anonymous)
- [x] 2.3 Construir `NamespaceMap` a partir dos targetNamespaces de todos os schemas no `XmlSchemaSet` (excluindo xmldsig) e atribuir prefixos automáticos
- [x] 2.4 Propagar `NamespaceMap` para o `SchemaDocument` retornado

## 3. Serializer runtime — emissão multi-namespace

- [x] 3.1 Ajustar `SchemaBasedXmlSerializer.Serialize()` para declarar todos os namespaces do `NamespaceMap` como `xmlns:prefix` no root element
- [x] 3.2 Ajustar `EmitElement()` para resolver o namespace correto ao criar `XElement`: usar `SchemaComplexType.Namespace` quando disponível, fallback para `TargetNamespace`
- [x] 3.3 Ajustar `BuildComplexTypeContent()` para propagar resolução de namespace para elementos filhos
- [x] 3.4 Garantir que providers single-namespace (nacional, ABRASF) continuam funcionando sem regressão

## 4. Testes unitários e validação

- [x] 4.1 Criar testes unitários para `XsdSchemaAnalyzer` validando captura de namespace por tipo em schema multi-namespace (GISSOnline)
- [x] 4.2 Criar testes unitários para `XsdSchemaAnalyzer` validando que schema single-namespace (nacional) preserva namespace em todos os tipos
- [x] 4.3 Criar/ajustar testes de runtime XML para GISSOnline com dados mínimos e validação contra XSD
- [x] 4.4 Garantir que testes existentes de Nacional e ABRASF continuam passando (zero regressão)

## 5. Relatório e validação por provider

- [x] 5.1 Atualizar `AllProvidersXsdValidationSummaryTests` para incluir informação de namespace (single/multi) no relatório
- [x] 5.2 Atualizar `runtime-xsd-validation-summary.md` com status atualizado de todos os providers após multi-namespace
- [x] 5.3 Documentar gaps remanescentes por provider (ISSNet, Paulistana, Simpliss) com motivo técnico explícito
