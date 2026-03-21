# Delta Spec: nfse-serializer-manual

## MODIFIED Requirements

### Requirement: IBSCBS conditional inclusion

O serializer MUST emitir `<gDif>` (tipo `TCRTCInfoTributosDif`) dentro de `<gIBSCBS>` após `<gTribRegular>` quando dados de diferimento estiverem presentes. O bloco contém `<pDifUF>` (obrigatório), `<pDifMun>` (obrigatório) e `<pDifCBS>` (obrigatório), todos formatados como decimal (0.00).

#### Scenario: IBSCBS with deferment
- **WHEN** IbsCbs com Deferment preenchido (StateDefermentRate=0.50, MunicipalDefermentRate=0.00, CbsDefermentRate=0.20)
- **THEN** o XML contém `<gDif><pDifUF>0.50</pDifUF><pDifMun>0.00</pDifMun><pDifCBS>0.20</pDifCBS></gDif>` após `<gTribRegular>` ou `<cClassTrib>` dentro de `<gIBSCBS>`

#### Scenario: IBSCBS without deferment
- **WHEN** IbsCbs sem Deferment (null)
- **THEN** o XML não contém `<gDif>` dentro de `<gIBSCBS>`
