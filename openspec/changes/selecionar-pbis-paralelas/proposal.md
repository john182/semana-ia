## Why

O projeto possui 13+ PBIs na Fase 4 (Production Ready), mas execução serial é lenta e subutiliza o modelo multiagente. Queremos selecionar as 2 PBIs com menor risco de colisão de arquivos para execução paralela em worktrees isoladas, maximizando throughput sem conflitos de merge.

## What Changes

### Análise de colisão realizada

| PBI | Camada principal | Arquivos-alvo | Risco colisão |
|-----|-----------------|---------------|---------------|
| Envelope ABRASF (P0) | XmlGeneration | SchemaBasedXmlSerializer, SchemaSerializationPipeline | Baixo |
| Paulistana Assinatura (P0) | XmlGeneration + Domain | Provider-specific + hash generation | Médio |
| Certificado digital (P0) | Infrastructure | Novo serviço + lib externa | Médio |
| Error handling correlation ID (P0) | Api + Application + Infra | **Cross-cutting — ALTO** |
| Mapeamento dinâmico de enums (P0) | XmlGeneration | Engine core | Médio |
| Logging estruturado (P1) | Api + Infra | **Cross-cutting — ALTO** |
| Health check endpoint (P1) | Api | Novo controller isolado | **Muito baixo** |
| CI/CD pipeline (P1) | INFRA only | .github/workflows | **Zero** |
| Inferência de conditionals (P1) | XmlGeneration | Engine core | Médio |
| Inferência de enums (P1) | XmlGeneration | Engine core | Médio |
| Swagger/OpenAPI (P2) | Api | Configuração Swagger | Baixo |
| Rate limiting (P2) | Api + Infra | Middleware | Médio |
| Testes de contrato (P2) | Tests only | Test projects | **Zero** |

### PBIs selecionadas para execução paralela

1. **Health check endpoint** (P1-DEV) — Cria novo controller `/health` na camada Api. Arquivo novo, sem edição de arquivos existentes compartilhados.
2. **Envelope ABRASF** (P0-DEV) — Corrige envelope de envio na camada XmlGeneration. Edita SchemaSerializationPipeline e providers ABRASF. Zero sobreposição com Api.

**Justificativa da seleção:**
- Camadas completamente distintas: Api vs XmlGeneration
- Zero arquivos em comum
- Uma é P0 (bloqueador de produção), outra é P1 (essencial)
- Ambas são DEV puro, sem dependência de infraestrutura externa
- Escopo contido e bem definido

### PBIs descartadas e motivo

| PBI | Motivo exclusão |
|-----|----------------|
| Error handling correlation ID | Cross-cutting — toca Api, Application e Infrastructure simultaneamente |
| Logging estruturado | Cross-cutting — mesmo problema |
| Certificado digital | Depende de lib externa e infra, não é paralelizável com segurança |
| Paulistana Assinatura | Toca XmlGeneration — colidiria com Envelope ABRASF |
| Mapeamento/Inferência de enums | Toca XmlGeneration engine core — colidiria com Envelope ABRASF |
| CI/CD pipeline | Tipo INFRA, não DEV — baixo valor técnico para este ciclo |
| Testes de contrato | P2 — pode ser feito depois sem risco |

## Capabilities

### New Capabilities
- `nfse-health-check`: Endpoint `/health` que reporta status dos providers disponíveis, versão da engine e conectividade MongoDB

### Modified Capabilities
- `nfse-runtime-xml-serializer`: Corrigir detecção e geração de envelope ABRASF para providers que usam esse padrão (GISSOnline, Simpliss)

## Impact

- **Api**: Novo `HealthController` + registro no DI container
- **XmlGeneration**: Alteração em `SchemaSerializationPipeline` ou `SchemaBasedXmlSerializer` para envelope ABRASF
- **Providers**: Ajuste em configs de providers ABRASF (GISSOnline, Simpliss)
- **Tests**: Novos testes unitários e de integração para ambas as PBIs
- **Zero sobreposição de arquivos** entre as duas PBIs
