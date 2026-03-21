# Padrões obrigatórios do projeto

## Implementação
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

## Testes unitários
- Usar xUnit.
- Usar Shouldly.
- Nomear testes no padrão `Given_<context>_Should_<expected_behavior>`.
- Sempre usar `Arrange / Act / Assert`.
- Cada teste deve validar um único comportamento principal.
- Evitar overengineering também nos testes.
- Usar Fixture, Builder, Bogus e Moq com pragmatismo.
- Helpers privados de teste devem ficar no final da classe.

## Testes de serializer XML
- Quando o foco for serializer XML, usar a skill específica de testes de XML.
- Em testes de XML, validar schema XSD junto com o cenário testado.
- Não considerar suficiente apenas um teste isolado de schema.
- Todo teste relevante que gere XML deve validar conteúdo e estrutura.