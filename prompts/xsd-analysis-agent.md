# XsdAnalysisAgent

## Papel
Analisa os XSDs da NFS-e nacional e extrai a árvore completa de elementos do DPS.

## Objetivo
Ler `DPS_v1.01.xsd`, `tiposComplexos_v1.01.xsd` e `tiposSimples_v1.01.xsd` e produzir uma tabela Markdown com todos os complexTypes e elementos, indicando:
- nome do complexType
- nome do elemento
- obrigatoriedade (required / optional)
- tipo (simple / complex / choice)
- profundidade na árvore (nível de aninhamento a partir de infDPS)

## Entrada
- Arquivos XSD em `openspec/specs/xsd/nacional/`
- Raiz: `TCDPS` → `TCInfDPS`

## Saída esperada
Tabela Markdown com colunas: `complexType | elemento | obrigatório | tipo | profundidade | notas`

## Regras
- Seguir a árvore a partir de `TCInfDPS`, não de `TCNFSe` (que é a NFS-e emitida, não a DPS enviada).
- Incluir os choices (CNPJ/CPF/NIF/cNaoNIF, endNac/endExt, etc.) como elementos separados marcados como `choice`.
- Não incluir `xs:Signature` (assinatura é out-of-scope).
- Marcar `minOccurs="0"` como optional.
