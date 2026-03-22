## Context

A aplicação atual está hardcoded: `GenerateNfseXmlUseCase` injeta `NationalDpsManualSerializer` e ignora completamente a engine de runtime. O `SchemaSerializationPipeline` já funciona para 4 providers mas não é chamado pelo fluxo da aplicação. O controller não recebe qual provider usar. Não existe discovery de providers na pasta `providers/`.

O objetivo é conectar a engine schema-driven ao fluxo real da aplicação, com resolução dinâmica de provider e validação automática de onboarding.

## Goals / Non-Goals

**Goals:**
- Request chega ao controller com indicação do provider → engine resolve e gera XML
- Discovery automático de providers a partir da pasta `providers/`
- Validação de onboarding: schema ok? bindings ok? runtime XML ok?
- Paulistana e Simpliss configurados com bindings mínimos
- Testes end-to-end: Request → Provider → XML → XSD
- Classificação clara de gaps por provider
- 6/6 providers com pelo menos schema analysis

**Non-Goals:**
- UI/dashboard para onboarding (é API/CLI apenas)
- Suportar hot-reload de providers em runtime
- Eliminar completamente a necessidade de desenvolvimento para cases extremos
- Substituir o baseline manual nacional no endpoint de produção

## Decisions

### 1. ProviderResolver como discovery service

**Decisão:** Criar `ProviderResolver` no projeto XmlGeneration que descobre providers disponíveis escaneando a pasta `providers/`, verifica se possuem XSD + rules, e resolve qual usar a partir do nome do provider na request.

**Alternativa considerada:** Registry em appsettings.json.

**Racional:** A pasta `providers/` já é a fonte de verdade. Escanear o filesystem é mais simples e elimina a necessidade de manter configuração duplicada. O suporte adiciona arquivos na pasta e o provider está disponível.

### 2. ProviderSerializerFactory no projeto XmlGeneration

**Decisão:** Criar factory que recebe o nome do provider e retorna uma instância do `SchemaSerializationPipeline` configurada para aquele provider. Usa o `ProviderResolver` para localizar o diretório.

**Alternativa considerada:** Injetar `SchemaSerializationPipeline` diretamente no UseCase.

**Racional:** A factory encapsula a resolução de diretório, carregamento de profile e instanciação do pipeline. O UseCase não precisa saber detalhes do filesystem.

### 3. Resolução de provider por código de município (IBGE)

**Decisão:** O provider é resolvido automaticamente a partir do código de município (IBGE) do prestador presente no `DpsDocument` (`Provider.MunicipalityCode` ou `cLocEmi`). Cada provider declara no seu `base-rules.json` quais códigos de município atende (seção `municipalityCodes`). O `ProviderResolver` mapeia código de município → provider.

**Formato no base-rules.json:**
```json
{
  "provider": "gissonline",
  "municipalityCodes": ["3550308", "3509502", "..."],
  ...
}
```

Para o provider "nacional" (NFS-e Nacional / SPED), o mapeamento é o fallback default — qualquer município que não tenha provider específico usa o nacional.

**Alternativa considerada:** Provider name explícito na request HTTP.

**Racional:** No domínio de NFS-e, cada município contrata um provedor de serviço (GISSOnline, ISSNet, Simpliss, etc.). O prestador de serviço não escolhe o provider — ele é determinado pelo município onde o serviço é prestado. O código IBGE já está presente no `DpsDocument` como informação obrigatória do documento fiscal, então a resolução é automática e transparente.

### 4. ProviderOnboardingValidator como diagnóstico

**Decisão:** Criar validador que recebe o nome de um provider e produz um `OnboardingReport` com status em cada etapa: schema loadable? analysis ok? bindings present? runtime XML producible? XSD validation pass? Cada falha é classificada como: `ConfigurationGap`, `EngineGap`, `InputGap`, `SchemaIncompatibility`.

**Racional:** O suporte precisa saber se o provider está pronto ou o que falta. A classificação de gaps permite distinguir o que o suporte pode resolver (configuração) do que precisa de desenvolvimento (engine gap).

### 5. Bindings simplificados para Paulistana e Simpliss

**Decisão:** Configurar `base-rules.json` com bindings mínimos suficientes para produzir XML válido contra XSD. Simpliss reutiliza padrão ABRASF. Paulistana requer mapeamento específico do schema SP.

**Racional:** O foco é provar o workflow de onboarding, não cobrir 100% dos cenários de negócio desses providers.

### 6. Testes end-to-end via teste de integração

**Decisão:** Criar testes que simulam o fluxo completo: construir request → resolver provider → bind → serialize → validate XSD. Não é necessário levantar o servidor HTTP — os testes chamam o UseCase diretamente.

**Racional:** Testes de integração in-process são mais rápidos e confiáveis que testes HTTP. O fluxo é o mesmo.

## Risks / Trade-offs

- **[Risco] Paulistana pode ter schema incompatível com bindings simples** → Mitigation: Se não atingir PASS, documentar gap como `EngineGap` ou `ConfigurationGap` no relatório. O objetivo é avançar, não garantir 100%.

- **[Risco] Mudança no UseCase pode quebrar fluxo existente** → Mitigation: Manter o `NationalDpsManualSerializer` como fallback. O runtime serializer é uma opção adicional, não substituição.

- **[Trade-off] Discovery por filesystem vs registry** → Mais simples, mas não suporta providers em banco ou em serviço externo. Aceitável para a POC.

- **[Risco] Mapeamento município→provider precisa ser mantido atualizado** → Mitigation: Cada provider declara seus municípios no `base-rules.json`. O suporte pode adicionar/remover códigos de município sem alterar código. Em produção, esse mapeamento viria de uma fonte mais dinâmica (banco, API), mas para a POC o JSON é suficiente.

- **[Trade-off] Fallback para nacional** → Municípios sem provider específico usam o nacional. Isso é semanticamente correto (NFS-e Nacional/SPED é o padrão federal), mas pode gerar XML num formato que o município não aceita se ele tiver um provider específico não cadastrado.
