---
name: dotnet-implementation
description: Implementa código C#/.NET seguindo estritamente o padrão oficial do projeto
---

# Objetivo

Gerar ou refatorar código C#/.NET de produção seguindo estritamente os padrões do projeto, com foco em clareza, coesão, reutilização, domínio e baixo acoplamento.

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
- Antes de finalizar, revisar se a implementação introduziu duplicação, complexidade desnecessária ou violação de padrão do projeto.

# Saída esperada

Ao implementar:
1. gerar código pronto para uso
2. manter aderência aos padrões do projeto
3. minimizar impacto desnecessário
4. reutilizar o que já existe antes de criar novas abstrações
5. explicar de forma objetiva qualquer decisão técnica relevante