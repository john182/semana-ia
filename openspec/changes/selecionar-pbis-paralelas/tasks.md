## 1. Análise e Seleção de PBIs

- [x] 1.1 Revisar backlog Fase 4 e mapear cada PBI para camadas Onion e arquivos-alvo
- [x] 1.2 Construir matriz de colisão (PBI x PBI) identificando arquivos compartilhados
- [x] 1.3 Selecionar as 2 PBIs com menor colisão e documentar justificativa (resultado: Health Check + Envelope ABRASF)

## 2. Validação por Spec-Agent

- [x] 2.1 Executar spec-agent para PBI-A (nfse-health-check) — validar que a spec está completa e sem ambiguidade
- [x] 2.2 Executar spec-agent para PBI-B (nfse-runtime-xml-serializer delta ABRASF) — validar que o delta spec é coerente com a spec base

## 3. Preparação de Worktrees

- [x] 3.1 Criar worktree para PBI-A: `git worktree add ../pbi-health-check master`
- [x] 3.2 Criar worktree para PBI-B: `git worktree add ../pbi-envelope-abrasf master`
- [x] 3.3 Verificar que ambas as worktrees compilam e testes existentes passam

## 4. Implementação PBI-A — Health Check Endpoint

- [x] 4.1 Criar `HealthController` em `Api/Controllers/` com endpoint `GET /health`
- [x] 4.2 Implementar health check que consulta providers disponíveis via `IProviderResolver` ou serviço equivalente
- [x] 4.3 Implementar health check de conectividade MongoDB
- [x] 4.4 Retornar status Healthy/Degraded/Unhealthy com resumo de providers por `OperationalStatus`
- [x] 4.5 Registrar health checks no DI container (`AddHealthChecks()`)

## 5. Implementação PBI-B — Envelope ABRASF

- [x] 5.1 Investigar XSDs de GISSOnline e Simpliss para entender estrutura de envelope de envio
- [x] 5.2 Implementar detecção de XSD de envio ABRASF na pasta do provider
- [x] 5.3 Ajustar `SchemaSerializationPipeline` ou `SchemaBasedXmlSerializer` para gerar envelope correto
- [x] 5.4 Verificar que providers não-ABRASF (nacional, ISSNet) mantêm comportamento inalterado

## 6. Testes PBI-A

- [x] 6.1 Testes unitários para `HealthController` (cenários Healthy, Degraded, Unhealthy)
- [x] 6.2 Teste de integração para `GET /health` via WebApplicationFactory
- [x] 6.3 Verificar que todos os 727+ testes existentes continuam passando

## 7. Testes PBI-B

- [x] 7.1 Testes unitários para detecção de envelope ABRASF
- [x] 7.2 Testes de serialização XML para GISSOnline com envelope correto + validação XSD
- [x] 7.3 Testes de serialização XML para Simpliss com envelope correto + validação XSD
- [x] 7.4 Testes de regressão: nacional, ISSNet, ABRASF continuam PASS
- [x] 7.5 Verificar que todos os 727+ testes existentes continuam passando

## 8. Review Cruzado

- [x] 8.1 Review-agent PBI-A: verificar que health check não introduz dependências em XmlGeneration
- [x] 8.2 Review-agent PBI-B: verificar que envelope ABRASF não altera interface pública usada pela Api
- [x] 8.3 Validar que nenhum arquivo foi editado por ambas as PBIs

## 9. Merge e Validação Final

- [x] 9.1 Merge worktree PBI-A → master
- [x] 9.2 Rodar suite completa de testes no master atualizado
- [x] 9.3 Rebase worktree PBI-B sobre novo master
- [x] 9.4 Rodar suite completa de testes no PBI-B rebasado
- [x] 9.5 Merge worktree PBI-B → master
- [x] 9.6 Rodar suite completa de testes final no master
- [x] 9.7 Limpar worktrees (`git worktree remove`)
