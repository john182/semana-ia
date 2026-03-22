---
name: dotnet-implementation
description: Implementa código C#/.NET seguindo estritamente o padrão oficial do projeto
---

# Objetivo

Gerar ou refatorar código C#/.NET de produção seguindo estritamente os padrões do projeto, com foco em clareza, coesão, reutilização, domínio, baixo acoplamento e aderência total à arquitetura já adotada pelo projeto.

# Regras obrigatórias

- Sempre seguir SOLID com pragmatismo.
- Sempre seguir Clean Code sem overengineering.
- Preferir classes especializadas com nomes orientados ao domínio.
- Não criar `UseCase` por padrão.
- Para CRUD simples, pode usar `Service` quando fizer sentido e o nome for específico.
- Evitar classes genéricas como `Helper`, `Utils`, `Manager`, `Processor`, `CommonService`.
- Evitar abstrações sem necessidade real.
- Evitar interfaces sem justificativa concreta.
- Preferir baixo acoplamento e alta coesão.
- Métodos privados devem ficar no final da classe.
- Antes de propor a solução, avaliar se ela está mais complexa do que o problema exige.
- Sempre que houver strings repetidas relevantes, extrair constantes.
- Não deixar números mágicos; extrair constantes ou tipos apropriados.
- Sempre que um campo possuir conjunto finito de valores possíveis, preferir `enum`.
- Preservar o comportamento existente em refatorações, salvo quando a mudança solicitada exigir alteração funcional explícita.

# Regras obrigatórias de aderência arquitetural

- Antes de implementar, identificar explicitamente qual arquitetura o projeto já utiliza.
- A implementação deve seguir a arquitetura já existente no projeto, e não a arquitetura que o agente considera ideal.
- Se o projeto usar Onion Architecture, manter Onion Architecture.
- Se o projeto usar Arquitetura Hexagonal, manter Arquitetura Hexagonal.
- Se o projeto usar MVC, manter MVC.
- Se o projeto usar Clean Architecture, manter Clean Architecture.
- Se o projeto usar modularização por feature, manter a organização por feature.
- Se o projeto usar organização por camada técnica, manter a organização por camada técnica.
- Não introduzir um novo estilo arquitetural em um projeto que já possui padrão estabelecido.
- Não misturar padrões arquiteturais incompatíveis sem solicitação explícita.
- Não transformar projeto em Onion, Hexagonal, MVC, Clean Architecture, Vertical Slice, CQRS ou qualquer outro estilo se isso não fizer parte da arquitetura já adotada.
- Respeitar a separação de camadas, módulos, diretórios, dependências e convenções já presentes no projeto.
- Respeitar os pontos de entrada e saída já definidos pela arquitetura atual.
- Respeitar a direção de dependências já adotada no projeto.
- Não mover código entre camadas sem necessidade real.
- Não criar novas camadas, novos módulos ou novos projetos sem necessidade clara e sem alinhamento com a estrutura já existente.
- Não introduzir abstrações arquiteturais novas apenas porque seriam “mais corretas” teoricamente.
- Se a arquitetura atual tiver limitações, ainda assim manter coerência com ela, salvo solicitação explícita de evolução arquitetural.
- Só propor alteração arquitetural quando isso for pedido explicitamente pelo usuário.
- Quando houver dúvida sobre a arquitetura dominante do projeto, primeiro inferir pela estrutura existente de pastas, namespaces, dependências, convenções e tipos já implementados.
- Em caso de dúvida entre duas interpretações possíveis, preferir a abordagem que menos altera a arquitetura atual do projeto.
- Componentes novos devem nascer no mesmo estilo arquitetural dos componentes equivalentes já existentes.
- Classes, interfaces, handlers, services, controllers, repositories, use cases, presenters, adapters, gateways, mappers e validators devem ser posicionados de acordo com a arquitetura vigente no projeto, não com preferência pessoal do agente.

# Regras obrigatórias de leitura estrutural do projeto

- Antes de criar qualquer arquivo novo, localizar componentes equivalentes já existentes no projeto.
- Observar onde o projeto normalmente coloca:
    - controllers
    - services
    - handlers
    - repositories
    - validators
    - mappers
    - DTOs
    - entities
    - value objects
    - adapters
    - gateways
    - casos de aplicação
- Repetir o padrão estrutural já adotado.
- Repetir o padrão de injeção de dependência já adotado.
- Repetir o padrão de composição de respostas, erros e validações já adotado.
- Repetir o padrão de organização de namespaces e pastas já adotado.
- Não inventar nova convenção de nomes, agrupamento ou separação de responsabilidade se já existir padrão claro no projeto.

# Regras obrigatórias de reutilização e centralização

- Antes de criar qualquer método auxiliar para formatação, normalização, parsing, conversão, limpeza ou composição de valores, procurar se já existe implementação equivalente no projeto.
- Não duplicar métodos para comportamentos recorrentes como CEP, telefone, documento, máscara, código postal, texto, números, datas, identificadores e normalizações semelhantes.
- Quando houver comportamento recorrente e reutilizável, centralizar em um ponto único apropriado, com nome orientado ao domínio.
- Não criar uma nova implementação local apenas para resolver uma necessidade pontual se já existir comportamento equivalente ou muito semelhante no projeto.
- Se já existir uma implementação semelhante, preferir reutilizar diretamente.
- Se a implementação existente não atender completamente ao novo cenário, preferir refatorar e consolidar em vez de criar outra versão paralela.
- Não espalhar lógica compartilhada em builders, services, validators, mappers, converters ou handlers.
- Evitar métodos locais duplicados como `FormatCep`, `NormalizeCep`, `ApplyCepMask`, `FormatPhone`, `NormalizePhone`, `CleanDocument` e equivalentes quando representarem o mesmo comportamento.
- Só criar um novo componente para esse tipo de lógica quando ficar claro que não existe ponto reutilizável já disponível no projeto.
- Quando precisar criar um ponto central novo, escolher nome coeso, específico e orientado ao domínio, evitando nomes genéricos.

# Regras obrigatórias de implementação

- Respeitar a arquitetura existente do projeto.
- Não mover responsabilidade para camadas inadequadas.
- Não misturar regras de domínio com detalhes de infraestrutura sem necessidade.
- Preferir métodos curtos e com responsabilidade única.
- Preferir nomes que expressem intenção.
- Evitar duplicação de lógica.
- Em fluxos de transformação, manter clareza entre entrada, processamento e saída.
- Quando houver mapeamento ou serialização, evitar acoplamento excessivo entre contrato externo e estrutura interna.
- Antes de finalizar, revisar se a implementação introduziu duplicação, complexidade desnecessária, violação arquitetural ou violação de padrão do projeto.

# Regras obrigatórias de decisão

- Sempre perguntar internamente: “como este projeto já resolve problemas parecidos?”
- A nova implementação deve parecer nativa do projeto.
- O código novo não deve parecer importado de outra arquitetura ou de outro estilo de projeto.
- Se existir componente equivalente, copiar o padrão estrutural e de dependência dele.
- Priorizar consistência com o projeto acima de preferência teórica de arquitetura.

# Saída esperada

Ao implementar:
1. gerar código pronto para uso
2. manter aderência aos padrões do projeto
3. manter aderência total à arquitetura existente
4. minimizar impacto desnecessário
5. reutilizar o que já existe antes de criar novas abstrações
6. explicar de forma objetiva qualquer decisão técnica relevante

## Naming obrigatório

- Nunca usar nomes genéricos ou de uma letra para parâmetros, variáveis locais, métodos privados e resultados intermediários, exceto em lambdas extremamente curtas e óbvias.
- Parâmetros devem ter nome semântico completo, representando claramente o papel no contexto.
- Métodos privados devem ter nomes que expressem claramente intenção e resultado.
- Variáveis intermediárias devem receber nomes que expliquem o conteúdo ou propósito, evitando expressões complexas inline quando isso prejudicar legibilidade.

### Exemplos proibidos
- `r`
- `x`
- `obj`
- `data`
- `value`
- `item`
- `res`
- `result` quando houver um nome mais específico
- métodos como `Gap`, `Build`, `Handle`, `Process` sem contexto suficiente

### Exemplos esperados
- `serializationResult`
- `firstValidationError`
- `providerRuleProfile`
- `schemaAnalysisResult`
- `BuildValidationGapSummary`
- `ResolveFirstValidationMessage`
- `CreateProviderRuntimeSummary`