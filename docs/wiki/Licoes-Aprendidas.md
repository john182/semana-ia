# Lições Aprendidas

> Projeto NFSe: 34 commits, 727 testes, 48 providers, 3 MVPs.
> Uma semana de desenvolvimento intensivo com IA. Aqui está o que ficou.

---

## O que funcionou

### Runtime serialization > build-time

A decisão de migrar de geração build-time (PRs #16, #17) para serialização runtime (#18) foi a mais impactante do projeto. Build-time gerava classes C# estáticas a partir de XSD — cada provider novo exigia recompilação e deploy. Runtime lê XSD em tempo de execução: adicionar um provider é configuração, não código.

Resultado: escalou de 3 providers para 48 sem alterar uma linha do engine. O `SchemaSerializationPipeline` processa qualquer XSD válido sem saber de antemão qual provider vai receber.

### Typed Rules DSL configurável

O `TypedRuleResolver` (#29) permite que suporte técnico ajuste regras por provider (formato de data, mapeamento de enum, campos opcionais) sem envolver desenvolvimento. É a diferença entre "abre um ticket para dev" e "configura direto no painel".

Sete categorias de regras cobrem 95% dos ajustes necessários entre providers. O catálogo expõe via API quais regras estão disponíveis e quais estão ativas por provider.

### CommonFieldMappingDictionary como base para auto-gen

O dicionário de campos comuns (`CommonFieldMappingDictionary.cs`) identificou que ~80% dos campos são compartilhados entre providers com variações de path. Isso viabilizou o auto-generation de configurações no PR #24 (`feat(engine): add support-friendly provider onboarding with auto-config generation`).

Onboarding de provider novo: upload 4 XSDs, auto-gen gera 80% da config, suporte ajusta os 20% restantes.

### Multi-agent para mudanças complexas

Orquestração com 5 agentes especializados (spec → impl → test → xml-test → review) garantiu que mudanças complexas como o Typed Rule DSL (#29) saíssem completas: código, testes, validação XSD e revisão de padrões — tudo em um ciclo.

O `CLAUDE.md` como contrato entre agentes foi essencial. Sem ele, cada agente tomava decisões inconsistentes de nomeação e estrutura.

### E2E com WebApplicationFactory + MongoDB real

Testes de integração com `WebApplicationFactory` conectando a MongoDB real (via Docker) pegam problemas que unit tests e mocks nunca pegariam. Serialização, persistência, validação XSD e resposta HTTP — tudo validado no mesmo teste.

O investimento em CI com MongoDB service container (`ci.yml`) pagou-se na primeira vez que um teste de integração pegou um bug de serialização que 200 unit tests não viram.

---

## O que não funcionou

### IsDataNode heurística quebrando entre providers

A heurística `IsDataNode` tentava identificar automaticamente quais nós do XSD são "dados" versus "estrutura/envelope". Funcionava para ABRASF puro, mas providers com schemas customizados tinham nós ambíguos. Um nó que é envelope no provider A pode ser dado no provider B.

O PR #31 (`feat(engine): add xs:attribute support, ABRASF envelope detection`) teve que refatorar essa lógica para detecção específica de envelope ABRASF em vez de heurística genérica.

**Lição:** heurísticas genéricas para domínios municipalizados são armadilhas. Cada provider tem suas idiossincrasias.

### Enum-to-code não é 1:1 com enums C#

O mapeamento de enums C# para códigos XML parecia simples: `SimNao.Sim → "1"`, `SimNao.Nao → "2"`. Mas a realidade dos providers é mais complexa. O campo `opSimpNac` (Optante Simples Nacional) em alguns providers aceita `"1"/"2"`, em outros `"S"/"N"`, e em outros `"true"/"false"`.

O PR #32 (`fix(engine): resolve enum-to-code mappings, date format, and ABRASF field gaps`) corrigiu isso, mas o trabalho reativo foi maior do que seria um design preventivo.

**Lição:** enums de domínio fiscal brasileiro nunca são simples. Mapear cedo e por provider.

### GenericReusableFields skip frágil

A tentativa de criar uma lista genérica de campos reutilizáveis que o serializer poderia "pular" (skip) quando já processados foi frágil. A ordem de processamento dos campos variava entre providers, e o skip às vezes pulava campos que precisavam ser emitidos.

**Lição:** "genérico" e "48 providers municipais" não combinam. Preferir explícito sobre implícito.

### Agentes paralelos conflitando em mesmo arquivo

Durante experimentos com execução paralela de agentes, dois agentes tentaram modificar o mesmo arquivo simultaneamente. O resultado foi conflito de merge que precisou de resolução manual.

**Lição:** multi-agente funciona em sequência para o mesmo escopo. Paralelismo só quando os escopos são disjuntos (arquivos diferentes, módulos diferentes).

---

## O que faria diferente

### Enum mapping rules desde o início

Em vez de assumir que enums C# mapeiam diretamente para códigos XML, teria criado o `TypedRuleResolver` com regras de mapeamento de enum desde o PR #18 (runtime serializer). O retrabalho dos PRs #29 e #32 poderia ter sido evitado.

### Separar envelope detection por padrão

Em vez de heurística genérica (`IsDataNode`), teria definido uma estratégia de detecção de envelope por padrão ABRASF desde o início. Cada padrão (ABRASF 2.04, ABRASF 1.0, proprietário) tem uma estrutura de envelope previsível que pode ser detectada por assinatura de namespace.

### Investir em sample data com XSD restrictions

Os testes validavam estrutura XML contra XSD, mas não geravam sample data que respeitasse restrictions (`xs:minLength`, `xs:maxLength`, `xs:pattern`, `xs:enumeration`). Ter um gerador de dados de teste baseado em XSD restrictions teria pego bugs de formato antes de chegar em produção.

---

## Métricas finais

| Métrica | Valor |
|---------|-------|
| Commits totais | 34 |
| Testes (unit + integration) | 727 |
| MVPs entregues | 3 |
| Providers testáveis | 48 |
| Erros XSD no padrão nacional | 0 |
| PRs criados | 33 |
| Agentes especializados | 5 |
| Skills registradas | 3 |
| Servidores MCP | 3 |
| Dias de desenvolvimento | ~7 |

### O que os números não contam

Os 727 testes não foram escritos manualmente. A combinação de unit-test-agent + xml-test-agent com padrões definidos no `CLAUDE.md` (xUnit, Shouldly, `Given_Should`) gerou a maioria. O humano revisou e ajustou.

Os 48 providers testáveis são resultado direto da decisão runtime. Com build-time, teríamos no máximo 10 — o custo por provider seria proibitivo.

Os 0 erros XSD no padrão nacional são resultado do investimento em validação XSD integrada aos testes. Cada teste de serialização valida contra o schema real. Se o XML não é válido, o teste falha.

---

## Conclusão

A maior lição é sobre **quando ser genérico e quando ser específico**. Em domínios como NFSe, onde cada município pode ter regras próprias, abstrações genéricas falham mais do que ajudam. O equilíbrio está em: **base comum + configuração específica por provider**. O `CommonFieldMappingDictionary` + `TypedRuleResolver` materializa exatamente esse equilíbrio.

A segunda lição é sobre **IA como multiplicador, não substituto**. A IA gerou 80% do código e dos testes, mas as decisões arquiteturais (runtime vs build-time, resolução por município, regras configuráveis) foram humanas, informadas por conversas com a IA. O humano decide, a IA executa e valida.
