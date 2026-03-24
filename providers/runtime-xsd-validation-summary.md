# Runtime Serializer XSD Validation Summary

| Provider | Schema Root | Namespace | XSD Valid | OperationalStatus | Onboarding Status | Gap Classification | Choice | Sequence |
|----------|------------|-----------|----------|-------------------|-------------------|-------------------|--------|----------|
| Nacional | TCDPS/DPS | Single | PASS | SupportReady | Fully Onboarded | None | CNPJ/CPF/NIF/cNaoNIF | tpAmb->valores |
| ABRASF | EnviarLoteRpsEnvio (inline) | Single | PASS | NeedsEngineering | Schema Only | ConfigurationGap, EngineGap | Cpf/Cnpj choice | NumeroLote->ListaRps |
| GISSOnline | EnviarLoteRpsEnvio (inline) | Multi | PASS | NeedsEngineering | Schema Only | ConfigurationGap, EngineGap | Cpf/Cnpj choice | NumeroLote->ListaRps+IBSCBS |
| ISSNet | EnviarLoteDpsEnvio (inline) | Single | PASS | SupportReady | Fully Onboarded | None | CNPJ/CPF choice | LoteDps->ListaDps->DPS via binder |
| Paulistana | PedidoEnvioLoteRPS (inline) | Multi | FAIL | SupportReady | Fully Onboarded | None | CPF/CNPJ | Cabecalho->ListaRPS |
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
| nacional | SupportReady | Fully Onboarded | 5 | 5 | None |
| gissonline | NeedsEngineering | Schema Only | 3 | 5 | BindingsPresent, RuntimeProducible |
| paulistana | SupportReady | Fully Onboarded | 5 | 5 | None |
| abrasf | NeedsEngineering | Schema Only | 3 | 5 | BindingsPresent, RuntimeProducible |
| webiss | SupportReady | Fully Onboarded | 5 | 5 | None |
| issnet | SupportReady | Fully Onboarded | 5 | 5 | None |
| simpliss | SupportReady | Fully Onboarded | 5 | 5 | None |

## Gaps

| Provider | Gap Kind | Gap | Reason |
|----------|----------|-----|--------|
| gissonline | ConfigurationGap | BindingsPresent | No typed rules configured. |
| gissonline | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| abrasf | ConfigurationGap | BindingsPresent | No typed rules configured. |
| abrasf | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
