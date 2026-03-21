---
name: write-dotnet-xml-serializer-tests
description: Escreve testes de serializer XML em C# com validação funcional e estrutural contra XSD
---

# Objetivo

Gerar testes para serializers XML em C#/.NET seguindo estritamente o padrão oficial do projeto, com foco em:
- validade estrutural do XML
- aderência ao schema XSD
- clareza
- baixo boilerplate
- legibilidade
- cobertura de regras de negócio do serializer
- reutilização correta de fixtures, builders e helpers
- evitar overengineering

# Quando usar esta skill

Use esta skill quando:
- for necessário criar testes para serializers XML
- for necessário validar XML contra XSD
- o código gerar documentos XML estruturados
- houver regras condicionais que alterem nós, atributos ou blocos do XML
- houver necessidade de validar conteúdo e estrutura ao mesmo tempo

# Regras obrigatórias

Ao gerar testes, seguir obrigatoriamente estas regras:

- Usar xUnit
- Usar Shouldly para todas as asserções
- Não usar `Assert.*`, exceto se o usuário pedir explicitamente
- Nomear os testes obrigatoriamente no padrão:

`Given_<context>_Should_<expected_behavior>`

- Organizar todos os testes com comentários explícitos de:
    - Arrange
    - Act
    - Assert
- Cada teste deve validar um único comportamento principal
- Todo teste relevante que gere XML deve validar:
    1. comportamento esperado
    2. conteúdo relevante do XML
    3. validade estrutural contra os XSDs aplicáveis

# Regra mandatória de schema

Para testes de serializer XML:

- Não basta existir um único teste separado validando XSD
- Cada teste que chama o serializer e gera XML deve validar também o schema
- Não considerar suficiente um teste genérico como `Should_BeValidAgainstSchema` isolado
- A validação contra XSD faz parte da definição de pronto do teste
- Não considerar o teste concluído enquanto o XML gerado não estiver validado contra os XSDs aplicáveis no próprio cenário testado

# Regras obrigatórias de XML

- Não validar apenas nós isolados sem validar o schema
- Não validar apenas o schema sem validar o conteúdo relevante
- Validar também presença, ausência, escolha ou ordem lógica dos blocos mais importantes quando fizer sentido
- Quando houver mais de um XSD relacionado, carregar o conjunto completo necessário para validação
- A validação XSD deve ser reutilizável e não copiada manualmente em cada teste

# Regra obrigatória de organização da classe de teste

Toda classe de teste deve seguir esta ordem:

1. constantes
2. campos estáticos
3. campos readonly
4. construtor
5. testes `[Fact]` e `[Theory]`
6. métodos privados/helpers

Regras obrigatórias:
- Nenhum método privado deve aparecer antes dos testes
- Todo helper privado deve ficar no final da classe
- Isso inclui helpers de parse XML, helpers de namespace, helpers de assertion e métodos utilitários privados
- Se houver apenas um helper privado, ainda assim ele deve ficar no final da classe

# Estratégia de criação de massa de teste

Seguir esta ordem de preferência:

1. montagem simples inline, quando o objeto for pequeno e o teste continuar claro
2. Fixture, quando houver cenário base reutilizável
3. Builder, quando houver muitas variações sobre o mesmo documento
4. Bogus, apenas quando reduzir boilerplate sem prejudicar clareza

# Quando usar Fixture

Usar Fixture quando:
- existir um documento base válido reutilizado por vários testes
- o cenário mínimo válido for recorrente
- houver necessidade de um cenário completo conhecido
- a intenção for centralizar massa base do documento

A Fixture deve expor nomes claros como:
- `CreateValidMinimal()`
- `CreateValidDefault()`
- `CreateComplete()`

# Quando usar Builder

Usar Builder quando:
- houver muitas variações sobre o mesmo documento base
- o documento possuir muitos campos opcionais
- houver cenários como:
    - CPF vs CNPJ
    - nacional vs estrangeiro
    - presença vs ausência de blocos opcionais
    - totais por valor vs percentual
    - escolhas condicionais do schema

# Regras para Builder

O Builder deve:
- criar um documento válido por padrão
- permitir sobrescrita incremental
- retornar `this`
- expor métodos orientados ao domínio
- esconder complexidade de inicialização
- evitar nomes genéricos como `SetX`, `SetY`

Exemplos desejados:
- `WithCpfBorrower()`
- `WithCnpjBorrower()`
- `WithForeignBorrower()`
- `WithNationalAddress()`
- `WithForeignAddress()`
- `WithIntermediary()`
- `WithApproximateTotalsByAmount()`
- `WithApproximateTotalsByRate()`

# Quando usar Bogus

Usar Bogus apenas quando fizer sentido real, por exemplo:
- houver muitos campos irrelevantes para o comportamento testado
- houver coleções ou partes extensas de apoio
- o uso de dados artificiais reduzir boilerplate sem esconder a intenção do cenário

Ao usar Bogus:
- manter previsibilidade
- sobrescrever explicitamente os campos relevantes do cenário
- não depender de aleatoriedade para validar regra de negócio

# Estratégia de implementação para XML

Ao gerar testes para serialização XML:

- criar helper reutilizável para validação do XML contra XSD
- evitar duplicar lógica de carregamento de schemas em cada teste
- preferir uma assertion reutilizável, por exemplo:
    - `result.Xml.ShouldBeValidAgainstSchema(...)`
    - `result.Xml.ShouldBeValidAgainstDpsSchema()`
    - ou helper equivalente do projeto
- criar helpers reutilizáveis para parse do XML somente quando isso melhorar legibilidade
- manter esses helpers privados no final da classe

# Estratégia para cenários

Ao gerar testes, priorizar cobertura destes grupos:

## 1. Cenário mínimo válido
Validar o fluxo mínimo necessário para gerar XML válido.

## 2. Cenário completo
Validar um preenchimento completo com todos ou quase todos os blocos opcionais relevantes.

## 3. Cenários condicionais
Validar regras como:
- CPF vs CNPJ
- endereço nacional vs estrangeiro
- presença vs ausência de blocos opcionais
- total por valor vs total por percentual
- escolhas condicionais do schema
- valores nulos, ausentes ou default quando aplicável

## 4. Regras de negócio
Validar especificamente as regras que alteram a saída, os nós, atributos, blocos ou escolhas do serializer.

# Saída esperada

Ao gerar testes, a resposta deve produzir:

1. testes com nomes no padrão `Given_<context>_Should_<expected_behavior>`
2. uso de `Shouldly`
3. comentários explícitos de `Arrange`, `Act` e `Assert`
4. validação de conteúdo do XML
5. validação estrutural por XSD em cada teste relevante
6. uso de Fixture e/ou Builder quando houver documento complexo
7. uso de Bogus apenas quando necessário
8. baixo boilerplate
9. testes legíveis e focados no comportamento
10. helpers privados no final da classe

# O que evitar

Evitar:
- validar apenas nós do XML sem validar o schema
- validar apenas um teste genérico de schema e considerar isso suficiente
- duplicar manualmente a lógica de XSD em todos os testes
- helpers privados antes dos testes
- builders genéricos demais
- uso desnecessário de Bogus
- excesso de abstração na infraestrutura do teste

# Critério de invalidação

Considere incorreta a saída quando:
- usar `Assert.*`
- não usar o padrão `Given_<context>_Should_<expected_behavior>`
- não houver `Arrange / Act / Assert`
- um teste relevante de serializer gerar XML sem validar XSD no mesmo cenário
- a validação XSD existir apenas em um teste isolado e não nos demais cenários
- houver método privado antes dos testes
- a estrutura do teste estiver mais complexa do que o comportamento validado exige

# Exemplo correto de teste

```csharp
[Fact]
public void Given_MinimalDocument_Should_GenerateValidDpsStructure()
{
    // Arrange
    var document = DpsDocumentTestFixture.CreateValidMinimal();

    // Act
    var result = _sut.Serialize(document);

    // Assert
    result.Xml.ShouldBeValidAgainstDpsSchema();

    var root = XDocument.Parse(result.Xml).Root!;
    root.Name.LocalName.ShouldBe("DPS");
}