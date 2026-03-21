# Change: prove-build-time-generation-with-gissonline-schema

## Why

A engine já foi validada com nacional e ABRASF. GISSOnline é o terceiro provider e utiliza um namespace e estrutura de schema próprios (`http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd`), com imports entre schemas distintos do padrão nacional. Provar que a engine funciona com GISSOnline valida a genericidade e confirma o padrão de onboarding por provider.

Os XSDs do GISSOnline já existem em `providers/gissonline/xsd/` (3 arquivos). O `base-rules.json` é uma cópia do ABRASF e precisa ser customizado.

## What Changes

- Customizar `providers/gissonline/rules/base-rules.json` com dados do GISSOnline (namespace, provider name).
- Executar `SchemaGenerationRunner` sobre o provider GISSOnline.
- Validar que a engine produz SchemaModel, records e builder skeleton.
- Criar testes que executam o runner para GISSOnline e validam saída.
- Criar testes de validação XML contra os XSDs do GISSOnline (choice, sequence, required).

## Capabilities

### New Capabilities

_(nenhuma)_

### Modified Capabilities

- `nfse-xsd-generation-engine`: Terceiro provider validado (GISSOnline). Confirma que o padrão de onboarding funciona com 3 providers distintos.

## Impact

- **Providers**: `base-rules.json` customizado para GISSOnline. Artefatos gerados em `providers/gissonline/generated/` (gitignored).
- **Tests**: Testes de geração e validação XML do GISSOnline.
- **Zero alteração** em código de produção, serializer manual, endpoint ou engine.
