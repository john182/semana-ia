# Tasks: generate-national-artifacts-from-schema-model

## 1. SchemaCodeGenerator — records

- [x] 1.1 Criar `SchemaCodeGenerator` em `XmlGeneration/SchemaEngine/` com método `GenerateRecords(SchemaDocument, string outputDir)` que gera um arquivo `.cs` por complexType
- [x] 1.2 Cada record gerado contém propriedades por elemento: `Name` (PascalCase), tipo C# inferido do XSD type (string, decimal, int, DateOnly, etc.), nullabilidade baseada em IsRequired
- [x] 1.3 Incluir header com comentário indicando que o arquivo foi auto-gerado, o complexType de origem e o namespace

## 2. SchemaCodeGenerator — builder skeleton

- [x] 2.1 Adicionar método `GenerateBuilderSkeleton(SchemaDocument, IProviderRuleResolver, string outputDir)` que gera um arquivo `.cs` com classe de builder
- [x] 2.2 Gerar um método `Build{TypeName}` por complexType relevante (filtrar os que fazem parte de TCInfDPS)
- [x] 2.3 Cada método emite os elementos na ordem do XSD: `xml.{elementName}(...)` com placeholder de valor
- [x] 2.4 Para elementos em choice group, gerar comentário `// choice: {groupName}` e condicionais
- [x] 2.5 Para campos com regra de formatting no resolver, gerar comentário ou código de formatação (ex: `// padLeft(6, '0')`)
- [x] 2.6 Para campos com conditional no resolver, gerar comentário `// conditional: {expression}`

## 3. Executar geração para provider nacional

- [x] 3.1 Criar diretório `providers/nacional/generated/Records/` e `providers/nacional/generated/Builders/`
- [x] 3.2 Executar `GenerateRecords` sobre o SchemaModel do DPS nacional
- [x] 3.3 Executar `GenerateBuilderSkeleton` sobre o SchemaModel com regras do `base-rules.json`
- [x] 3.4 Verificar que os arquivos foram gerados corretamente

## 4. Relatório de comparação

- [x] 4.1 Criar método `GenerateComparisonReport(SchemaDocument, string manualSerializerPath)` que lista complexTypes do schema e mapeia para métodos Build* do serializer manual
- [x] 4.2 Gerar `providers/nacional/generated/comparison-report.md` com tabela: complexType | no manual? | no gerado? | campos diferentes

## 5. Testes

- [x] 5.1 Criar `SchemaCodeGeneratorTests` em `tests/.../SchemaEngine/`
- [x] 5.2 Teste: Given_NacionalSchema_Should_GenerateRecordForTCInfDPS (output não vazio, contém record keyword, contém propriedades esperadas)
- [x] 5.3 Teste: Given_NacionalSchema_Should_GenerateBuilderSkeletonWithBuildMethods (output contém Build methods)
- [x] 5.4 Teste: Given_FormattingRule_Should_IncludeFormattingComment (cTribNac aparece com nota de padLeft)

## 6. Build e validação

- [x] 6.1 `dotnet build` sem erros (artefatos gerados não incluídos na compilação)
- [x] 6.2 `dotnet test` com todos os testes passando
