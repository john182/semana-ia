# spec-mcp-poc

POC para geração de XML de NFS-e a partir de:
- contrato OpenAPI/YAML
- schemas XSD
- regras de negócio já existentes no serializer legado
- fluxo com OpenSpec + MCP + LSP + OpenCode + Claude Code

## Objetivo
Demonstrar uma abordagem guiada por especificação para evoluir o serializer atual para uma arquitetura mais testável e legível:

request -> mapper -> modelo canônico -> builders XML -> XML final

## Escopo inicial
Primeira iteração limitada a:
- infDPS
- prest
- toma
- serv
- valores

## Próximos passos
1. Consolidar a spec da POC em `openspec/`.
2. Evoluir o request C# a partir do YAML enviado.
3. Implementar o modelo canônico e os builders XML.
4. Conectar o MCP `spec-assistant` ao OpenCode e ao Claude Code.
5. Gerar testes BDD + 3A para os primeiros critérios de aceite.


## Ajuste aplicado

Esta versão inicial da POC foi ajustada para usar o **XBuilder** como tecnologia de infraestrutura para geração de XML, refletindo o framework já utilizado no projeto real. O fluxo agora é `request -> mapper -> modelo canônico -> builder XML com XBuilder -> XML final`.
