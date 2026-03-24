## Context

O SemanaIA possui modelos simplificados (`DpsDocument`, `NfseGenerateXmlRequest`) criados durante o MVP para provar a engine de geração XML. O sistema de produção real (`nfeio-service-invoices`) possui modelos maduros (`ServiceInvoice`, `ServiceInvoiceIssueMessage`) com ~56 tipos, enums com valores numéricos específicos, campos calculados e ciclo de vida completo.

**Gap principal**: A estrutura `Values` agrupa campos fiscais que na produção são flat no `ServiceInvoice`. Enums são `int` ou `string` no SemanaIA mas enums tipados com valores flags na produção. Sub-modelos como `Lease`, `ForeignTrade`, `Suspension` usam `int` para categorias que na produção são enums descritivos.

## Goals / Non-Goals

**Goals:**
- Criar documentação de referência dos modelos de produção para que agentes de IA tenham contexto correto
- Alinhar `DpsDocument` com `ServiceInvoice` de produção (campos, tipos, estrutura)
- Alinhar `NfseGenerateXmlRequest` com `ServiceInvoiceIssueMessage` de produção
- Manter compatibilidade com a engine de serialização XML (bindings, rules)
- Atualizar testes e exemplos para refletir novos modelos

**Non-Goals:**
- Implementar ciclo de vida de emissão (FlowStatus, events) — fora de escopo
- Implementar persistência do domain (MongoDB) — já existe para providers
- Copiar lógica de negócio (cálculo de impostos na descrição, hash de assinatura)
- Replicar camada de resources/mapper do projeto de produção

## Decisions

### D1: Flatten `Values` para campos diretos no `DpsDocument`

**Decisão**: Remover a classe `Values` e mover campos fiscais para o nível raiz do `DpsDocument`, espelhando produção.

**Racional**: Na produção, `ServiceInvoice` tem campos como `ServicesAmount`, `IssRate`, `IssTaxAmount` diretamente na raiz. O agrupamento em `Values` foi uma simplificação do MVP que agora gera divergência de paths nos bindings.

**Alternativa descartada**: Manter `Values` como wrapper e criar adapter — adicionaria complexidade sem valor.

**Impacto**: `ServiceInvoiceSchemaDataBinder` precisa atualizar paths de `Values.ServicesAmount` para `ServicesAmount`. Rules dos providers precisam ajustar source paths.

### D2: Usar enums tipados com valores numéricos de produção

**Decisão**: Substituir enums simplificados por enums com os mesmos valores numéricos de produção. Ex: `TaxationType` com flags (`WithinCity=1`, `OutsideCity=2`, `Export=4`, etc.).

**Racional**: Os valores numéricos são usados para mapear para códigos XML. Se divergirem, as regras `EnumMapping` ficam incompatíveis.

**Alternativa descartada**: Manter enums como string na request e converter — adiciona camada de conversão desnecessária.

### D3: Unificar `Provider`, `Borrower`, `Person` em uma única classe `Person`

**Decisão**: Remover `Provider` e `Borrower` como classes separadas. Usar uma única classe `Person` para todos os papéis: prestador, tomador, intermediário e destinatário.

**Racional**: Prestador, tomador, intermediário e destinatário são todos pessoas (física ou jurídica). Ter classes separadas com 80% dos campos repetidos é duplicidade desnecessária. A diferença entre PF e PJ é resolvida por `PersonType` (inferido pelo tamanho do `FederalTaxNumber`). Campos específicos de PJ (`TaxRegime`, `SpecialTaxRegime`, `TradeName`, etc.) ficam nullable — preenchidos apenas quando aplicável.

**Alternativa descartada**: Manter classes separadas como na produção (`LegalPerson`, `Borrower`, `PartyResource`) — a produção já sofre com essa duplicidade.

**Impacto**: Bindings mudam de `Provider.Cnpj` para `Provider.FederalTaxNumber` (uniforme). `DpsDocument` usa `Person` para Provider, Borrower, Intermediary e Recipient.

**Campos da `Person` unificada**: `Name`, `FederalTaxNumber`, `Email`, `PhoneNumber`, `Address`, `MunicipalTaxNumber`, `StateTaxNumber`, `Caepf`, `Nif`, `NoTaxIdReason`, `ServiceTakerType`, `TaxRegime`, `SpecialTaxRegime`, `LegalNature`, `TradeName`, `CompanyRegistryNumber`, `RegionalTaxNumber`, `MunicipalTaxId`, `FederalTaxDetermination`, `MunicipalTaxDetermination`.

### D3b: Unificar `Location` e `Address` em uma única classe `Address`

**Decisão**: Remover `Location` como classe separada. Usar uma única classe `Address` para todos os endereços: pessoa, local de prestação, obra, evento.

**Racional**: `Location` e `Address` têm exatamente os mesmos campos. Hoje `Address : Location` é uma herança vazia. Uma classe só elimina a duplicidade e simplifica o modelo.

**Impacto**: `DpsDocument.Location` muda de tipo `Location` para `Address`. Renaming simples sem mudança de campos.

### D4: Documentação de referência como markdown na wiki

**Decisão**: Criar `docs/wiki/Modelos-de-Producao.md` com mapeamento completo.

**Racional**: Arquivo na wiki é carregado automaticamente pelos agentes via MCP e fica versionado no repo.

### D5: Request mantém estrutura flat (não agrupa em sub-objetos como produção)

**Decisão**: `NfseGenerateXmlRequest` permanece com campos escalares no nível raiz (como já está) mas adiciona campos faltantes. Grupos complexos (Lease, Construction, etc.) continuam como sub-objetos.

**Racional**: O request atual já espelha bem o `ServiceInvoiceIssueMessage` de produção que também tem campos flat + grupos complexos. Minimiza breaking changes.

### D6: Fase 1 antes de Fase 2

**Decisão**: A documentação de referência (Fase 1) DEVE ser criada antes de iniciar as mudanças de código (Fase 2).

**Racional**: Os agentes de IA precisam do contexto para gerar código correto. Sem a documentação, mudanças de código repetirão os mesmos erros de divergência.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| **Flatten de Values quebra todos os bindings** | Atualizar rules.json de todos os 7 providers junto com a mudança. Script de migração se necessário. |
| **Enums com flags podem confundir serialização XML** | Manter mapeamento explícito em `EnumMapping` rules. Não usar flags compostos na serialização — apenas valores base. |
| **Volume de mudança é alto (domain + request + binder + rules + tests)** | Dividir em tasks incrementais. Fase 1 (docs) não quebra nada. Fase 2 usa feature branch. |
| **Testes snapshot (golden master) falham em massa** | Regenerar snapshots após mudança. Validar contra XSD que é a fonte de verdade. |
| **Campos adicionados podem não ter binding XSD** | Campos novos sem correspondência XSD ficam ignorados pela engine — sem risco. Documentar como pendentes. |
