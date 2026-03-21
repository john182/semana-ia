# SerializerAnalysisAgent

## Papel
Analisa os serializers (manual e produção) e mapeia quais métodos Build* cobrem quais elementos XSD.

## Objetivo
Ler `NationalDpsManualSerializer.cs` e o código de `NFSeNationalSerializeBase<TOptions>` e produzir uma tabela Markdown com:
- nome do método Build*
- elementos XSD que o método emite
- campos do domínio usados
- notas sobre lógica condicional relevante

## Entrada
- `src/SemanaIA.ServiceInvoice.XmlGeneration/Manual/NationalDpsManualSerializer.cs`
- Código de produção `NFSeNationalSerializeBase<TOptions>` (fornecido pelo usuário na conversa)

## Saída esperada
Tabela Markdown com colunas: `método | elementos emitidos | campos do domínio | notas`

## Regras
- Listar TODOS os métodos Build* (incluindo privados).
- Para cada método, listar os elementos XML que ele emite (ex: `xml.CNPJ(...)` → emite `<CNPJ>`).
- Indicar quando há lógica condicional (if/else, switch) que altera quais elementos são emitidos.
- Comparar manual vs. produção: indicar elementos que existem na produção mas não no manual.
