# Critérios de aceite

1. A solução deve receber um request JSON compatível com o contrato base da POC.
2. A solução deve mapear o request para um modelo canônico antes de gerar XML.
3. A solução deve gerar uma estrutura XML contendo, no mínimo:
   - infDPS
   - prest
   - toma
   - serv
   - valores
4. As regras de preenchimento devem ficar separadas da montagem de XML.
5. Deve existir um endpoint na API para gerar o XML.
6. Deve existir um MCP próprio chamado `spec-assistant` com tools mínimas de spec.
7. Deve ser possível demonstrar um fluxo multiagente com OpenCode/Claude Code.
8. Devem existir exemplos de prompts/comandos para apresentação.
