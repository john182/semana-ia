## MODIFIED Requirements

### Requirement: Versao attribute emission

O serializer MUST NOT emitir `versao` no root element quando o root complex type do schema NÃO declara `versao` nos seus `Attributes`. Para providers ABRASF criados via API (issnet, gissonline), o root `EnviarLoteRpsEnvio` não tem `versao` — deve ser omitido.

#### Scenario: Nacional root emits versao
- **WHEN** o root type `TCDPS` declara `versao` como atributo
- **THEN** o XML emite `<DPS versao="1.01">`

#### Scenario: ISSNet root omits versao
- **WHEN** o root type `EnviarLoteRpsEnvio` NÃO declara `versao`
- **THEN** o XML emite `<EnviarLoteRpsEnvio>` sem atributo versao

### Requirement: ABRASF envelope completeness

O serializer + pipeline MUST produzir envelope completo para issnet/gissonline. O `LoteRps` MUST conter `ListaRps` com dados vinculados via `BindingPathPrefix` e `WrapperBindings`.

#### Scenario: ISSNet envelope has LoteRps with content
- **WHEN** o provider issnet é resolvido e serializado
- **THEN** o XML contém `<EnviarLoteRpsEnvio><LoteRps>...</LoteRps></EnviarLoteRpsEnvio>` com conteúdo
