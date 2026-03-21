# Change: create-manual-nfse-national-serializer

## Why

O projeto possui um serializer mínimo (`NationalDpsXBuilderXmlBuilder`) que gera apenas um subconjunto do DPS XML usando XBuilder dinâmico. Este approach não cobre os grupos exigidos pelo XSD nacional (intermediário, endereço do prestador, tributos federais, IBSCBS, deduções, etc.) e não é extensível para futuras evoluções. É necessário criar uma estrutura manual de serialização organizada em blocos reutilizáveis que cubra o fluxo completo `ServiceInvoice → DPS XML` antes de migrar para geração em build a partir de XSD.

## What Changes

- Criar o namespace `SemanaIA.ServiceInvoice.XmlGeneration.Manual` com um serializer manual que substitui o builder atual para o fluxo nacional.
- Criar blocos de serialização XML reutilizáveis em `SemanaIA.ServiceInvoice.XmlGeneration.Builders/` organizados por responsabilidade (Provider, Borrower, Intermediary, Service, Values/Tributos, Address).
- Expandir o modelo de domínio `DpsDocument` com os campos necessários para os blocos faltantes (intermediário, tributos federais, dados de endereço do prestador).
- Criar testes unitários que validem a geração XML para cenários mínimo e completo.
- Manter o XBuilder como engine de geração XML (infraestrutura já existente).

## Capabilities

### New Capabilities

_(nenhuma — a capacidade `nfse-serializer-manual` já existe)_

### Modified Capabilities

- `nfse-serializer-manual`: Implementação inicial dos requisitos funcionais RF-001 a RF-005 da spec — serializer manual com blocos para provider, borrower, intermediary, service, values/tributos e address, além de testes iniciais.

## Impact

- **Domain**: `DpsDocument` e modelos associados em `SemanaIA.ServiceInvoice.Domain` serão expandidos com campos para intermediário, tributos federais e endereço do prestador.
- **XmlGeneration**: Novo serializer manual e blocos de builder em `SemanaIA.ServiceInvoice.XmlGeneration`.
- **Tests**: Novo projeto ou expansão de `SemanaIA.ServiceInvoice.UnitTests` com testes de serialização XML.
- **API**: Nenhuma alteração na camada API/Swagger — o mapper existente continuará funcionando. O novo serializer será invocado internamente.
- **Dependências**: Nenhuma nova dependência de pacote — usa `System.Xml.Linq` e o XBuilder já existente.