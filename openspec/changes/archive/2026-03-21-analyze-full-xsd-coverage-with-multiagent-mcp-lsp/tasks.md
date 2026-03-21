# Tasks: analyze-full-xsd-coverage-with-multiagent-mcp-lsp

## 1. Preparar prompts dos agentes

- [x] 1.1 Criar `prompts/xsd-analysis-agent.md` — prompt para XsdAnalysisAgent: ler XSDs, extrair árvore completa de complexTypes e elementos com obrigatoriedade (required vs optional), gerar tabela Markdown
- [x] 1.2 Criar `prompts/serializer-analysis-agent.md` — prompt para SerializerAgent: ler NationalDpsManualSerializer e NFSeNationalSerializeBase, listar todos os métodos Build*, mapear cada método para os elementos XML que gera
- [x] 1.3 Criar `prompts/spec-governance-agent.md` — prompt para SpecAgent: usar MCP spec-assistant para ler specs e critérios, consolidar escopo e decisões de cobertura, listar o que está in-scope vs out-of-scope

## 2. Executar análise multiagente

- [x] 2.1 Executar XsdAnalysisAgent: ler DPS_v1.01.xsd, tiposComplexos_v1.01.xsd e tiposSimples_v1.01.xsd, extrair árvore completa de elementos do DPS/infDPS com profundidade total
- [x] 2.2 Executar SerializerAgent: ler NationalDpsManualSerializer.cs e o código de produção NFSeNationalSerializeBase fornecido pelo usuário, mapear métodos Build* → elementos XSD
- [x] 2.3 Executar SpecAgent via MCP: usar spec-assistant para ler nfse-serializer-manual spec e critérios, consolidar decisões de escopo (in-scope, out-of-scope, deferred)
- [x] 2.4 Executar os 3 agentes em paralelo (Agent tool com múltiplas chamadas simultâneas) e coletar resultados

## 3. Demonstrar LSP

- [x] 3.1 Usar mcp__ide__getDiagnostics ou grep semântico para localizar todos os métodos Build* em NationalDpsManualSerializer e rastrear quais elementos XML cada um emite
- [x] 3.2 Documentar a navegação no relatório: para cada Build* method, listar os elementos XSD que ele gera (ex: BuildProvider → CNPJ/CPF/NIF/cNaoNIF, CAEPF, IM, regTrib)

## 4. Consolidar relatório de cobertura

- [x] 4.1 Criar `docs/coverage/xsd-coverage-report.md` consolidando a análise dos 3 agentes: tabela com complexType, elemento, obrigatoriedade, status (✅/⚠️/❌), método Build* correspondente, notas
- [x] 4.2 Incluir seção de resumo: total de elementos, % cobertos, % parciais, % faltantes
- [x] 4.3 Incluir seção de demonstração MCP/LSP: evidências de que MCP e LSP foram usados no fluxo

## 5. Gerar backlog de evolução

- [x] 5.1 Criar `docs/coverage/evolution-backlog.md` com gaps identificados, priorizados: alta (blocos obrigatórios faltantes), média (blocos opcionais relevantes), baixa (blocos avançados/raros)
- [x] 5.2 Para cada gap, indicar: complexType/elemento faltante, impacto, sugestão de change futura

## 6. Validação

- [x] 6.1 Verificar que os prompts dos agentes estão funcionais e claros
- [x] 6.2 Verificar que o relatório está completo e sem blocos sem status
- [x] 6.3 Verificar que o backlog é acionável e priorizado