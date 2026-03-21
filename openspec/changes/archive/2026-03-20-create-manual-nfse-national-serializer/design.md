# Design: create-manual-nfse-national-serializer

## Context

O projeto possui um serializer funcional (`NationalDpsXBuilderXmlBuilder`) que gera XML DPS via XBuilder dinâmico. Ele cobre apenas provider (CNPJ), borrower (CPF), service e values com tributação ISS básica. O modelo de domínio `DpsDocument` reflete esse escopo mínimo.

O XSD nacional exige blocos adicionais: intermediário, endereço do prestador, tributos federais (PIS, COFINS, IR, CSLL, INSS), IBSCBS condicional, além de regras de seleção de documento do tomador (CNPJ/CPF/NIF/cNaoNIF).

A infraestrutura XBuilder (`SemanaIA.ServiceInvoice.Infrastructure.Xml`) já existe e será mantida como engine de geração.

## Goals / Non-Goals

**Goals:**

- Implementar um serializer manual organizado em blocos que cubra os requisitos RF-001 a RF-005 da spec `nfse-serializer-manual`.
- Expandir `DpsDocument` com os campos necessários para os novos blocos.
- Criar blocos de builder separados por responsabilidade, cada um testável isoladamente.
- Cobrir cenários mínimo e completo com testes unitários baseados em snapshot XML.

**Non-Goals:**

- Geração automática a partir de XSD (spec `nfse-serializer-build-generation`).
- Grupo IBSCBS completo — expor apenas o ponto de integração (`BuildIbsCbs` stub).
- Assinatura XML — fora do escopo desta change.
- Substituir o serializer atual em produção — esta é uma POC paralela.

## Decisions

### D-01 — Manter XBuilder como engine

**Decisão**: Continuar usando `XBuilder` dinâmico para construção do XML.
**Alternativa**: Usar `XDocument`/`XElement` diretamente.
**Razão**: XBuilder já está integrado, oferece sintaxe fluente com `Fragment()` e o builder atual já o utiliza. Migrar para XElement seria trabalho sem ganho nesta fase.

### D-02 — Um builder class com métodos estáticos por bloco

**Decisão**: Criar `NationalDpsManualSerializer` como classe principal e métodos `Build*` estáticos internos para cada bloco XML (Provider, Borrower, Intermediary, Address, Service, Values, Tributos).
**Alternativa**: Uma classe por bloco em arquivos separados (strategy pattern).
**Razão**: O volume de código por bloco é pequeno (5-15 linhas). Classes separadas adicionariam overhead de arquivos sem benefício real. Se um bloco crescer significativamente, extrair em change futura.

### D-03 — Expansão aditiva do modelo de domínio

**Decisão**: Adicionar campos opcionais (nullable) a `DpsDocument` e modelos associados. Não criar novos modelos de domínio.
**Alternativa**: Criar um modelo intermediário `NationalDpsDocument` separado.
**Razão**: O modelo existente é simples e a expansão é aditiva — nenhum campo existente é alterado. Um modelo separado duplicaria dados sem justificativa nesta fase.

### D-04 — Regras de seleção de documento por tipo

**Decisão**: A seleção de elemento XML para o documento fiscal (CNPJ vs CPF vs NIF vs cNaoNIF) será baseada no campo `FederalTaxNumber` length/value + um novo campo `DocumentType` string no modelo `Borrower`.
**Razão**: O XSD exige exatamente um dos quatro elementos. A lógica deve ser explícita no serializer.

### D-05 — Testes por snapshot XML

**Decisão**: Testes unitários comparam XML gerado contra strings esperadas (snapshot inline). Usar `XDocument.Parse()` + `XNode.DeepEquals()` para comparação semântica.
**Alternativa**: Validação contra XSD nos testes.
**Razão**: Validação XSD é objetivo futuro (change dedicada). Snapshots são mais rápidos de implementar e suficientes para verificar a estrutura nesta fase.

## Risks / Trade-offs

- **[Divergência com XSD]** → O serializer manual pode divergir do XSD real por falta de validação automática. Mitigação: os snapshots de teste serão baseados na estrutura do XSD, e validação XSD será adicionada em change futura.
- **[Modelo de domínio crescendo]** → Adicionar campos a `DpsDocument` pode torná-lo grande. Mitigação: campos são opcionais e agrupados semanticamente. Refatoração para sub-objetos pode ser feita incrementalmente.
- **[XBuilder quirks]** → XBuilder tem comportamento especial com `xmlns` vazio e precisa de `RemoveEmptyXmlnsOnOutput`. Mitigação: manter o mesmo pattern do builder atual.

## Estrutura de arquivos

```
src/SemanaIA.ServiceInvoice.Domain/
  Models/ServiceInvoice.cs                          ← expandir DpsDocument, Borrower, Provider, Values

src/SemanaIA.ServiceInvoice.XmlGeneration/
  Manual/
    NationalDpsManualSerializer.cs                  ← serializer principal + métodos Build* por bloco

tests/SemanaIA.ServiceInvoice.UnitTests/
  XmlGeneration/Manual/
    NationalDpsManualSerializerTests.cs             ← testes snapshot (mínimo + completo)
```

## Blocos do serializer

| Método Build*        | Elemento XSD        | Campos de DpsDocument usados                       |
|----------------------|---------------------|---------------------------------------------------|
| `BuildProvider`      | `prest`             | Provider.Cnpj/Cpf, IM, regTrib                    |
| `BuildBorrower`      | `toma`              | Borrower.*, DocumentType                           |
| `BuildIntermediary`  | `interm`            | Intermediary.* (condicional)                       |
| `BuildAddress`       | `end` / `endNac`    | Address.*                                          |
| `BuildService`       | `serv`              | Service.*                                          |
| `BuildValues`        | `valores`           | Values.*, tributos federais                        |
| `BuildTributos`      | `trib` / `tribMun`  | Values.TaxationType, IssRate, retenções            |