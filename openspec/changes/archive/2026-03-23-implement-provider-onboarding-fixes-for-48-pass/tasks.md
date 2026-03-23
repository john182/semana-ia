## 1. Root Element de Envio (Fix 1)

- [ ] 1.1 Adicionar `CompNfse`, `compNfse`, `ConsultarNfse`, `SubstituirNfse` aos `ExcludePatterns` do `SendXsdSelector`
- [ ] 1.2 Adicionar padrões ABRASF (`RecepcionarLoteRps`, `GerarNfse`, `EnviarLoteRps`) aos `SendPatterns` com prioridade adequada
- [ ] 1.3 Escrever testes unitários para os novos padrões de exclusão e priorização
- [ ] 1.4 Validar que os 48 providers selecionam o XSD correto após as mudanças

## 2. Atributo versao (Fix 2)

- [ ] 2.1 Adicionar propriedade `RootVersionAttribute` ao `SchemaDocument`
- [ ] 2.2 No `XsdSchemaAnalyzer`, detectar atributo `versao` no root element complex type e extrair valor fixed/default
- [ ] 2.3 No `ProviderConfigGenerator`, usar `RootVersionAttribute` quando disponível, fallback para `DefaultVersion`
- [ ] 2.4 Escrever testes unitários para inferência de versão (fixed, default, ausente)

## 3. CommonFieldMappingDictionary (Fix 3)

- [ ] 3.1 Adicionar mapeamentos ABRASF: `tpRps`, `StatusRps`, `NumeroRps`, `SerieRps`, `DataEmissaoRps`
- [ ] 3.2 Adicionar mapeamentos de tax/serviço: `NaturezaOperacao`, `RegimeEspecialTributacao`, `MunicipioIncidencia`, `CodigoCnae`, `ValorIss`, `ValorDeducoes`
- [ ] 3.3 Adicionar mapeamentos auxiliares: `OutrasInformacoes`, `CodigoMunicipioIBGE`, `ValorPis`, `ValorCofins`, `ValorInss`, `ValorIr`, `ValorCsll`
- [ ] 3.4 Escrever testes unitários validando os novos mapeamentos

## 4. ProviderConfigGenerator no Teste Genérico (Fix 4)

- [ ] 4.1 Refatorar o fluxo de validação genérica para usar `ProviderConfigGenerator` quando não houver `base-rules.json`
- [ ] 4.2 Garantir que `base-rules.json` tem precedência sobre config gerado em runtime
- [ ] 4.3 Escrever testes para o fallback automático no fluxo de validação

## 5. XSDs de Dependência (Fix 5)

- [ ] 5.1 Identificar providers que falham por schema incompleto (import/include sem arquivo presente)
- [ ] 5.2 Copiar XSDs de dependência faltantes (tipos compartilhados, xmldsig) para os diretórios `providers/*/xsd/`
- [ ] 5.3 Validar que o `XmlSchemaSet` compila sem erros para todos os providers após a inclusão
- [ ] 5.4 Documentar providers que continuam com dependências não resolvíveis localmente

## 6. Namespace de Root Inline Type (Fix 6)

- [ ] 6.1 Ajustar `XsdSchemaAnalyzer` para propagar `targetNamespace` corretamente ao `rootInlineType`
- [ ] 6.2 Ajustar `SchemaBasedXmlSerializer` para usar `TargetNamespace` como fallback quando o inline type não tem namespace
- [ ] 6.3 Escrever testes unitários para namespace resolution com inline types proprietários

## 7. Validação Consolidada e Relatório

- [ ] 7.1 Implementar suite de validação que executa todos os 48 providers end-to-end
- [ ] 7.2 Gerar resumo sumarizado por provider: Schema Analysis, Runtime XML + XSD, Choice, Sequence, Status final, gap remanescente
- [ ] 7.3 Executar validação completa e documentar resultado final
- [ ] 7.4 Registrar backlog residual explícito com indicação de qual fix cada provider pendente ainda precisa
