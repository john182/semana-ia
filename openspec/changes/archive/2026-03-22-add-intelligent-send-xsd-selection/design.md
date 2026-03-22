## Context

O `XsdSchemaAnalyzer.LoadSchemaSet()` faz `Directory.GetFiles(directory, "*.xsd")` e carrega todos os schemas de um diretório. Isso funciona quando o diretório tem apenas 2-3 schemas (envio + tipos + xmldsig), mas falha quando tem 20+ schemas com múltiplas versões de tipos (`tipos_v02.xsd`, `tipos_v03.xsd`) ou schemas de operações diferentes (envio, resposta, consulta, cancelamento).

O `XmlSchemaSet` do .NET já resolve includes/imports automaticamente. Isso significa que basta carregar **apenas** o XSD de envio — o runtime resolverá as dependências (tipos, xmldsig) via `<xs:include>` e `<xs:import>` declarados no próprio schema.

## Goals / Non-Goals

**Goals:**
- Carregar apenas o XSD de envio + dependências resolvidas automaticamente
- Selecionar o XSD de envio por padrões de nome (heurística)
- Permitir override via config quando a heurística falhar
- Resolver os 8 providers que falharam no load test
- Zero regressão nos 6 providers base

**Non-Goals:**
- Análise semântica profunda do conteúdo do XSD para determinar se é envio
- Suporte a schemas sem nenhum padrão de nome identificável
- Carregar schemas de resposta/consulta/cancelamento

## Decisions

### 1. Carregar apenas o XSD selecionado, não todos do diretório

**Decisão:** `LoadSchemaSet` recebe um único path de XSD e carrega APENAS esse arquivo. Os includes/imports são resolvidos pelo `XmlSchemaSet` automaticamente usando o `XmlUrlResolver` padrão (que resolve paths relativos a partir do diretório do XSD principal).

**Alternativa considerada:** Filtrar quais XSDs carregar do diretório.

**Racional:** Carregar apenas o XSD principal é mais seguro, previsível e alinhado com o comportamento do `XmlSchemaSet`. Filtrar ainda exigiria heurísticas de exclusão e poderia falhar com nomes não-padronizados.

### 2. SendXsdSelector com heurísticas de nome

**Decisão:** Criar `SendXsdSelector` com lista ordenada de padrões de nome para identificar o XSD de envio:

```
Prioridade 1: contém "enviar" e "envio" (ex: enviar-lote-rps-envio-v2_04.xsd)
Prioridade 2: contém "servico_enviar" (ex: servico_enviar_lote_rps_envio.xsd)
Prioridade 3: começa com "DPS" (Nacional: DPS_v1.01.xsd)
Prioridade 4: começa com "Pedido" (Paulistana: PedidoEnvioLoteRPS_v02.xsd)
Prioridade 5: contém "schema_nfse" (ABRASF: schema_nfse_v2-04.xsd)
Prioridade 6: começa com "nfse" e não contém "resposta"/"retorno" (Simpliss: nfse_3.xsd)
Prioridade 7: arquivo único .xsd não-xmldsig no diretório
Fallback: primeiro .xsd que não seja xmldsig
```

**Racional:** Cobre os padrões de nome encontrados em 52 providers reais testados. A lista é extensível.

### 3. Override via ProviderProfile.PrimaryXsdFile

**Decisão:** Adicionar `primaryXsdFile` (string?) ao `ProviderProfile`. Quando presente, o seletor usa esse valor diretamente sem aplicar heurísticas.

**Racional:** Para providers com nomes não-padronizados (ex: `betha.xsd`, `el-nfse.xsd`), o suporte pode definir explicitamente qual é o XSD de envio.

### 4. Diagnóstico quando seleção é ambígua

**Decisão:** Quando o seletor não encontra match ou encontra múltiplos matches, retorna o resultado com flag `IsAmbiguous = true` e lista de candidatos. O `ProviderOnboardingValidator` reporta isso como `ConfigurationGap` com recomendação de usar `primaryXsdFile`.

## Risks / Trade-offs

- **[Risco] Heurística pode selecionar XSD errado** → Mitigation: override via `primaryXsdFile`. A heurística é best-effort.

- **[Risco] `XmlSchemaSet` pode não resolver includes quando carrega apenas 1 arquivo** → Mitigation: includes/imports relativos são resolvidos automaticamente pelo `XmlUrlResolver` do .NET se os arquivos estiverem no mesmo diretório. Testado com Nacional (includes) e GISSOnline (imports).

- **[Trade-off] Carregar 1 XSD vs carregar todos filtrados** → Carregar 1 é mais seguro mas pode perder tipos definidos em arquivos separados que não são referenciados via include/import. Aceitável — se o XSD de envio não referencia um tipo, ele não é necessário para serialização.
