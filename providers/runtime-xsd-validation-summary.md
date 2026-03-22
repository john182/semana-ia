# Runtime Serializer XSD Validation Summary

| Provider | Schema Root | XSD Valid | Choice | Sequence | Required |
|----------|------------|----------|--------|----------|----------|
| Nacional | TCDPS/DPS | PASS | CNPJ/CPF/NIF/cNaoNIF | tpAmb→valores | tpAmb,dhEmi,serie,prest,serv,valores |
| ABRASF | EnviarLoteRpsEnvio (inline) | PASS | Cpf/Cnpj choice | NumeroLote→ListaRps | None |
| GISSOnline | EnviarLoteRpsEnvio (inline) | FAIL | Cpf/Cnpj choice | NumeroLote→ListaRps+IBSCBS | XSD: [Error] The element 'LoteRps' in namespace 'http://www.giss. |
| ISSNet | EnviarLoteDpsEnvio (inline) | FAIL | CNPJ/CPF choice | tpAmb→valores via DPS | Errors: Required complex element 'LoteDps' has no data |
| Paulistana | PedidoEnvioLoteRPS (inline) | ANALYZED (32 types) | CPF/CNPJ | Cabecalho→ListaRPS | Data bindings not yet configured |

## Summary

**Total providers:** 5
**Runtime XML validated (XSD pass):** 2/4 (Nacional, ABRASF, GISSOnline, ISSNet)
**Schema analyzed:** Paulistana (32 types)
**Inline type support:** Enabled — anonymous complexTypes resolved recursively
