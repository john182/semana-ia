# Runtime Serializer XSD Validation Summary

| Provider | Schema Root | Namespace | XSD Valid | OperationalStatus | Onboarding Status | Gap Classification | Choice | Sequence |
|----------|------------|-----------|----------|-------------------|-------------------|-------------------|--------|----------|
| Nacional | TCDPS/DPS | Single | PASS | NeedsEngineering | Not Started | ConfigurationGap, SchemaIncompatibility, EngineGap | CNPJ/CPF/NIF/cNaoNIF | tpAmb->valores |
| ABRASF | EnviarLoteRpsEnvio (inline) | Single | PASS | NeedsEngineering | Not Started | ConfigurationGap, SchemaIncompatibility, EngineGap | Cpf/Cnpj choice | NumeroLote->ListaRps |
| GISSOnline | EnviarLoteRpsEnvio (inline) | Multi | PASS | NeedsEngineering | Not Started | ConfigurationGap, SchemaIncompatibility, EngineGap | Cpf/Cnpj choice | NumeroLote->ListaRps+IBSCBS |
| ISSNet | EnviarLoteDpsEnvio (inline) | Single | PASS | NeedsEngineering | Not Started | ConfigurationGap, SchemaIncompatibility, EngineGap | CNPJ/CPF choice | LoteDps->ListaDps->DPS via binder |
| Paulistana | PedidoEnvioLoteRPS (inline) | Multi | FAIL | NeedsEngineering | Not Started | ConfigurationGap, SchemaIncompatibility, EngineGap | CPF/CNPJ | Cabecalho->ListaRPS |
| Simpliss | nfse (ABRASF-based) | Single | FAIL | NeedsEngineering | Not Started | ConfigurationGap, SchemaIncompatibility, EngineGap | Cpf/Cnpj | NumeroLote->ListaRps |

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
| Abrasf | NeedsEngineering | Not Started | 0 | 5 | SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid |
| Gissonline | NeedsEngineering | Not Started | 0 | 5 | SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid |
| Issnet | NeedsEngineering | Not Started | 0 | 5 | SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid |
| Nacional | NeedsEngineering | Not Started | 0 | 5 | SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid |
| Paulistana | NeedsEngineering | Not Started | 0 | 5 | SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid |
| Simpliss | NeedsEngineering | Not Started | 0 | 5 | SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid |
| Webiss | NeedsEngineering | Not Started | 0 | 5 | SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid |
| _Shared | NeedsEngineering | Not Started | 0 | 5 | SchemaLoadable, AnalysisOk, BindingsPresent, RuntimeProducible, XsdValid |

## Gaps

| Provider | Gap Kind | Gap | Reason |
|----------|----------|-----|--------|
| Abrasf | ConfigurationGap | SchemaLoadable | XSD directory not found: D:\exemplos\bkp\SemanaIA\tests\SemanaIA.ServiceInvoice.UnitTests\providers\Abrasf\xsd |
| Abrasf | SchemaIncompatibility | AnalysisOk | Skipped: schema is not loadable. |
| Abrasf | ConfigurationGap | BindingsPresent | Failed to load provider profile (rules.json missing or invalid). |
| Abrasf | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| Abrasf | SchemaIncompatibility | XsdValid | Skipped: schema is not loadable. |
| Gissonline | ConfigurationGap | SchemaLoadable | XSD directory not found: D:\exemplos\bkp\SemanaIA\tests\SemanaIA.ServiceInvoice.UnitTests\providers\Gissonline\xsd |
| Gissonline | SchemaIncompatibility | AnalysisOk | Skipped: schema is not loadable. |
| Gissonline | ConfigurationGap | BindingsPresent | Failed to load provider profile (rules.json missing or invalid). |
| Gissonline | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| Gissonline | SchemaIncompatibility | XsdValid | Skipped: schema is not loadable. |
| Issnet | ConfigurationGap | SchemaLoadable | XSD directory not found: D:\exemplos\bkp\SemanaIA\tests\SemanaIA.ServiceInvoice.UnitTests\providers\Issnet\xsd |
| Issnet | SchemaIncompatibility | AnalysisOk | Skipped: schema is not loadable. |
| Issnet | ConfigurationGap | BindingsPresent | Failed to load provider profile (rules.json missing or invalid). |
| Issnet | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| Issnet | SchemaIncompatibility | XsdValid | Skipped: schema is not loadable. |
| Nacional | ConfigurationGap | SchemaLoadable | XSD directory not found: D:\exemplos\bkp\SemanaIA\tests\SemanaIA.ServiceInvoice.UnitTests\providers\Nacional\xsd |
| Nacional | SchemaIncompatibility | AnalysisOk | Skipped: schema is not loadable. |
| Nacional | ConfigurationGap | BindingsPresent | Failed to load provider profile (rules.json missing or invalid). |
| Nacional | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| Nacional | SchemaIncompatibility | XsdValid | Skipped: schema is not loadable. |
| Paulistana | ConfigurationGap | SchemaLoadable | XSD directory not found: D:\exemplos\bkp\SemanaIA\tests\SemanaIA.ServiceInvoice.UnitTests\providers\Paulistana\xsd |
| Paulistana | SchemaIncompatibility | AnalysisOk | Skipped: schema is not loadable. |
| Paulistana | ConfigurationGap | BindingsPresent | Failed to load provider profile (rules.json missing or invalid). |
| Paulistana | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| Paulistana | SchemaIncompatibility | XsdValid | Skipped: schema is not loadable. |
| Simpliss | ConfigurationGap | SchemaLoadable | XSD directory not found: D:\exemplos\bkp\SemanaIA\tests\SemanaIA.ServiceInvoice.UnitTests\providers\Simpliss\xsd |
| Simpliss | SchemaIncompatibility | AnalysisOk | Skipped: schema is not loadable. |
| Simpliss | ConfigurationGap | BindingsPresent | Failed to load provider profile (rules.json missing or invalid). |
| Simpliss | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| Simpliss | SchemaIncompatibility | XsdValid | Skipped: schema is not loadable. |
| Webiss | ConfigurationGap | SchemaLoadable | XSD directory not found: D:\exemplos\bkp\SemanaIA\tests\SemanaIA.ServiceInvoice.UnitTests\providers\Webiss\xsd |
| Webiss | SchemaIncompatibility | AnalysisOk | Skipped: schema is not loadable. |
| Webiss | ConfigurationGap | BindingsPresent | Failed to load provider profile (rules.json missing or invalid). |
| Webiss | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| Webiss | SchemaIncompatibility | XsdValid | Skipped: schema is not loadable. |
| _Shared | ConfigurationGap | SchemaLoadable | XSD directory not found: D:\exemplos\bkp\SemanaIA\tests\SemanaIA.ServiceInvoice.UnitTests\providers\_Shared\xsd |
| _Shared | SchemaIncompatibility | AnalysisOk | Skipped: schema is not loadable. |
| _Shared | ConfigurationGap | BindingsPresent | Failed to load provider profile (rules.json missing or invalid). |
| _Shared | EngineGap | RuntimeProducible | Skipped: analysis or bindings not available. |
| _Shared | SchemaIncompatibility | XsdValid | Skipped: schema is not loadable. |
