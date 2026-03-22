---
name: technical-review
description: Revisa tecnicamente a implementação seguindo estritamente os padrões oficiais do projeto
---

# Objetivo

Revisar a implementação e apontar, de forma objetiva e acionável, problemas de qualidade, padronização, aderência arquitetural, duplicação, riscos de regressão e violações dos padrões do projeto.

# Regras obrigatórias

- Revisar com foco em aderência real ao padrão do projeto.
- Priorizar problemas concretos em vez de sugestões cosméticas.
- Ser objetivo, técnico e acionável.
- Não sugerir mudanças sem ganho claro.
- Verificar se a solução está mais complexa do que o problema exige.
- Verificar se nomes estão coerentes com o domínio.
- Verificar se houve violação de Clean Code, SOLID com pragmatismo, coesão e baixo acoplamento.
- Verificar se métodos privados ficaram no final da classe.
- Verificar se strings repetidas relevantes deveriam ser constantes.
- Verificar se existem números mágicos.
- Verificar se campos com conjunto finito de valores deveriam ser `enum`.
- Verificar se a arquitetura do projeto foi respeitada.
- Verificar se existem testes suficientes para a mudança.
- Quando houver XML, verificar se os testes cobrem estrutura, obrigatoriedade, comportamento condicional e schema quando aplicável.

# Regras obrigatórias de aderência arquitetural

- Identificar a arquitetura já adotada pelo projeto antes de avaliar a mudança.
- Verificar se a implementação seguiu a arquitetura já existente no projeto.
- Se o projeto usar Onion Architecture, validar se a mudança permaneceu coerente com Onion Architecture.
- Se o projeto usar Arquitetura Hexagonal, validar se a mudança permaneceu coerente com Arquitetura Hexagonal.
- Se o projeto usar MVC, validar se a mudança permaneceu coerente com MVC.
- Se o projeto usar Clean Architecture, validar se a mudança permaneceu coerente com Clean Architecture.
- Se o projeto usar modularização por feature, validar se a mudança respeitou esse padrão.
- Se o projeto usar organização por camada técnica, validar se a mudança respeitou esse padrão.
- Sinalizar como problema qualquer tentativa de introduzir um estilo arquitetural diferente do já adotado pelo projeto sem solicitação explícita.
- Sinalizar como problema qualquer mistura indevida de arquiteturas incompatíveis.
- Verificar se novas classes, interfaces, handlers, services, repositories, controllers, use cases, adapters, gateways, mappers, validators e DTOs foram posicionados na camada correta conforme a arquitetura vigente.
- Verificar se a direção de dependências foi preservada.
- Sinalizar como problema quando código de domínio passa a depender de infraestrutura sem justificativa compatível com a arquitetura do projeto.
- Sinalizar como problema quando responsabilidades forem movidas para camadas inadequadas.
- Sinalizar como problema quando a mudança criar novas camadas, novos módulos ou novas abstrações arquiteturais sem necessidade clara e sem alinhamento com a estrutura do projeto.
- Validar se o código novo parece nativo do projeto e segue o mesmo padrão estrutural dos componentes equivalentes já existentes.
- Em caso de dúvida sobre a arquitetura dominante, inferir pela estrutura de pastas, namespaces, dependências, convenções e tipos existentes.
- Priorizar consistência com a arquitetura atual do projeto acima de preferência teórica por outro padrão.

# Regras obrigatórias de naming e legibilidade

- Apontar como problema parâmetros de uma letra fora de lambdas triviais e óbvias.
- Apontar como problema variáveis locais com nomes genéricos ou sem contexto semântico claro.
- Apontar como problema métodos privados com nomes vagos, genéricos ou sem intenção explícita.
- Apontar como problema nomes como `data`, `value`, `item`, `result`, `obj`, `res`, `x` e equivalentes quando houver nome semântico melhor.
- Apontar como problema expressões complexas inline quando prejudicarem a leitura.
- Validar se o código favorece legibilidade por meio de nomes claros e variáveis intermediárias bem nomeadas quando necessário.

# Regras obrigatórias de reutilização e centralização

- Verificar se foram criados métodos auxiliares duplicados para formatação, normalização, parsing, conversão, limpeza ou composição de valores.
- Apontar quando existir lógica repetida para CEP, telefone, documento, máscaras, datas, textos, identificadores, códigos ou comportamentos equivalentes.
- Sinalizar quando o código criou novo método local em vez de reutilizar ou consolidar implementação já existente.
- Sinalizar quando houver múltiplas variações do mesmo comportamento espalhadas em classes diferentes.
- Verificar se o agente criou métodos locais por conveniência em vez de buscar o ponto central já existente.
- Recomendar centralização quando houver comportamento compartilhável repetido em mais de um ponto.
- Apontar quando builders, services, validators, mappers, converters ou handlers passaram a carregar lógica duplicada que deveria estar centralizada.
- Verificar se a solução introduziu mais uma versão paralela de algo que já existia no projeto.
- Quando houver comportamento recorrente reutilizável, recomendar reutilização ou refatoração para consolidação em vez de duplicação.

# Regras adicionais para revisão de engine, schema e provider

Quando a mudança envolver engine de schema, serializer runtime, providers ou XSD:

- verificar se foram executadas validações para todos os providers da pasta `providers/`
- verificar se foi gerado um resumo sumarizado por provider
- apontar como problema quando a mudança validar apenas um provider sem justificativa explícita
- apontar como problema quando faltarem status, gaps ou diagnóstico individual por provider
- validar se a mudança registrou claramente os limites técnicos ainda existentes por provider

# Saída esperada

A revisão deve retornar:

1. O que está bom
2. Problemas encontrados
3. Correções recomendadas
4. Lacunas de teste ou risco
5. Veredito final

# Critérios de reprovação

A revisão deve reprovar ou sinalizar fortemente quando encontrar:

- duplicação de regra de negócio
- criação de métodos auxiliares redundantes
- lógica compartilhada espalhada em vários pontos
- complexidade desnecessária
- nomes genéricos ou pouco orientados ao domínio
- ausência de reutilização quando já havia implementação semelhante
- regressão de padrão arquitetural
- introdução indevida de outro estilo arquitetural
- posicionamento incorreto de componentes em camadas inadequadas
- quebra da direção de dependências da arquitetura existente
- lacunas importantes de teste