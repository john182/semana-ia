---
name: technical-review
description: Revisa tecnicamente a implementação seguindo estritamente os padrões oficiais do projeto
---

# Objetivo

Revisar a implementação e apontar, de forma objetiva e acionável, problemas de qualidade, padronização, duplicação, riscos de regressão e violações dos padrões do projeto.

# Regras obrigatórias

- Revisar com foco em aderência real ao padrão do projeto.
- Priorizar problemas concretos em vez de sugestões cosméticas.
- Ser objetivo, técnico e acionável.
- Não sugerir mudanças sem ganho claro.
- Verificar se a solução está mais complexa do que o problema exige.
- Verificar se nomes estão coerentes com o domínio.
- Verificar se houve violação de Clean Code, SOLID com pragmatismo, coesão e baixo acoplamento.
- Verificar se métodos privados ficaram no final da classe.
- Verificar se strings repetidas relevantes deveriam ser constantes.
- Verificar se existem números mágicos.
- Verificar se campos com conjunto finito de valores deveriam ser `enum`.
- Verificar se a arquitetura do projeto foi respeitada.
- Verificar se existem testes suficientes para a mudança.
- Quando houver XML, verificar se os testes cobrem estrutura, obrigatoriedade, comportamento condicional e schema quando aplicável.

# Regras obrigatórias de reutilização e centralização

- Verificar se foram criados métodos auxiliares duplicados para formatação, normalização, parsing, conversão, limpeza ou composição de valores.
- Apontar quando existir lógica repetida para CEP, telefone, documento, máscaras, datas, textos, identificadores, códigos ou comportamentos equivalentes.
- Sinalizar quando o código criou novo método local em vez de reutilizar ou consolidar implementação já existente.
- Sinalizar quando houver múltiplas variações do mesmo comportamento espalhadas em classes diferentes.
- Verificar se o agente criou métodos locais por conveniência em vez de buscar o ponto central já existente.
- Recomendar centralização quando houver comportamento compartilhável repetido em mais de um ponto.
- Apontar quando builders, services, validators, mappers, converters ou handlers passaram a carregar lógica duplicada que deveria estar centralizada.
- Verificar se a solução introduziu mais uma versão paralela de algo que já existia no projeto.
- Quando houver comportamento recorrente reutilizável, recomendar reutilização ou refatoração para consolidação em vez de duplicação.

# Saída esperada

A revisão deve retornar:

1. O que está bom
2. Problemas encontrados
3. Correções recomendadas
4. Lacunas de teste ou risco
5. Veredito final

# Critérios de reprovação

A revisão deve reprovar ou sinalizar fortemente quando encontrar:

- duplicação de regra de negócio
- criação de métodos auxiliares redundantes
- lógica compartilhada espalhada em vários pontos
- complexidade desnecessária
- nomes genéricos ou pouco orientados ao domínio
- ausência de reutilização quando já havia implementação semelhante
- regressão de padrão arquitetural
- lacunas importantes de teste