# Runtime Serializer XSD Validation Summary

| Provider | Schema Root | XSD Valid | Choice | Sequence | Required |
|----------|------------|----------|--------|----------|----------|
| Nacional | TCDPS/DPS | PASS | CNPJ/CPF/NIF/cNaoNIF | tpAmb→valores | tpAmb,dhEmi,serie,prest,serv,valores |
| ABRASF | tcLoteRps | ANALYZED (64 types) | Cpf/Cnpj choice identified | NumeroLote→ListaRps | Requires anonymous type support |
| GISSOnline | tcLoteRps | ANALYZED (71 types) | Cpf/Cnpj choice identified | NumeroLote→ListaRps | Requires anonymous type support |
| ISSNet | TCDPS (via EnviarLoteDpsEnvio) | ANALYZED (114 types) | CNPJ/CPF/NIF/cNaoNIF | tpAmb→valores | Requires anonymous type support |
| Paulistana | PedidoEnvioLoteRPS | ANALYZED | CPF/CNPJ | Cabecalho→ListaRPS | CPFCNPJRemetente,dtInicio,dtFim,QtdRPS |

## Summary

**Total providers:** 5
**Runtime XML validated (XSD pass):** Nacional = True
**Schema analyzed (model ready):** ABRASF (64 types), GISSOnline (71 types), ISSNet (114 types), Paulistana (31 types)
**Pending for runtime:** ABRASF, GISSOnline, ISSNet, Paulistana require anonymous inline type support in serializer
