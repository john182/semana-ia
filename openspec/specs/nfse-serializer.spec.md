# FEATURE-NFSE-SERIALIZER-001

## Título
POC - geração de XML de NFS-e a partir de contrato YAML, schemas XSD e regras existentes

## Problema
Hoje a geração do XML está centralizada em serializers com muitas responsabilidades: leitura de regras fiscais, decisões de presença de tags e montagem direta da árvore XML.

## Objetivo
Construir uma POC que demonstre uma abordagem orientada por especificação para:
- mapear request de API para um modelo canônico
- gerar XML compatível com a estrutura nacional da NFS-e
- preservar as regras de preenchimento já conhecidas
- habilitar colaboração com OpenCode e Claude Code usando MCP e LSP

## Entradas
- YAML/OpenAPI do request
- XSDs do layout nacional
- serializer legado baseado em XBuilder
- exemplos mínimo, intermediário e completo

## Saídas
- endpoint ASP.NET Core para gerar XML
- modelo canônico intermediário
- builders XML separados por responsabilidade
- MCP de spec
- prompts e papéis de agentes para demo

## Fora de escopo inicial
- assinatura digital completa
- todos os cenários de evento/cancelamento
- suporte completo a todos os grupos avançados


## Decisão técnica inicial

A POC deve usar o **XBuilder** na camada de infraestrutura como framework de construção XML, por ser a tecnologia já adotada no cenário real e por permitir demonstrar evolução arquitetural sem abandonar a base atual de serialização.
