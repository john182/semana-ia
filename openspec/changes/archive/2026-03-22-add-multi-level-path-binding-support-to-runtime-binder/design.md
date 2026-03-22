## Context

O `ServiceInvoiceSchemaDataBinder` converte `DpsDocument` em `Dictionary<string, object?>` usando bindings do `ProviderProfile`. O Nacional usa paths diretos como `infDPS.tpAmb` → `Environment`. O serializer consome esse dicionário e navega pelo schema emitindo elementos.

ISSNet usa o mesmo schema nacional (`sped.fazenda.gov.br/nfse`) mas envelopa em:
```
EnviarLoteDpsEnvio (root, inline type)
  └── LoteDps (tcLoteDps)
       ├── NumeroLote (obrigatório)
       ├── Prestador (tcIdentificacaoPrestadorDps, obrigatório)
       │    └── CNPJ/CPF (choice)
       ├── QuantidadeDps (obrigatório)
       └── ListaDps (inline type)
            └── DPS (TCDPS, unbounded)
                 └── infDPS (TCInfDPS)
```

O serializer espera encontrar `LoteDps.NumeroLote`, `LoteDps.Prestador.CNPJ`, etc. no dicionário. Mas o binder só gera paths como `infDPS.tpAmb`, sem os wrappers.

## Goals / Non-Goals

**Goals:**
- Permitir que o binder gere dados para wrapper elements intermediários via configuração JSON
- Suportar path prefix para remap de bindings do domínio para paths mais profundos
- ISSNet avançar de FAIL para PASS ou ter gap reduzido e diagnosticado
- Nacional, ABRASF e GISSOnline continuarem PASS (zero regressão)

**Non-Goals:**
- Gerar wrappers automaticamente a partir do schema (sem inferência — usa configuração explícita)
- Suportar lotes com múltiplos DPS (apenas 1 DPS por lote nesta fase)
- Onboarding completo de Paulistana e Simpliss

## Decisions

### 1. Wrapper bindings como seção no base-rules.json

**Decisão:** Adicionar seção `wrapperBindings` no `base-rules.json` que define valores estáticos e mapeamentos para wrapper elements.

**Formato:**
```json
{
  "wrapperBindings": {
    "LoteDps.NumeroLote": "1",
    "LoteDps.Prestador.CNPJ": "Provider.Cnpj | padLeft:14:0",
    "LoteDps.QuantidadeDps": "1"
  }
}
```

**Alternativa considerada:** Configuração de wrapper hierárquico com `rootWrapper.children`. Rejeitada por ser mais complexa e não reutilizar o formato de binding já existente.

**Racional:** Reutiliza exatamente o mesmo formato de binding já usado na seção `bindings` (path → expression com pipes). O binder processa `wrapperBindings` antes de `bindings`, gerando dados para os níveis intermediários.

### 2. Path prefix para rebasing de bindings

**Decisão:** Adicionar campo `bindingPathPrefix` no `base-rules.json`. Quando presente, todos os paths de `bindings` são prefixados automaticamente.

**Exemplo:** Se `bindingPathPrefix = "LoteDps.ListaDps.DPS"`, o binding `infDPS.tpAmb` se torna `LoteDps.ListaDps.DPS.infDPS.tpAmb`.

**Alternativa considerada:** Duplicar todos os bindings do Nacional com o prefixo completo na seção `bindings` do ISSNet. Rejeitada por violar DRY — os bindings internos do DPS são idênticos ao Nacional.

**Racional:** O Nacional e o ISSNet compartilham a mesma estrutura de DPS/infDPS. A diferença é apenas o envelope. O prefixo permite reutilizar exatamente os mesmos bindings do Nacional dentro do ISSNet.

### 3. Processamento de wrapperBindings no binder

**Decisão:** O `ServiceInvoiceSchemaDataBinder.Bind()` processa `wrapperBindings` primeiro, depois aplica `bindingPathPrefix` a cada path de `bindings`, e finalmente processa os bindings normais.

**Racional:** Garante que os dados de wrapper já existem no dicionário quando o serializer começa a navegar pelo schema. A ordem é: wrappers → dados internos.

### 4. Sem mudanças no serializer

**Decisão:** Nenhuma mudança no `SchemaBasedXmlSerializer`. O serializer já navega corretamente por qualquer profundidade de schema — o problema é apenas que o dicionário de dados não tinha os paths intermediários.

**Racional:** A mudança é 100% na camada de binding/input, não na camada de serialização.

## Risks / Trade-offs

- **[Risco] Regressão em Nacional/ABRASF/GISSOnline** → Mitigation: `bindingPathPrefix` e `wrapperBindings` são opcionais. Quando ausentes, o comportamento é idêntico ao atual.

- **[Risco] ISSNet schema pode ter requisitos adicionais** → Mitigation: O schema ISSNet é baseado no Nacional (`sped.fazenda.gov.br/nfse`). A estrutura interna de DPS é idêntica. Os wrappers são a única diferença estrutural.

- **[Trade-off] Configuração explícita vs inferência** → Preferimos configuração explícita por ser previsível e debuggable, embora exija manutenção manual do `base-rules.json` por provider.
