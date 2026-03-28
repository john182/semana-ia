# fix(tests): ExternalProviderXmlGenerationTests engolia falhas XSD com catch silencioso

**Labels:** bug, priority:high, tests
**Milestone:** Production Ready
**Status:** RESOLVIDO nesta change

## Problema (já corrigido)

O `ExternalProviderXmlGenerationTests.Given_ExternalProvider_Should_GenerateXmlAndValidateAgainstSchema` tinha um `catch (ShouldAssertException) { }` que engolia silenciosamente todas as falhas de validação XSD. O teste passava para 100% dos providers mesmo quando a engine produzia XML inválido.

```csharp
// ANTES (engolia falhas):
catch (Shouldly.ShouldAssertException)
{
    // Provider-specific validation issues are captured in the report test
}
```

## Correção aplicada

O catch foi removido. Agora o teste faz assertion direta:

```csharp
// DEPOIS (falha explícita):
context.ShouldNotBeNull($"[{providerName}] Schema preparation failed");
xml.ShouldNotBeNull($"[{providerName}/{scenario}] XML generation returned null");
xml.ShouldBeValidAgainstProviderSchema(context.XsdDir);
```

## Impacto

18 providers que antes passavam silenciosamente agora falham explicitamente. Isso expôs 3 categorias de gaps na engine que foram registradas como issues separadas:
- Ordem de elementos no envelope (8 providers)
- Dummy values não respeitam constraints XSD (4 providers)
- Schema não carregável / XML null (6 providers)

## Arquivos alterados

- `tests/SemanaIA.ServiceInvoice.UnitTests/Engine/ProviderConfig/ExternalProviderXmlGenerationTests.cs`
- `tests/SemanaIA.ServiceInvoice.UnitTests/Providers/AllDataProvidersFillingVariationsTests.cs` (novo, sem skip silencioso)
