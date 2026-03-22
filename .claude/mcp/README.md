# MCP da POC

## Objetivo
Centralizar a documentação dos MCPs usados ou planejados na POC de estudo sobre uso de IA.

## Servidor MCP atual
- Nome: `spec-assistant`
- Estado: ativo

## Registro
O servidor MCP é registrado na raiz do projeto em `.mcp.json`.

## Implementação
O código do servidor MCP atual fica em:

`ia/mcp-spec-server`

## Execução esperada
O Claude Code deve conseguir iniciar o servidor a partir de:

`node ./ia/mcp-spec-server/dist/index.js`

## Requisitos
- O diretório `dist/` deve existir.
- O build do servidor deve estar atualizado.
- O workspace deve ser aberto na raiz do projeto para que o caminho relativo funcione corretamente.

## Papel na POC
O MCP atual existe para apoiar o fluxo spec-driven, especialmente em:
- leitura e interpretação de specs
- apoio ao entendimento de proposal e tasks
- apoio ao fluxo de aplicação de change
- enriquecimento do `spec-agent`

## Estratégia da POC
Nesta POC:
- `.claude/mcp/` documenta os MCPs
- `ia/` contém o código real dos servidores MCP
- `.mcp.json` faz o vínculo entre Claude Code e os servidores

## Próximos MCPs possíveis
- GitHub
- documentação externa
- observabilidade

## Regra de simplicidade
Não adicionar MCP novo sem um caso de uso claro no fluxo da POC.
Priorizar primeiro o MCP de spec já existente.