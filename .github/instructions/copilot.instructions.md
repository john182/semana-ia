# Padrões obrigatórios para o GitHub Copilot

## Idioma
- Todas as respostas, reviews e comentários devem ser escritos em português do Brasil.
- Nunca escrever comentários de review em inglês.
- Mesmo que o código-fonte, PR ou issue estejam em inglês, responder em português do Brasil.

## Objetivo do review
- Priorizar bugs, regressões, riscos de produção e violações de regra de negócio.
- Priorizar problemas reais e acionáveis.
- Evitar comentários superficiais ou genéricos.

## Regras do review
- Não inventar problemas sem evidência no diff.
- Não focar em estilo já coberto por formatter ou linter.
- Não sugerir refatorações grandes sem ganho claro.
- Verificar se a implementação está mais complexa do que o problema exige.
- Verificar se há duplicação desnecessária.
- Verificar se os nomes estão claros e orientados ao domínio.
- Verificar se a mudança respeita a arquitetura do projeto.
- Verificar impactos em segurança, performance, concorrência e observabilidade.
- Verificar se testes cobrem os comportamentos alterados.

## Formato obrigatório dos comentários
Ao apontar um problema, usar:
- Problema:
- Impacto:
- Sugestão:

## Estilo de escrita
- Escrever de forma objetiva, técnica e curta.
- Evitar elogios genéricos.
- Evitar comentários vagos como "talvez melhorar isso".
- Quando possível, sugerir correção concreta.
