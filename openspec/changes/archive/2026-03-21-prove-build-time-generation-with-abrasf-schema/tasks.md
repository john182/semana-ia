# Tasks: prove-build-time-generation-with-abrasf-schema

## 1. Expandir SchemaModel com SimpleType restrictions

- [x] 1.1 Expandir `SchemaElement` record com propriedade `Restriction: SchemaSimpleTypeRestriction?`
- [x] 1.2 Expandir `XsdSchemaAnalyzer` para capturar SimpleType restrictions: quando o elemento referencia um SimpleType com `XmlSchemaSimpleTypeRestriction`, extrair enumerações, pattern, minLength, maxLength
- [x] 1.3 Associar a restriction ao `SchemaElement` correspondente no modelo

## 2. Customizar base-rules.json do ABRASF

- [x] 2.1 Reescrever `providers/abrasf/rules/base-rules.json` com dados do ABRASF: provider="abrasf", namespace="http://www.abrasf.org.br/nfse.xsd", version, e apenas complementos que o XSD não expressa
- [x] 2.2 Manter o JSON mínimo — não duplicar informações que o XSD já contém (enumerações, lengths, patterns)

## 3. SchemaGenerationRunner

- [x] 3.1 Criar `SchemaGenerationRunner` em `XmlGeneration/SchemaEngine/` com método `RunForProvider(string providerName, string providersBaseDir)` que: localiza XSDs, analisa, carrega rules, gera records + skeleton + report
- [x] 3.2 O runner grava output em `providers/{provider}/generated/`
- [x] 3.3 Adicionar `providers/*/generated/` ao `.gitignore`

## 4. Executar geração ABRASF

- [x] 4.1 Executar o runner para o provider ABRASF via teste
- [x] 4.2 Verificar que records foram gerados para os complexTypes do ABRASF
- [x] 4.3 Verificar que o builder skeleton foi gerado com métodos Build*
- [x] 4.4 Verificar que o schema-report.md foi gerado

## 5. Testes de análise e geração

- [x] 5.1 Criar `AbrasfSchemaGenerationTests` em `tests/.../SchemaEngine/`
- [x] 5.2 Teste: Given_AbrasfXsd_Should_ProduceSchemaDocumentWithAbrasfComplexTypes
- [x] 5.3 Teste: Given_AbrasfXsd_Should_CaptureSimpleTypeRestrictions (enumerações, patterns)
- [x] 5.4 Teste: Given_AbrasfProvider_Should_GenerateArtifactsViaRunner
- [x] 5.5 Teste: Given_NacionalProvider_Should_StillGenerateCorrectly (regressão)

## 6. Testes de validação XML do serializer gerado

- [x] 6.1 Para o provider nacional: gerar um builder skeleton funcional mínimo que use XBuilder para produzir XML, executá-lo e validar o XML resultante contra DPS_v1.01.xsd via ShouldBeValidAgainstDpsSchema()
- [x] 6.2 Para o provider ABRASF: gerar um builder skeleton funcional mínimo, executá-lo e validar o XML resultante contra os XSDs do ABRASF
- [x] 6.3 Teste de choice: dado um complexType com choice group (ex: CNPJ/CPF/NIF/cNaoNIF em TCInfoPrestador ou TCInfoPessoa), o builder gerado MUST emitir exatamente um dos elementos do choice — validar que o XML com mais de um elemento do mesmo choice é rejeitado pelo XSD
- [x] 6.4 Teste de sequence: dado um complexType com sequence (ex: TCInfDPS com tpAmb, dhEmi, verAplic, serie, nDPS em ordem), o builder gerado MUST emitir os elementos na ordem do XSD — validar que o XML com ordem incorreta é rejeitado pelo XSD
- [x] 6.5 Teste de obrigatoriedade: dado um complexType com elementos required, o builder gerado MUST emitir todos — validar que XML sem elemento obrigatório é rejeitado pelo XSD
- [x] 6.6 Teste de restrições SimpleType: dado um elemento com pattern/length/enumeration, o builder gerado MUST respeitar — validar que valor fora da restrição é rejeitado pelo XSD

## 7. Build e validação final

- [x] 7.1 `dotnet build` sem erros
- [x] 7.2 `dotnet test` com todos os testes passando (existentes + novos)
- [x] 7.3 Verificar que `providers/*/generated/` está no `.gitignore` e nenhum artefato gerado está rastreado pelo git
