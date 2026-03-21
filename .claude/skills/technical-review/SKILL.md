---
name: technical-review
description: Revisa tecnicamente a mudança aplicada, validando aderência à change, qualidade, testes e riscos
---

# Objetivo
Executar revisão técnica final da mudança antes de considerá-la concluída.

# Responsabilidades
- Validar aderência à proposal, tasks e change.
- Revisar consistência arquitetural e qualidade da implementação.
- Revisar clareza de nomes e orientação ao domínio.
- Avaliar abstrações desnecessárias, acoplamento e duplicação.
- Verificar uso de constantes, enums e números mágicos quando aplicável.
- Validar se os testes apropriados foram criados ou ajustados.
- Validar testes XML/schema quando aplicável.
- Explicitar riscos, pendências e validações manuais restantes.

# Checklist obrigatório
- [ ] mudança aderente ao escopo
- [ ] implementação consistente
- [ ] testes adequados
- [ ] testes XML/schema quando aplicável
- [ ] riscos e pendências explícitos

# Saída esperada
- resumo do que está consistente
- problemas encontrados
- lacunas restantes
- riscos técnicos