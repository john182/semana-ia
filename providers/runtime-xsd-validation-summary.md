# Runtime Serializer XSD Validation Summary

| Provider | Schema Root | Namespace | XSD Valid | OperationalStatus | Onboarding Status | Gap Classification | Choice | Sequence |
|----------|------------|-----------|----------|-------------------|-------------------|-------------------|--------|----------|
| Nacional | TCDPS/DPS | Single | PASS | SupportReady | Fully Onboarded | None | CNPJ/CPF/NIF/cNaoNIF | tpAmb->valores |
| ABRASF | EnviarLoteRpsEnvio (inline) | Single | PASS | NeedsEngineering | Schema Only | ConfigurationGap, EngineGap | Cpf/Cnpj choice | NumeroLote->ListaRps |
| GISSOnline | EnviarLoteRpsEnvio (inline) | Multi | PASS | NeedsEngineering | Schema Only | ConfigurationGap, EngineGap | Cpf/Cnpj choice | NumeroLote->ListaRps+IBSCBS |
| ISSNet | EnviarLoteDpsEnvio (inline) | Single | PASS | SupportReady | Fully Onboarded | None | CNPJ/CPF choice | LoteDps->ListaDps->DPS via binder |
| Paulistana | PedidoEnvioLoteRPS (inline) | Multi | FAIL | SupportConfigOnly | Partial | ConfigurationGap | CPF/CNPJ | Cabecalho->ListaRPS |
| Simpliss | nfse (ABRASF-based) | Single | FAIL | SupportReady | Fully Onboarded | None | Cpf/Cnpj | NumeroLote->ListaRps |

## Summary

**Total providers:** 6
**Runtime XML attempted:** 6/6
**Runtime XML validated (XSD pass):** 4/6 (Nacional, ABRASF, GISSOnline, ISSNet)
**Runtime XML failed:** Paulistana, Simpliss
**Inline type support:** Enabled -- anonymous complexTypes resolved recursively
**Multi-namespace support:** Enabled -- elements emitted in correct namespace per type

## Onboarding Status

| Provider | OperationalStatus | Status | Checks Passed | Total Checks | Failed Checks |
|----------|-------------------|--------|---------------|--------------|---------------|
| abrasf | NeedsEngineering | Schema Only | 3 | 5 | BindingsPresent, RuntimeProducible |
| gissonline | NeedsEngineering | Schema Only | 3 | 5 | BindingsPresent, RuntimeProducible |
| issnet | SupportReady | Fully Onboarded | 5 | 5 | None |
| nacional | SupportReady | Fully Onboarded | 5 | 5 | None |
| paulistana | SupportConfigOnly | Partial | 4 | 5 | RuntimeProducible |
| simpliss | SupportReady | Fully Onboarded | 5 | 5 | None |
| webiss | SupportConfigOnly | Partial | 4 | 5 | RuntimeProducible |

## Gaps

| Provider | Gap Kind | Gap | Reason |
|----------|----------|-----|--------|
| abrasf | ConfigurationGap | BindingsPresent | No bindings configured in base-rules.json. |
| abrasf | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| gissonline | ConfigurationGap | BindingsPresent | No bindings configured in base-rules.json. |
| gissonline | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| paulistana | ConfigurationGap | RuntimeProducible | Serialization produced errors: [InputError] RPS: Required complex element 'RPS' has no data; [InputError] : Required element '' has no value and no default |
| webiss | ConfigurationGap | RuntimeProducible | Serialization produced errors: [InputError] MensagemRetorno.IdentificacaoRps.Numero: Required element 'Numero' has no value and no default; [InputError] MensagemRetorno.IdentificacaoRps.Tipo: Required element 'Tipo' has no value and no default; [InputError] MensagemRetorno.Codigo: Required element 'Codigo' has no value and no default; [InputError] MensagemRetorno.Mensagem: Required element 'Mensagem' has no value and no default |
