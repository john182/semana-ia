## Why

O `XsdSchemaAnalyzer.LoadSchemaSet()` carrega TODOS os `*.xsd` de um diretório com `Directory.GetFiles(directory, "*.xsd")`. Quando o provider tem múltiplos schemas (envio, resposta, consulta, cancelamento) e múltiplas versões de tipos no mesmo diretório, o `XmlSchemaSet.Compile()` falha com `XmlSchemaException` por conflito de tipos globais duplicados. Isso causou falha em 8/52 providers no load test (GINFES, ISSNET, National, Natal, Salvador, DBSeller, GeisWeb, Paulistana). A solução é selecionar apenas o XSD de envio + seus tipos auxiliares, em vez de carregar todos os schemas indiscriminadamente.

## What Changes

- Criar `SendXsdSelector` que identifica automaticamente o XSD principal de envio por provider baseado em padrões de nome (`enviar*`, `servico_enviar*`, `DPS*`, `Pedido*`, `schema_nfse*`, `nfse*`, `betha*`)
- Ajustar `XsdSchemaAnalyzer.LoadSchemaSet()` para carregar apenas o XSD selecionado + seus imports/includes resolvidos pelo `XmlSchemaSet`, em vez de todos os `*.xsd` do diretório
- Permitir override via `ProviderProfile.primaryXsdFile` para providers com nome não-convencional
- Atualizar `ProviderConfigGenerator` e `SchemaSerializationPipeline` para usar o seletor
- Validar com todos os providers existentes + re-executar load test com providers externos
- Gerar relatório com XSD selecionado por provider

## Capabilities

### New Capabilities

_Nenhuma capability nova — a mudança melhora a capability existente de schema analysis._

### Modified Capabilities

- `nfse-xsd-generation-engine`: Ajustar loading de XSD para selecionar apenas o schema de envio + tipos auxiliares referenciados, em vez de todos os schemas do diretório

## Impact

- **XsdSchemaAnalyzer.cs**: `LoadSchemaSet` carrega apenas o XSD selecionado (includes/imports resolvidos automaticamente pelo `XmlSchemaSet`)
- **Novo `SendXsdSelector.cs`**: Seleção inteligente do XSD de envio por padrões de nome
- **ProviderProfile.cs**: Nova propriedade `PrimaryXsdFile` (string?) para override
- **ProviderConfigGenerator.cs**: Usar seletor em vez de `xsdFiles[0]`
- **SchemaSerializationPipeline.cs**: Usar seletor
- **Testes**: Validação expandida para providers multi-XSD
- **Load test**: Re-execução com providers que falhavam por conflito
- **Providers estáveis**: Nacional, ABRASF, GISSOnline, ISSNet devem continuar PASS
