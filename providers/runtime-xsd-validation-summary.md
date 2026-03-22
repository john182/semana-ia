# Runtime Serializer XSD Validation Summary

| Provider | Schema Root | Namespace | XSD Valid | Onboarding Status | Gap Classification | Choice | Sequence |
|----------|------------|-----------|----------|-------------------|-------------------|--------|----------|
| Nacional | TCDPS/DPS | Single | PASS | Partial | EngineGap | CNPJ/CPF/NIF/cNaoNIF | tpAmb->valores |
| ABRASF | EnviarLoteRpsEnvio (inline) | Single | PASS | Schema Only | ConfigurationGap, EngineGap | Cpf/Cnpj choice | NumeroLote->ListaRps |
| GISSOnline | EnviarLoteRpsEnvio (inline) | Multi | PASS | Schema Only | ConfigurationGap, EngineGap | Cpf/Cnpj choice | NumeroLote->ListaRps+IBSCBS |
| ISSNet | EnviarLoteDpsEnvio (inline) | Single | PASS | Partial | EngineGap | CNPJ/CPF choice | LoteDps->ListaDps->DPS via binder |
| Paulistana | PedidoEnvioLoteRPS (inline) | Multi | ANALYZED (32 types) | Schema Only | ConfigurationGap, EngineGap | CPF/CNPJ | Cabecalho->ListaRPS |
| Simpliss | nfse (ABRASF-based) | Single | ANALYZED (58 types) | Schema Only | ConfigurationGap, EngineGap | Cpf/Cnpj | NumeroLote->ListaRps |

## Summary

**Total providers:** 6
**Runtime XML attempted:** 4/6
**Runtime XML validated (XSD pass):** 4/4 (Nacional, ABRASF, GISSOnline, ISSNet)
**Schema analyzed only:** Paulistana, Simpliss
**Inline type support:** Enabled -- anonymous complexTypes resolved recursively
**Multi-namespace support:** Enabled -- elements emitted in correct namespace per type

## Onboarding Status

| Provider | Status | Checks Passed | Total Checks | Failed Checks |
|----------|--------|---------------|--------------|---------------|
| abrasf | Schema Only | 3 | 5 | BindingsPresent, RuntimeProducible |
| gissonline | Schema Only | 3 | 5 | BindingsPresent, RuntimeProducible |
| issnet | Partial | 4 | 5 | RuntimeProducible |
| nacional | Partial | 4 | 5 | RuntimeProducible |
| paulistana | Schema Only | 3 | 5 | BindingsPresent, RuntimeProducible |
| simpliss | Schema Only | 3 | 5 | BindingsPresent, RuntimeProducible |

## Gaps

| Provider | Gap Kind | Gap | Reason |
|----------|----------|-----|--------|
| abrasf | ConfigurationGap | BindingsPresent | No bindings configured in base-rules.json. |
| abrasf | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| gissonline | ConfigurationGap | BindingsPresent | No bindings configured in base-rules.json. |
| gissonline | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| issnet | EngineGap | RuntimeProducible | Serialization produced errors: [InputError] LoteDps.Prestador.IM: Required element 'IM' has no value and no default; [InputError] LoteDps.ListaDps.DPS.infDPS.prest.IM: Required element 'IM' has no value and no default; [InputError] LoteDps.ListaDps.DPS.infDPS.serv.cServ.cTribMun: Required element 'cTribMun' has no value and no default; [InputError] LoteDps.ListaDps.DPS.infDPS.serv.cServ.cNBS: Required element 'cNBS' has no value and no default |
| nacional | EngineGap | RuntimeProducible | Serialization produced errors: [InputError] infDPS.serv.cServ.cNBS: Required element 'cNBS' has no value and no default |
| paulistana | ConfigurationGap | BindingsPresent | No bindings configured in base-rules.json. |
| paulistana | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| simpliss | ConfigurationGap | BindingsPresent | No bindings configured in base-rules.json. |
| simpliss | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
