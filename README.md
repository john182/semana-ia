# SemanaIA — NFSe Schema-Driven Engine

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
![MongoDB](https://img.shields.io/badge/MongoDB-7-47A248)
![Tests](https://img.shields.io/badge/tests-727%20passing-brightgreen)
![MVP](https://img.shields.io/badge/status-MVP-blue)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=SemanaIA&metric=alert_status)](https://sonarcloud.io/dashboard?id=SemanaIA)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SemanaIA&metric=coverage)](https://sonarcloud.io/dashboard?id=SemanaIA)

## O que é

O SemanaIA é um motor de geração de XML de NFS-e (Nota Fiscal de Serviços Eletrônica) orientado por schema XSD. Em vez de manter serializadores manuais para cada município, a engine analisa os XSDs dos providers em runtime, aplica regras tipadas e produz XML válido automaticamente. Isso elimina a necessidade de código específico por prefeitura e permite onboarding de novos providers sem intervenção de desenvolvedor.

O projeto foi construído inteiramente com assistência de IA (Claude Code com Opus 4.6) ao longo de 34 commits, passando por 6 fases de evolução — do serializer manual ao MVP com API, persistência MongoDB e 727 testes automatizados. A arquitetura segue o padrão Onion com 5 camadas bem definidas.

## Quick Start

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (para MongoDB)

### Subir a infraestrutura

```bash
docker compose up -d
```

### Executar a API

```bash
cd src/SemanaIA.ServiceInvoice.Api
dotnet run
```

A API estará disponível em **http://localhost:5211** e o Swagger em **http://localhost:5211/swagger**.

### Executar os testes

```bash
dotnet test
```

> 611 testes unitários + 116 testes de integração = **727 testes**, todos passando.

## Documentação

A documentação completa do projeto está na wiki:

**[Wiki — SemanaIA NFSe Engine](docs/wiki/Home.md)**

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Runtime | .NET 10 / C# |
| API | ASP.NET Core Minimal API |
| Banco de dados | MongoDB 7 |
| Testes | xUnit, Shouldly, Bogus, Moq |
| Containers | Docker Compose |
| Engine XML | XSD analysis em runtime, `XElement`-based serialization |
| IA | Claude Code (Opus 4.6) via OpenSpec workflow |

## Estrutura do Projeto

```
src/
├── SemanaIA.ServiceInvoice.Api/            # Endpoints e configuração
├── SemanaIA.ServiceInvoice.Application/    # Casos de uso
├── SemanaIA.ServiceInvoice.Domain/         # Modelos de domínio
├── SemanaIA.ServiceInvoice.Infrastructure/ # Integrações e persistência
└── SemanaIA.ServiceInvoice.XmlGeneration/  # Engine de geração XML
providers/                                   # XSDs, regras e configs por provider
tests/                                       # Testes unitários e de integração
docs/wiki/                                   # Documentação completa
```

## Licença

Este projeto é de uso interno. Licença a ser definida.
