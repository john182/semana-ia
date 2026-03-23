## Context

Os 3 MVP providers (national, issnet, gissonline) passam nos testes E2E com filtros de gaps conhecidos (`IsKnownXsdGap`). Para fechar o MVP, os gaps precisam ser corrigidos e os filtros removidos.

## Goals / Non-Goals

**Goals:**
- Enum fields emitem código numérico no XML (não nome do enum)
- Versao não emitido em roots ABRASF que não declaram o atributo
- DataEmissao formatado como `xs:date` quando o schema exige
- Envelope issnet/gissonline completo com LoteRps populado
- `IsKnownXsdGap` removido — assertion forte para MVP providers
- 0 falhas em unit + integration tests

**Non-Goals:**
- Resolver gaps de providers fora do MVP (45 providers restantes)
- Refatorar o TypedRuleResolver além do necessário
- Criar EnumMapping rules para todos os enums possíveis

## Decisions

### 1. Enum-to-int via auto-gen EnumMapping rules

O `DpsDocumentFieldResolver.ResolvePropertyPath` já converte enums para int (line 44). Mas o `CommonFieldMappingDictionary` mapeia `opSimpNac → Provider.TaxRegime` como **Binding**, não como **EnumMapping**. O `TypedRuleResolver.ApplyBindingFormatting` já converte enums para int (adicionado no commit anterior). Se isso funciona nos unit tests mas não na API, o problema está no fluxo da API.

**Investigação**: Verificar se o profile salvo no Mongo preserva as rules corretamente e se o `TypedRuleResolver` é usado no pipeline da API.

### 2. Versao em ABRASF roots

O `RootTypeHasVersaoAttribute` já foi implementado. Se não funciona para API, o root type do schema pode não ter `Attributes` populados quando vem do Mongo. Investigar se a materialização reconstrói o schema corretamente.

### 3. Date format inference

Adicionar ao `CommonFieldMappingDictionary` o formato correto para `DataEmissao` ABRASF: `yyyy-MM-dd`. O nacional já usa `dhEmi` com formato datetime. O campo `DataEmissao` é ABRASF-específico e deve usar date format.

### 4. Envelope completeness

Verificar se o auto-gen produz `WrapperBindings` e `BindingPathPrefix` corretos para issnet/gissonline quando criados via API. Se não, corrigir a materialização do profile.
