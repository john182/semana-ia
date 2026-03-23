---
name: product-documentation-generator
description: Gera documentação de produto clara, estruturada e acionável, alinhada ao contexto real do sistema e útil para negócio, suporte, QA e desenvolvimento
---

# Objetivo

Gerar documentação de produto com foco em clareza funcional, alinhamento com a regra de negócio real e utilidade prática para diferentes públicos do time.

A documentação deve explicar o que a funcionalidade é, por que existe, como funciona, quais regras possui, quais fluxos cobre, quais exceções existem, quais dependências possui e quais impactos causa no sistema.

# Quando usar

Usar esta spec quando for necessário:
- documentar uma funcionalidade nova
- documentar um módulo existente
- registrar comportamento funcional antes de refatoração
- criar material para onboarding de time
- produzir base para suporte, QA, produto e desenvolvimento
- consolidar regra de negócio dispersa em código, tickets e conhecimento tácito

# Princípios obrigatórios

- A documentação deve ser objetiva, útil e baseada no comportamento real do sistema.
- Não escrever texto genérico, abstrato ou com cara de marketing.
- Explicar a regra de negócio de forma concreta.
- Sempre priorizar comportamento, fluxo, exceções e impacto.
- Sempre deixar explícito o que é certeza e o que é suposição.
- Quando faltar contexto, sinalizar lacunas de informação.
- Não inventar regras de negócio.
- Não descrever apenas tela ou endpoint; documentar o comportamento funcional completo.
- Escrever para leitura humana, com linguagem clara e técnica na medida certa.
- A documentação deve poder ser usada por produto, suporte, QA e desenvolvimento.
- Sempre que possível, relacionar a funcionalidade com entradas, processamento e saídas.
- Sempre que possível, registrar restrições, validações, dependências e efeitos colaterais.
- Se houver risco de ambiguidade, preferir exemplos concretos.

# Estrutura obrigatória da documentação

A saída deve seguir esta estrutura.

## 1. Visão geral
Descrever:
- nome da funcionalidade
- objetivo de negócio
- problema que resolve
- valor para o usuário ou operação
- contexto dentro do produto

## 2. Escopo
Descrever:
- o que a funcionalidade cobre
- o que não cobre
- limites explícitos do comportamento
- premissas importantes

## 3. Perfis envolvidos
Descrever:
- quem usa
- quem é impactado
- perfis e responsabilidades
- diferenças de comportamento por perfil, tenant, permissão ou contexto

## 4. Fluxo funcional principal
Descrever o fluxo principal passo a passo, em ordem lógica, desde a entrada até a saída.

Deve conter:
- gatilho de início
- entradas esperadas
- decisões relevantes
- processamento principal
- resultado esperado
- persistência, integrações ou efeitos produzidos

## 5. Regras de negócio
Listar as regras de negócio de forma explícita.

Para cada regra, informar:
- nome curto da regra
- descrição objetiva
- condição
- comportamento esperado
- impacto no fluxo
- exceções, se existirem

Preferir estrutura como:
- Regra:
- Quando:
- Então:
- Observações:

## 6. Validações e restrições
Documentar:
- campos obrigatórios
- formatos esperados
- combinações inválidas
- limites de uso
- validações de permissão
- validações por status
- bloqueios de processamento

## 7. Exceções e cenários alternativos
Descrever:
- erros esperados
- cenários de rejeição
- comportamentos alternativos
- fallback
- reprocessamento
- idempotência, se aplicável
- mensagens ou efeitos para usuário e sistema

## 8. Entradas e saídas
Documentar:
- dados de entrada relevantes
- origem das entradas
- dados gerados
- saídas para usuário
- saídas para integrações
- alterações de estado
- eventos publicados
- arquivos, logs ou registros persistidos

## 9. Dependências e integrações
Descrever:
- módulos internos envolvidos
- serviços externos
- filas, banco, cache, storage, APIs
- dependências funcionais
- impacto caso uma dependência falhe

## 10. Impactos no sistema
Documentar:
- tabelas, coleções ou entidades afetadas
- campos alterados
- eventos gerados
- impactos em relatórios
- impactos em auditoria, rastreabilidade ou observabilidade
- efeitos para suporte e operação

## 11. Critérios de aceite
Listar critérios verificáveis e objetivos.

Cada critério deve ser testável e sem ambiguidade.

## 12. Exemplos práticos
Sempre que possível, incluir:
- exemplo de uso feliz
- exemplo de rejeição
- exemplo com exceção relevante
- exemplo de entrada e resultado esperado

## 13. Pontos de atenção
Registrar:
- riscos funcionais
- ambiguidades
- dependência de parametrização
- comportamento sensível por configuração
- impacto multi-tenant
- riscos operacionais ou fiscais, se aplicável

## 14. Lacunas identificadas
Quando houver incerteza, listar explicitamente:
- o que não foi possível confirmar
- o que precisa ser validado com negócio, suporte ou desenvolvimento
- o que depende de análise adicional

# Regras de qualidade da saída

- Não repetir conteúdo com palavras diferentes.
- Não usar frases vagas como “o sistema trata”, “o sistema gerencia”, “o usuário poderá”.
- Explicar exatamente o que acontece.
- Evitar adjetivos sem valor funcional.
- Priorizar listas estruturadas quando melhorar a leitura.
- Preferir exemplos concretos a descrições abstratas.
- Nomear conceitos com coerência de domínio.
- Se existir status, enumerar os status e seus significados.
- Se existir transição de estado, deixar explícita.
- Se existir parametrização, dizer quem configura, onde impacta e como altera o comportamento.
- Se existir comportamento por tenant, perfil ou ambiente, destacar isso.
- Se existir integração assíncrona, deixar claro onde começa e onde termina a responsabilidade da funcionalidade.
- Se existir processamento em lote, detalhar unidade de processamento, critério de sucesso parcial e reprocessamento.

# Instruções extras para o agente

Ao gerar a documentação:
1. identificar primeiro a funcionalidade central
2. separar fluxo principal de regras de negócio
3. explicitar exceções e validações
4. evitar documentação superficial orientada apenas à interface
5. incluir impactos técnicos somente quando forem relevantes para entendimento funcional
6. sinalizar toda suposição como suposição
7. organizar a resposta para leitura por humanos, não para impressionar
8. quando houver contexto insuficiente, gerar a documentação parcial e uma seção de lacunas
9. quando houver código, inferir comportamento com cautela e não assumir intenção sem evidência
10. quando houver endpoint, tela, job, consumidor ou workflow, documentar o processo ponta a ponta, não apenas o ponto de entrada

# Formato esperado da saída

A saída deve ser em Markdown, com títulos claros e seções bem definidas.

Sempre gerar:
- documentação principal
- lista final de dúvidas/lacunas
- lista final de critérios de aceite

# O que não fazer

- Não gerar texto promocional
- Não inventar decisão de produto
- Não escrever documentação genérica que serviria para qualquer sistema
- Não confundir requisito com implementação
- Não omitir exceções
- Não esconder incertezas
- Não focar apenas em endpoint, classe ou tela isoladamente