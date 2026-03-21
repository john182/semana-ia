---
name: dotnet-implementation
description: Gera e refatora implementações .NET seguindo SOLID, Clean Code, baixo acoplamento, nomes expressivos, patterns com pragmatismo e sem overengineering
---

# Objetivo

Criar implementações em C#/.NET com foco em:
- SOLID
- Clean Code
- baixo acoplamento
- alta coesão
- nomes expressivos
- classes especializadas
- simplicidade
- evitar overengineering
- uso pragmático de design patterns
- eliminação de valores mágicos

# Quando usar esta skill

Use esta skill quando:
- for necessário implementar uma nova funcionalidade em C#
- for necessário refatorar código existente
- for necessário melhorar nomes, responsabilidades e dependências
- houver necessidade de reduzir acoplamento
- houver necessidade de reorganizar classes e métodos
- for importante manter consistência arquitetural no projeto

# Regras obrigatórias

Ao gerar implementações, seguir obrigatoriamente estas regras:

- Seguir SOLID com pragmatismo
- Seguir Clean Code sem criar abstrações desnecessárias
- Preferir baixo acoplamento e alta coesão
- Usar nomes orientados ao domínio
- Métodos devem ter nomes expressivos
- Variáveis, propriedades e classes devem revelar intenção
- Métodos privados devem ficar no final da classe
- Não criar código excessivamente genérico
- Não criar padrões apenas por estética
- Evitar valores mágicos no código
- Extrair constantes quando isso melhorar clareza e manutenção

# Simplicidade e pragmatismo

- Evitar overengineering
- Sempre preferir a solução mais simples que resolva corretamente o problema atual
- Não antecipar cenários futuros hipotéticos sem necessidade real
- Não criar abstrações, interfaces, factories, strategies, orchestrators ou handlers sem necessidade concreta
- Não transformar regras simples em arquiteturas complexas
- Não dividir demais o código em muitas classes pequenas sem ganho claro de legibilidade, manutenção ou testabilidade
- Toda abstração deve ter justificativa real
- Antes de propor a solução, avaliar se ela está mais complexa do que o problema exige
- Se uma classe direta resolver o problema com clareza, não criar duas ou três
- Aplicar design patterns apenas quando resolverem um problema real de modelagem, manutenção, extensão ou acoplamento

# Arquitetura e design

- Não criar `UseCase` por padrão
- Para comportamentos específicos, preferir classes especializadas com nomes explícitos
- Para CRUD simples, pode usar `Service` quando fizer sentido
- O nome da classe deve deixar clara a responsabilidade
- Não usar classes genéricas demais como:
  - `Helper`
  - `Utils`
  - `Manager`
  - `CommonService`
  - `Processor`
  - `Handler`
  - `GenericService`
- Só usar interface quando houver necessidade real de:
  - variação de comportamento
  - contrato claro
  - desacoplamento justificado
  - testabilidade com ganho real
- Não criar interface por obrigação ou convenção vazia
- Preferir composição simples a hierarquias desnecessárias
- Aplicar patterns com pragmatismo, não como checklist teórico
- Quando um pattern melhorar clareza, extensibilidade ou reduzir acoplamento, ele pode ser aplicado
- Quando uma implementação direta for suficiente, preferi-la

# Nomenclatura

- Usar nomes orientados ao domínio
- O nome deve explicar o papel da classe sem exigir leitura da implementação
- Métodos devem expressar intenção
- Evitar abreviações desnecessárias
- Evitar nomes vagos como:
  - `Process`
  - `Execute`
  - `Do`
  - `Handle`
  - `Run`
    quando houver nome mais específico
- Exemplos desejados:
  - `VisaBrandParser`
  - `MastercardBrandParser`
  - `CustomerDocumentNormalizer`
  - `InvoiceXmlBuilder`
  - `ProductInvoiceLookupClient`
  - `PaymentStatusResolver`

# Constantes e valores mágicos

- Não usar magic numbers quando o valor tiver significado de negócio, regra, limite, código, tamanho, status, versão ou comportamento relevante
- Não repetir strings relevantes em múltiplos pontos do código quando houver risco de mudança ou impacto de manutenção
- Quando uma string literal aparecer em mais de um lugar e representar regra, chave, código, nome técnico, rota, status, tipo, header, claim, campo, namespace, tag XML ou valor de comparação, extrair para constante
- Quando um número tiver significado semântico, extrair para constante nomeada
- O nome da constante deve revelar claramente o propósito do valor
- Preferir constantes privadas na própria classe quando o valor for local à implementação
- Promover a constante para escopo compartilhado apenas quando houver uso real em mais de uma classe
- Não extrair constante para valores triviais sem ganho de clareza
- Não criar arquivo de constantes genérico
- Agrupar constantes por contexto de domínio ou responsabilidade, evitando classes do tipo `Constants` genéricas demais

## Exemplos de extração desejada

- strings repetidas como nomes de status, headers, claims, tipos de documento, nomes de elementos XML, mensagens reutilizadas ou chaves de dicionário
- números como limites, tamanhos máximos, códigos fixos, timeouts, versões, fatores, casas decimais relevantes ou quantidades de negócio
- valores usados em mais de um branch condicional ou em mais de um método da mesma classe

## Exemplos a evitar

- extrair constante para um valor usado uma única vez sem significado semântico relevante
- criar constante apenas para “seguir regra” quando isso piorar a legibilidade
- criar classe global `Constants` ou `MagicValues` sem contexto de domínio

# Acoplamento e dependências

- Cada classe deve depender apenas do que realmente usa
- Não injetar dependências desnecessárias
- Evitar acoplamento entre domínio, aplicação e infraestrutura
- Não misturar regras de negócio com detalhes de infraestrutura quando isso prejudicar clareza e manutenção
- Não criar serviços inchados com múltiplas responsabilidades
- Extrair responsabilidades apenas quando isso melhorar clareza, teste ou manutenção

# Métodos

- Métodos devem ser curtos e ter responsabilidade única
- Métodos privados devem ficar no final da classe
- Métodos públicos primeiro, privados por último
- Extrair blocos complexos para métodos privados somente quando isso melhorar legibilidade
- Não quebrar um fluxo simples em métodos demais sem ganho real
- Evitar parâmetros ambíguos
- Evitar booleanos de controle quando houver alternativa mais clara
- Usar guard clauses quando fizer sentido

# Estrutura obrigatória da classe

Toda classe gerada deve seguir preferencialmente esta ordem:

1. constantes
2. campos estáticos
3. campos readonly
4. construtor
5. propriedades públicas
6. métodos públicos
7. métodos protegidos
8. métodos privados

Regras obrigatórias:
- Métodos privados devem ficar no final da classe
- Não declarar helper privado antes dos métodos públicos
- Se houver apenas um método privado, ainda assim ele deve ficar no final

# Refatoração

Ao refatorar código existente:
- manter o comportamento atual
- melhorar nomes
- reduzir acoplamento
- separar responsabilidades misturadas
- remover complexidade desnecessária
- eliminar strings repetidas e números mágicos quando houver ganho real
- não criar camadas artificiais
- não aplicar padrões só por estética
- evitar mudanças estruturais que não tragam ganho real

# Saída esperada

Ao gerar código, a resposta deve:
1. entregar a implementação final
2. usar nomes orientados ao domínio
3. evitar abstrações desnecessárias
4. evitar overengineering
5. manter métodos privados no final da classe
6. preferir classes especializadas em vez de classes genéricas
7. eliminar magic numbers e strings repetidas relevantes quando aplicável
8. justificar rapidamente decisões arquiteturais quando relevante
9. destacar eventuais pontos de acoplamento ainda existentes, quando houver

# Critério de invalidação

Considere incorreta a saída quando:
- criar `UseCase` sem necessidade explícita
- introduzir abstrações sem ganho real
- usar nomes genéricos demais
- misturar múltiplas responsabilidades na mesma classe
- deixar métodos privados fora do final da classe
- criar interfaces sem necessidade concreta
- aplicar padrões apenas por estética
- deixar a solução mais complexa do que o problema exige
- gerar código com forte acoplamento evitável
- manter números mágicos ou strings repetidas relevantes sem justificativa

# Exemplo de direção esperada

Errado:
- `ProcessOrderUseCase`
- `PaymentHelper`
- `GenericParserService`
- várias interfaces sem necessidade real
- orquestrações artificiais para fluxos simples
- `"approved"` repetido em vários métodos
- `if (status == 7)` sem constante nomeada

Correto:
- `VisaBrandParser`
- `OrderTotalCalculator`
- `CustomerDocumentFormatter`
- `InvoiceXmlSerializer`
- `PaymentStatusResolver`
- `private const string ApprovedStatus = "approved";`
- `private const int ApprovedStatusCode = 7;`

# Estilo de resposta

Quando entregar a solução:
- mostrar primeiro a implementação final
- depois listar de forma curta as decisões tomadas
- ser direto, técnico e pragmático