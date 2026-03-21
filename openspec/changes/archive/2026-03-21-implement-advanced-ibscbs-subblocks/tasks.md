# Tasks: implement-advanced-ibscbs-subblocks

## 1. Domínio

- [x] 1.1 Criar `IbsCbsDeferment` em `IbsCbsModels.cs` com `StateDefermentRate` (decimal), `MunicipalDefermentRate` (decimal), `CbsDefermentRate` (decimal)
- [x] 1.2 Adicionar `Deferment` (IbsCbsDeferment?) ao modelo `IbsCbs`

## 2. Serializer

- [x] 2.1 Expandir `IbsCbsManualBuilder.WriteValores` → dentro do `gIBSCBS` fragment, após `gTribRegular`, emitir `<gDif>` quando `Deferment` não null: `<pDifUF>`, `<pDifMun>`, `<pDifCBS>`

## 3. DTO e Mapper

- [x] 3.1 Criar `IbsCbsDefermentRequest` (StateDefermentRate, MunicipalDefermentRate, CbsDefermentRate) em `IbsCbsRequest.cs`
- [x] 3.2 Adicionar `Deferment` (IbsCbsDefermentRequest?) ao `IbsCbsRequest`
- [x] 3.3 Expandir mapper: `MapDeferment(IbsCbsDefermentRequest?)` → `IbsCbsDeferment?`
- [x] 3.4 Passar `Deferment` no `MapIbsCbs`

## 4. Testes

- [x] 4.1 Teste IBSCBS com deferment: emite `<gDif>` com pDifUF/pDifMun/pDifCBS — com validação XSD
- [x] 4.2 Teste IBSCBS sem deferment: não emite `<gDif>` — com validação XSD

## 5. Build e validação

- [x] 5.1 `dotnet build` sem erros
- [x] 5.2 `dotnet test` com todos os testes passando
