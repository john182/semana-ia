---
description: Executa uma change aprovada com orquestração técnica por agentes, priorizando reutilização, centralização, testes, uso complementar de MCP de spec e revisão final
argument-hint: <change-name>
---

# Objetivo

Executar uma change já aprovada de forma disciplinada, usando orquestração por agentes especializados, respeitando a spec, a arquitetura existente, os padrões do projeto e as skills configuradas.

## Política de reutilização
Antes de criar qualquer método auxiliar novo, verificar se já existe implementação equivalente no projeto.
Se existir, reutilizar ou consolidar.
Se não existir, criar um único ponto central apropriado ao domínio.
É proibido criar duplicações locais por conveniência.

A execução deve priorizar:

- aderência à spec
- implementação mínima necessária
- reutilização antes de criação
- centralização de comportamento compartilhável
- cobertura de testes
- revisão técnica final

# Entrada obrigatória

Receba o nome da change em `$ARGUMENTS`.

Se o nome da change não for informado, interrompa e peça explicitamente o identificador correto da change.

# Regra principal de execução

Não trate a execução como um fluxo genérico de um único agente.

A execução deve ser conduzida como uma orquestração entre agentes especializados, cada um responsável por uma etapa específica do trabalho.

# Uso do MCP de spec

Quando o MCP de spec estiver disponível no projeto, ele deve ser usado como apoio complementar principalmente na etapa de entendimento da change.

Regras:
- o MCP não substitui os arquivos oficiais da change
- proposal, tasks e spec local continuam sendo a fonte principal
- o MCP deve ser usado para enriquecer contexto, não para sobrescrever a change
- se houver divergência entre o MCP e os arquivos versionados da change, priorizar os arquivos versionados
- a execução não deve falhar apenas porque o MCP não está disponível

# Fluxo obrigatório

## Etapa 1 — Entendimento da mudança

Use o `spec-agent` para:

- localizar a change informada
- ler proposal, tasks, spec e contexto relacionado
- usar o MCP de spec como apoio complementar, quando disponível
- resumir o objetivo da mudança
- listar critérios de aceite
- identificar riscos, dependências e lacunas
- propor a ordem técnica de execução

O resultado dessa etapa deve deixar claro:

1. o que precisa ser implementado
2. o que não faz parte do escopo
3. quais arquivos ou áreas tendem a ser impactados
4. quais testes serão necessários
5. se existe impacto em XML/schema/serialização
6. se o MCP foi usado como apoio ou não

Antes de seguir para implementação, consolide esse entendimento.

---

## Etapa 2 — Implementação de produção

Use o `implementation-agent` para implementar a mudança.

A implementação deve seguir estritamente as skills e padrões do projeto.

### Regras obrigatórias da implementação

- Fazer apenas as alterações necessárias para entregar a change.
- Respeitar a arquitetura existente.
- Seguir SOLID com pragmatismo.
- Seguir Clean Code sem overengineering.
- Preferir classes especializadas com nomes orientados ao domínio.
- Evitar nomes genéricos como `Helper`, `Utils`, `Manager`, `Processor`, `CommonService`.
- Não criar `UseCase` por padrão.
- Métodos privados devem ficar no final da classe.
- Extrair constantes para strings repetidas relevantes.
- Não deixar números mágicos.
- Quando houver conjunto finito de valores, preferir `enum`.
- Preservar comportamento existente, salvo quando a própria change exigir mudança funcional.

### Regras obrigatórias de reutilização e centralização

Antes de criar qualquer método auxiliar para formatação, normalização, parsing, conversão, limpeza ou composição de valores:

- procurar implementação equivalente já existente no projeto
- reutilizar o que já existe sempre que possível
- evitar criação de implementação paralela apenas por conveniência local

Não duplicar comportamentos recorrentes como:

- CEP
- telefone
- documento
- máscara
- datas
- textos
- identificadores
- códigos
- normalizações equivalentes

Não espalhar lógica compartilhada em:

- builders
- services
- validators
- mappers
- converters
- handlers

Se existir implementação semelhante mas incompleta para o novo cenário:

- preferir refatorar e consolidar
- não criar outra versão paralela

Evitar múltiplas variações do mesmo comportamento, como por exemplo:

- `FormatCep`
- `NormalizeCep`
- `ApplyCepMask`
- `FormatPhone`
- `NormalizePhone`
- `CleanDocument`

quando tudo isso representar a mesma responsabilidade.

Só criar um novo ponto central quando realmente não existir nada reutilizável no projeto.

Se precisar criar esse ponto central:

- usar nome coeso
- usar nome específico
- usar nome orientado ao domínio
- evitar componente genérico

### Regras obrigatórias de escopo

- Não alterar arquivos sem necessidade.
- Não fazer refatoração ampla se ela não estiver diretamente conectada à change.
- Não introduzir abstrações novas sem justificativa concreta.
- Não misturar regras de domínio com infraestrutura sem necessidade.
- Não seguir por caminho mais complexo do que o problema exige.

Ao final da implementação, consolide:

1. o que foi alterado
2. por que foi alterado
3. quais pontos foram reutilizados
4. quais centralizações foram feitas ou preservadas

---

## Etapa 3 — Testes unitários

Use o `unit-test-agent` para criar ou ajustar os testes unitários necessários.

### Regras obrigatórias dos testes unitários

- Usar xUnit.
- Usar Shouldly.
- Nomear testes no padrão `Given_<context>_Should_<expected_behavior>`.
- Sempre usar Arrange / Act / Assert.
- Cada teste deve cobrir um único comportamento.
- Priorizar:
  - regras de negócio
  - cenários mínimos
  - edge cases
  - fluxos de erro
  - nulos
  - condicionais relevantes

Os testes devem cobrir o comportamento alterado, e não apenas detalhes internos sem valor.

Ao final, consolidar:

1. testes criados ou ajustados
2. cenários cobertos
3. lacunas que permaneceram

---

## Etapa 4 — Testes XML e schema

Se a change impactar qualquer um dos itens abaixo:

- serialização XML
- builders XML
- output XML
- ordem de nós
- obrigatoriedade de nós
- comportamento condicional de XML
- aderência a schema/XSD

então use o `xml-test-agent`.

### Regras obrigatórias dos testes XML

- Validar presença e ausência de nós relevantes.
- Validar valores relevantes.
- Validar comportamento condicional.
- Validar estrutura do XML gerado.
- Quando aplicável, validar aderência ao schema/XSD.
- Priorizar comportamento do domínio e regras do schema, evitando testes frágeis sem valor.

Ao final, consolidar:

1. testes XML criados ou ajustados
2. cenários cobertos
3. validações de schema realizadas
4. lacunas restantes

Se não houver impacto em XML, registrar explicitamente que essa etapa não se aplica.

---

## Etapa 5 — Revisão técnica final

Use o `review-agent` para revisar a mudança antes de concluir.

A revisão deve funcionar como gate de qualidade.

### Itens obrigatórios da revisão

Verificar e sinalizar objetivamente:

- complexidade desnecessária
- naming ruim
- violação de Clean Code
- violação de padrão arquitetural
- números mágicos
- strings repetidas
- enums ausentes quando cabíveis
- risco de regressão
- quebra de contrato
- lacunas de teste
- alterações fora do escopo

### Itens obrigatórios de reutilização e centralização

Verificar e sinalizar objetivamente:

- criação de métodos auxiliares duplicados
- lógica repetida para CEP, telefone, documento, máscara, datas, textos, identificadores e códigos
- criação de novo método local em vez de reutilização de implementação existente
- múltiplas variações do mesmo comportamento espalhadas em classes diferentes
- duplicação de lógica em builders, services, validators, mappers, converters ou handlers
- ausência de centralização quando havia comportamento compartilhável recorrente
- criação de implementação paralela quando já existia ponto reutilizável no projeto

A revisão final deve consolidar:

1. o que está bom
2. problemas encontrados
3. correções recomendadas
4. veredito final

---

# Regras globais obrigatórias

Durante toda a execução:

- não inventar escopo novo
- não ignorar a spec
- não concluir com pendências escondidas
- não criar duplicação evitável
- não criar utilitários paralelos por conveniência
- não criar abstrações genéricas sem necessidade
- não deixar a implementação mais complexa do que o problema exige

Sempre priorizar:

1. entendimento correto da change
2. implementação mínima necessária
3. reutilização antes de criação
4. centralização de comportamento compartilhável
5. testes coerentes
6. revisão técnica forte

# Forma de resposta esperada

Ao final da execução, apresente um resumo estruturado contendo:

## 1. Entendimento da change
- objetivo
- critérios de aceite
- escopo aplicado
- uso ou não do MCP como apoio

## 2. Implementação realizada
- arquivos alterados
- decisões técnicas principais
- pontos de reutilização
- pontos de centralização

## 3. Testes unitários
- testes criados/ajustados
- cenários cobertos

## 4. Testes XML/schema
- testes criados/ajustados
- validações realizadas
- ou justificativa de não aplicabilidade

## 5. Revisão final
- problemas encontrados
- ajustes recomendados
- veredito final

## Etapa opcional — Documentação funcional

Se a change:
- introduzir funcionalidade nova
- alterar regra de negócio
- modificar fluxo operacional
- alterar contrato funcional
- impactar suporte, QA ou onboarding

então usar o `product-doc-agent` para gerar ou atualizar a documentação funcional da feature.

Após isso, usar o `product-doc-review-agent` para revisar a documentação gerada.

A documentação deve:
- refletir o comportamento real da change
- explicitar fluxo principal, regras, validações, exceções e integrações
- listar lacunas quando houver incerteza
- incluir diagramas Mermaid quando agregarem clareza

# Critério de conclusão

Só considerar a execução concluída quando:

- a spec tiver sido respeitada
- a implementação tiver sido aplicada
- os testes necessários tiverem sido criados ou ajustados
- a etapa de XML/schema tiver sido tratada quando aplicável
- a revisão técnica final tiver sido executada
- não houver duplicação evitável ignorada
- não houver criação indevida de métodos auxiliares paralelos quando já existia comportamento reutilizável no projeto