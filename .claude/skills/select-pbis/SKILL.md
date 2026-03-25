---
name: select-pbis
description: Seleciona PBIs com menor impacto e menor colisão para execução paralela segura
agent: pbi-manager
---

# Objetivo

Avaliar múltiplas PBIs candidatas e escolher a combinação mais segura para execução.

# Entrada esperada

Uma lista de PBIs, por exemplo:
- 101
- 102
- 103

# Fluxo obrigatório

1. Ler cada PBI candidata
2. Classificar impacto
3. Identificar arquivos e módulos prováveis
4. Comparar colisão entre as PBIs
5. Identificar dependências e bloqueios
6. Escolher a melhor estratégia:
    - 1 PBI
    - 2 PBIs paralelas
    - 3 PBIs paralelas
    - 1 PBI quebrada internamente
    - enviar PBI para planejamento

# Saída obrigatória

1. resumo executivo
2. ranking das PBIs
3. matriz de colisão
4. combinações seguras
5. combinações não seguras
6. estratégia recomendada
7. plano de handoff