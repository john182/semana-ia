---
description: Executa uma change aprovada com orquestração multiagente, testes obrigatórios e revisão técnica final
---

/opsx:apply

Leia AGENTS.md antes de implementar.
Use as skills do projeto conforme a stack afetada.

# Objetivo
Aplicar uma change aprovada seguindo o fluxo do OpenSpec com orquestração técnica mais rigorosa.

# Quando usar
Usar este comando para mudanças que:
- impactam múltiplas camadas;
- exigem testes unitários obrigatórios;
- envolvem serializer XML, geração de XML ou validação XSD;
- exigem revisão técnica final;
- pedem execução coordenada com múltiplas skills.

# Modo de atuação
Atue como um orquestrador técnico multiagente.

Não execute a tarefa como um agente genérico único quando houver skills especializadas aplicáveis.

# Ordem obrigatória de execução
1. Ler e considerar proposal, tasks e change relevantes.
2. Resumir objetivo, escopo e impacto esperado.
3. Identificar o tipo de mudança e selecionar as skills adequadas.
4. Executar a implementação com a skill especializada.
5. Executar criação ou ajuste de testes unitários.
6. Executar criação ou ajuste de testes XML/schema quando aplicável.
7. Executar revisão técnica final.
8. Entregar resumo final com arquivos impactados, testes, riscos e pendências.

# Roteamento obrigatório por tipo de mudança

## Mudança .NET sem XML
Usar em conjunto:
- `openspec-apply-change`
- `dotnet-implementation`
- `write-dotnet-unit-tests`
- `technical-review`

## Mudança .NET com serializer XML, geração de XML ou validação XSD
Usar em conjunto:
- `openspec-apply-change`
- `dotnet-implementation`
- `write-dotnet-unit-tests`
- `write-dotnet-xml-serializer-tests`
- `technical-review`

## Mudança predominantemente estrutural ou refatoração ampla
Usar em conjunto:
- `openspec-apply-change`
- `dotnet-implementation`
- `technical-review`

Adicionar `write-dotnet-unit-tests` sempre que houver impacto funcional.

# Regras obrigatórias
- Não pular leitura do contexto da change.
- Não considerar a mudança concluída sem testes adequados.
- Quando houver XML, não considerar suficiente apenas gerar o XML; validar conteúdo, estrutura e schema quando aplicável.
- Não quebrar contratos públicos sem explicitar o impacto.
- Avaliar constantes para strings repetidas.
- Evitar números mágicos.
- Avaliar uso de enum para conjuntos finitos de valores em `string` ou `int`.
- Explicitar limitações, pendências e validações manuais necessárias.

# Saída esperada
Ao concluir, sempre entregar:
- resumo curto do que foi alterado;
- arquivos impactados;
- decisões técnicas principais;
- testes criados ou ajustados;
- riscos, pendências ou pontos para validação manual.