# Spec: nfse-openapi-sync

## Objetivo
Definir a capacidade do projeto de manter a documentação Swagger/OpenAPI da NFS-e sincronizada com a especificação funcional localizada em `specs/openapi/nfse-request.yaml`.

Essa spec descreve o comportamento esperado da sincronização entre:
- o contrato YAML/OpenAPI
- os DTOs C# usados na API
- os exemplos exibidos no Swagger
- os filtros e configurações de documentação

## Contexto
O projeto possui um arquivo YAML em `specs/openapi/nfse-request.yaml` que representa o contrato HTTP da requisição de NFS-e, incluindo:
- schema principal do request
- descrições de campos
- enums
- exemplos mínimo, intermediário e completo
- grupos complexos e objetos aninhados

A documentação Swagger no projeto .NET deve refletir esse contrato de forma consistente, reduzindo divergência entre documentação e implementação.

## Fonte de verdade
A fonte primária de verdade para o contrato HTTP desta capacidade é:

`specs/openapi/nfse-request.yaml`

Quando houver divergência entre o YAML e os DTOs/documentação do Swagger, o YAML deve ser considerado a referência funcional, salvo decisão explícita registrada em change/proposal.

## Escopo
Esta spec cobre:
- DTOs C# de request relacionados à NFS-e
- organização e modelagem de subobjetos do request
- exemplos apresentados no Swagger
- descrições e metadados exibidos na documentação
- filtros e configuração de OpenAPI/Swagger necessários para expor corretamente o contrato

## Fora de escopo
Esta spec não cobre:
- geração do serializer XML da NFS-e
- leitura de XSD para geração de XML
- assinatura digital
- geração em build
- regras completas de serialização XML
- processamento multiagente
- scheduling

## Requisitos funcionais

### RF-001 — Sincronização do contrato
O projeto deve possuir DTOs C# que representem de forma fiel o contrato descrito em `specs/openapi/nfse-request.yaml`.

### RF-002 — Cobertura dos grupos do request
Os grupos relevantes descritos no YAML devem estar refletidos no modelo C#, incluindo campos raiz e objetos aninhados necessários para a documentação Swagger.

### RF-003 — Exemplos de documentação
O Swagger deve expor exemplos representativos do request, incluindo pelo menos:
- exemplo mínimo
- exemplo intermediário
- exemplo completo

### RF-004 — Documentação legível
Os campos expostos no Swagger devem possuir nomes, descrições e estrutura compatíveis com o contrato YAML, respeitando a semântica da integração NFS-e.

### RF-005 — Evolução incremental
A sincronização pode ser implementada em fases, desde que cada fase preserve coerência entre o contrato documentado e os DTOs efetivamente expostos.

## Requisitos não funcionais

### RNF-001 — Clareza
A modelagem dos DTOs deve priorizar legibilidade e organização, evitando concentrar todo o contrato em uma única classe excessivamente grande.

### RNF-002 — Manutenibilidade
Objetos complexos devem ser extraídos para tipos próprios, facilitando manutenção da documentação e futuras evoluções.

### RNF-003 — Rastreabilidade
Mudanças significativas nessa capacidade devem ser registradas via proposals/changes do OpenSpec.

## Estratégia de evolução
A evolução desta capacidade deve ocorrer por changes menores, por exemplo:
- completar DTOs base do request
- adicionar grupos faltantes
- alinhar examples factory com o YAML
- ajustar descrições/documentação do Swagger
- adicionar grupos avançados como IBS/CBS

## Critérios de aceitação da capacidade
A capacidade `nfse-openapi-sync` será considerada atendida quando:
1. o YAML estiver presente no repositório
2. os DTOs do request refletirem os grupos necessários do contrato
3. o Swagger exibir corretamente os exemplos esperados
4. houver uma trilha clara de evolução via OpenSpec para mudanças futuras