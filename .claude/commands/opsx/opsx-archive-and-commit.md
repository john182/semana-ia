---
description: Arquiva uma change aprovada e finalizada, sincroniza a spec e gera commit semântico automaticamente
argument-hint: <change-name>
---

# Objetivo

Arquivar uma change já concluída, garantindo que:

- a spec consolidada esteja atualizada
- a pasta da change seja movida para archive
- o estado final do repositório seja revisado
- um commit semântico seja gerado automaticamente com base nas alterações realizadas

# Entrada obrigatória

Receba o nome da change em `$ARGUMENTS`.

Se o nome da change não for informado, interrompa e peça explicitamente o identificador correto da change.

# Regras gerais

- Só executar este fluxo para changes já implementadas e revisadas.
- Não arquivar se a change ainda tiver lacunas relevantes de implementação, testes ou revisão.
- Não criar commit vazio.
- O commit deve refletir apenas o estado final da change arquivada.
- A mensagem deve seguir o padrão de Conventional Commits.
- O commit deve resumir corretamente a natureza principal da mudança.

# Fluxo obrigatório

## Etapa 1 — Validar a existência da change

Localize a change informada.

Verifique se existem os artefatos esperados da change, como por exemplo:
- proposal
- tasks
- spec deltas
- arquivos associados à implementação

Se a change não existir ou estiver inconsistente, interrompa e explique o problema.

---

## Etapa 2 — Verificar se a change está pronta para archive

Antes de arquivar, revisar o estado da mudança.

Confirmar que:
- a implementação foi concluída
- os testes necessários foram criados ou ajustados
- a revisão técnica final foi feita
- não existem pendências explícitas da própria change
- não existem alterações sabidamente fora do escopo da change

Se houver indício forte de mudança incompleta, interrompa e explique o motivo.

---

## Etapa 3 — Executar o archive

Executar o fluxo de archive da change.

Objetivo desta etapa:
- consolidar a spec final
- mover a change para o local de archive
- deixar o repositório no estado final pós-change

Após o archive, validar:
- se a pasta da change saiu do estado ativo
- se os arquivos de spec ficaram consistentes
- se o estado final do repositório faz sentido

Se o archive falhar, interrompa e não tente commitar.

---

## Etapa 4 — Inspecionar o diff final

Após o archive, analisar os arquivos alterados no repositório.

Classificar as mudanças em grupos, por exemplo:
- código de produção
- testes
- spec/documentação
- infraestrutura/automação
- archive da change

Usar essa leitura para decidir o tipo principal do commit.

---

## Etapa 5 — Determinar o tipo semântico do commit

Escolher o tipo principal do commit com base no impacto predominante da change.

Usar preferencialmente um destes tipos:

- `feat` → quando a change adiciona funcionalidade nova relevante
- `fix` → quando corrige defeito ou comportamento incorreto
- `refactor` → quando melhora estrutura interna sem alterar comportamento esperado
- `test` → quando a principal mudança é cobertura de testes
- `docs` → quando a principal mudança é documentação/spec
- `chore` → quando a principal mudança é manutenção técnica sem valor funcional direto

Regras:
- escolher um único tipo principal
- priorizar o efeito dominante da mudança
- não escolher `chore` se houve entrega funcional clara
- não escolher `docs` se houve implementação real de código relevante

---

## Etapa 6 — Gerar a mensagem do commit

Gerar uma mensagem de commit curta, clara e semântica no padrão:

`tipo: descrição`

Exemplos válidos:
- `feat: add spec assistant MCP integration`
- `fix: correct XML serialization for IBS CBS blocks`
- `refactor: centralize cep and phone normalization`
- `test: add serializer coverage for conditional XML nodes`
- `docs: archive spec change for multi agent orchestration`

Regras da mensagem:
- usar inglês, salvo se o projeto adotar outro padrão explícito
- ser curta e objetiva
- descrever o efeito principal da mudança
- evitar mensagens vagas como `update`, `changes`, `adjustments`
- evitar múltiplas intenções no mesmo título

Se fizer sentido, pode adicionar corpo explicativo curto no commit, com:
- resumo das áreas afetadas
- observação de archive da change
- contexto resumido da implementação

---

## Etapa 7 — Preparar o commit

Antes de commitar:
- revisar os arquivos modificados
- garantir que o estado final está coerente
- garantir que não existem alterações acidentais e irrelevantes
- garantir que o commit não está vazio

Se houver arquivos suspeitos, sinalizar isso antes do commit.

---

## Etapa 8 — Executar o commit

Realizar o commit usando a mensagem gerada.

Se houver corpo complementar, incluir no commit.

Não criar mais de um commit nesta etapa.

Se o commit falhar, explicar claramente o erro.

---

# Forma de resposta esperada

Ao final, apresentar um resumo estruturado contendo:

## 1. Change arquivada
- nome da change
- confirmação do archive

## 2. Arquivos e áreas impactadas
- visão resumida do que entrou no estado final

## 3. Tipo semântico escolhido
- tipo do commit
- justificativa curta

## 4. Commit gerado
- mensagem final do commit

## 5. Observações finais
- riscos, pendências ou ausência delas

# Critério de conclusão

Só considerar concluído quando:
- a change tiver sido arquivada com sucesso
- o estado final do repositório tiver sido inspecionado
- o commit semântico tiver sido gerado
- o `git commit` tiver sido executado com sucesso