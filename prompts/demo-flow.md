# Prompts para demo

## 1. Ler spec
Leia `openspec/nfse-serializer.spec.md` e `openspec/acceptance-criteria.md`.
Resuma objetivo, critérios, riscos e backlog técnico inicial.

## 2. Ler contrato
Analise o YAML enviado para a POC e proponha os DTOs C# mínimos da primeira iteração.

## 3. Ler serializer legado
Analise o serializer atual e diga quais responsabilidades devem ser extraídas para builders e rules.

## 4. Propor modelo canônico
Proponha um modelo canônico para geração do XML nacional contendo provider, borrower, service e values.

## 5. Implementar
Implemente a primeira iteração do fluxo request -> mapper -> modelo canônico -> builders XML.

## 6. Testar
Gere testes em BDD + 3A cobrindo pelo menos conta/tomador nacional, serviço e valores.
