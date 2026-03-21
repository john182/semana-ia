# Change: nfse-openapi-sync-expand-dtos

## Spec de referência
[nfse-openapi-sync](../../specs/nfse-openapi-sync/spec.md)

## Data
2026-03-20

## Tipo
`feature`

## Proposta
Expandir os DTOs C# do request NFS-e e a documentação Swagger para cobrir o contrato definido em `specs/openapi/nfse-request-openapi.yaml`, eliminando a divergência atual entre o contrato YAML e a implementação .NET.

## Problema

O DTO raiz `NfseGenerateXmlRequest` expõe apenas 10 dos ~35 campos raiz definidos no YAML. Nenhum dos 14 grupos complexos definidos no contrato possui classe C# correspondente:

- `activityEvent`
- `referenceSubstitution`
- `lease`
- `construction`
- `realEstate`
- `foreignTrade`
- `deduction`
- `benefit`
- `suspension`
- `approximateTax`
- `approximateTotals`
- `additionalInformationGroup`
- `serviceAmountDetails`
- `intermediary` / `recipient` completos (shape de `partyDefinition`)

O Swagger também não possui filtro de exemplos nem anotações de descrição de campo — violando RF-002, RF-003 e RF-004 da spec.

## Solução proposta

1. Completar `NfseGenerateXmlRequest` com os campos raiz faltantes do YAML.
2. Expandir `BorrowerRequest` para o shape completo de `partyDefinition`, reutilizável por `intermediary`, `recipient` e `supplier` de dedução.
3. Criar uma classe C# por grupo complexo faltante, organizadas em `Requests/Groups/`.
4. Criar `NfseRequestExamplesFactory` com os três exemplos do YAML (mínimo, intermediário, completo).
5. Criar `NfseExamplesOperationFilter` (IOperationFilter) e registrá-lo no `AddSwaggerGen`.
6. Habilitar XML doc comments no `.csproj` e anotar os campos com semântica mais crítica.

## Fora de escopo

- Serializer XML
- Grupo IBS/CBS (tratado em change futura dedicada — exposto apenas como `object?` placeholder nesta change)
- Validações de negócio
- Geração em build por XSD

## Requisitos atendidos

| Requisito | Descrição |
|-----------|-----------|
| RF-001 | Sincronização do contrato |
| RF-002 | Cobertura dos grupos do request |
| RF-003 | Exemplos de documentação (mínimo, intermediário, completo) |
| RF-004 | Documentação legível |
| RNF-001 | Clareza — classes por grupo em pasta própria |
| RNF-002 | Manutenibilidade — tipos extraídos por responsabilidade |
| RNF-003 | Rastreabilidade — registrado via OpenSpec change |

