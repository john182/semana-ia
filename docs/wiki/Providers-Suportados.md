# Providers Suportados

## Tabela de providers

| Provider | Padrão | Namespace | Status Operacional | Onboarding | XSD Válido | Observações |
|----------|--------|-----------|-------------------|------------|------------|-------------|
| **Nacional** | DPS Nacional | Single | SupportReady | Fully Onboarded | PASS | MVP principal. 0 erros XSD. Schema root: `TCDPS/DPS`. |
| **ISSNet** | ISSNet DPS | Single | SupportReady | Fully Onboarded | PASS | MVP. Schema root: `EnviarLoteDpsEnvio` (inline). Path bindings multinível via binder. |
| **GISSOnline** | ABRASF-based | Multi | NeedsEngineering | Schema Only | PASS | MVP com gaps rastreados. Multi-namespace. Falta configuração de regras tipadas. |
| **ABRASF** | ABRASF 2.04 | Single | NeedsEngineering | Schema Only | PASS | Padrão base. Falta configuração de regras tipadas. Detecção de envelope implementada. |
| **Paulistana** | SP Municipal | Multi | SupportReady | Fully Onboarded | FAIL | Schema root: `PedidoEnvioLoteRPS` (inline). XSD validation falha — requer campo Assinatura digital. |
| **Simpliss** | ABRASF-based | Single | SupportReady | Fully Onboarded | FAIL | Baseado em ABRASF. XSD validation falha por diferenças no schema derivado. |
| **WebISS** | ABRASF-based | — | SupportReady | Fully Onboarded | — | Totalmente onboarded. Sem geração de artefatos comparativos. |

## Significado dos status

### OperationalStatus

| Status | Significado |
|--------|------------|
| **SupportReady** | Provider totalmente operacional. Suporte pode usar sem intervenção de desenvolvedor. |
| **NeedsEngineering** | Provider carregável mas com gaps que exigem desenvolvimento na engine ou configuração avançada. |

### Onboarding Status

| Status | Significado |
|--------|------------|
| **Fully Onboarded** | Todos os 5 checks passaram: SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid. |
| **Schema Only** | Schema carregável e analisável, mas sem bindings/regras configuradas. Precisa de configuração. |

### XSD Válido

| Resultado | Significado |
|-----------|------------|
| **PASS** | XML gerado pela engine valida contra o XSD do provider sem erros. |
| **FAIL** | XML gerado contém erros de validação XSD. Gaps estão rastreados. |

## Gaps conhecidos por provider

### ABRASF
| Check | Tipo de gap | Detalhe |
|-------|------------|---------|
| BindingsPresent | ConfigurationGap | Nenhuma regra tipada configurada. |
| RuntimeProducible | EngineGap | Ignorado: bindings não disponíveis. |

**Ação:** Configurar regras em `providers/abrasf/rules/rules.json` ou usar `ProviderConfigGenerator` para auto-gerar.

### GISSOnline
| Check | Tipo de gap | Detalhe |
|-------|------------|---------|
| BindingsPresent | ConfigurationGap | Nenhuma regra tipada configurada. |
| RuntimeProducible | EngineGap | Ignorado: bindings não disponíveis. |

**Ação:** Configurar regras em `providers/gissonline/rules/rules.json` ou usar `ProviderConfigGenerator` para auto-gerar.

### Paulistana
| Check | Tipo de gap | Detalhe |
|-------|------------|---------|
| XsdValid | FAIL | Requer campo `Assinatura` com hash digital que não é gerado pela engine. |

**Ação:** Implementar geração de hash de assinatura digital para o schema SP.

### Simpliss
| Check | Tipo de gap | Detalhe |
|-------|------------|---------|
| XsdValid | FAIL | Diferenças no schema derivado do ABRASF causam erros de validação. |

**Ação:** Investigar divergências entre o XSD Simpliss e o ABRASF padrão.

## Providers MVP

Os 3 providers definidos como MVP são:

1. **Nacional** — PASS completo, 0 erros XSD
2. **ISSNet** — PASS completo com path bindings multinível
3. **GISSOnline** — Schema analisável, gaps de configuração rastreados

---

**Páginas relacionadas:**
- [Visão Geral do Produto](Visao-Geral-do-Produto.md)
- [Cadastro de Providers](Cadastro-de-Providers.md)
- [Status e Classificação de Gaps](Status-e-Classificacao-de-Gaps.md)
- [Onboarding de Provider pelo Suporte](Onboarding-de-Provider-pelo-Suporte.md)
