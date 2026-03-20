# AGENTS.md

## Objetivo do projeto
Gerar XML de NFS-e em C# com base em contrato YAML/OpenAPI, XSDs e regras de negócio já presentes no serializer legado.

## Regras para os agentes
- Sempre ler primeiro `openspec/nfse-serializer.spec.md`.
- Sempre listar os critérios de aceite antes de propor implementação.
- Não gerar XML diretamente do request sem passar por um modelo canônico intermediário.
- Separar regras de negócio de montagem de XML.
- Em testes C#, usar nomes em BDD e estrutura Arrange / Act / Assert.
- Em refatorações, preferir coesão alta, baixo acoplamento e nomes alinhados à linguagem do domínio.

## Papéis sugeridos
### spec-agent
Lê a OpenSpec e traduz em backlog técnico.

### contract-agent
Analisa o YAML/OpenAPI e propõe modelos de contrato.

### schema-agent
Analisa XSD e define a estrutura XML alvo.

### implementation-agent
Gera ou ajusta código C#.

### quality-agent
Gera testes, faz review e sugere refatorações.
