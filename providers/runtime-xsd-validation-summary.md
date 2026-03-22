# Runtime Serializer XSD Validation Summary

| Provider | Schema Root | Namespace | XSD Valid | Choice | Sequence | Required |
|----------|------------|-----------|----------|--------|----------|----------|
| Nacional | TCDPS/DPS | Single | PASS | CNPJ/CPF/NIF/cNaoNIF | tpAmb→valores | tpAmb,dhEmi,serie,prest,serv,valores |
| ABRASF | EnviarLoteRpsEnvio (inline) | Single | PASS | Cpf/Cnpj choice | NumeroLote→ListaRps | None |
| GISSOnline | EnviarLoteRpsEnvio (inline) | Multi | PASS | Cpf/Cnpj choice | NumeroLote→ListaRps+IBSCBS | None |
| ISSNet | EnviarLoteDpsEnvio (inline) | Single | FAIL | CNPJ/CPF choice | tpAmb→valores via DPS | Errors: Required complex element 'LoteDps' has no data |
| Paulistana | PedidoEnvioLoteRPS (inline) | Multi | ANALYZED (32 types) | CPF/CNPJ | Cabecalho→ListaRPS | Data bindings not yet configured |
| Simpliss | nfse (ABRASF-based) | Single | ANALYZED (58 types) | Cpf/Cnpj | NumeroLote→ListaRps | Data bindings not yet configured |

## Summary

**Total providers:** 6
**Runtime XML validated (XSD pass):** 3/4 (Nacional, ABRASF, GISSOnline, ISSNet)
**Schema analyzed:** Paulistana (32 types), Simpliss (58 types)
**Inline type support:** Enabled — anonymous complexTypes resolved recursively
**Multi-namespace support:** Enabled — elements emitted in correct namespace per type

## Gaps

| Provider | Gap | Reason |
|----------|-----|--------|
| ISSNet | Errors: Required complex element 'LoteDps' has no data | LoteDps wrapper requires specific data bindings not yet mapped |
| Paulistana | Schema analyzed only | Data bindings not yet configured for SP municipal schema |
| Simpliss | Schema analyzed only | ABRASF-based schema; data bindings not yet configured |
