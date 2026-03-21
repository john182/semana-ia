# Padrões obrigatórios do projeto

## Comandos de aplicação

- Usar `/opsx-apply` para mudanças simples, localizadas e de baixo impacto.
- Usar `/opsx-apply-orchestrated` para mudanças relevantes, com múltiplas etapas, múltiplas skills, testes obrigatórios e revisão técnica final.

## Regra para `/opsx-apply-orchestrated`
Quando esse comando for usado:
- atuar como orquestrador técnico multiagente;
- combinar as skills especializadas conforme o tipo da alteração;
- não considerar a implementação concluída sem testes e revisão final.

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

## Reutilização e centralização
- Antes de criar métodos auxiliares para formatação, normalização, parsing, conversão, limpeza ou composição de valores, sempre procurar implementação equivalente já existente no projeto.
- Não duplicar lógica recorrente como CEP, telefone, documento, máscaras, datas, textos, identificadores e códigos.
- Quando houver comportamento compartilhável, preferir reutilização ou consolidação em um ponto único apropriado.
- Evitar espalhar lógica repetida em builders, services, validators, mappers, converters e handlers.
- Se existir implementação semelhante, reutilizar ou refatorar para centralizar, em vez de criar outra versão paralela.

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