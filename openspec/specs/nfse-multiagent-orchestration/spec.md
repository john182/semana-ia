# Spec: nfse-multiagent-orchestration

## Objective
Model how assistant mode, single-agent execution and multi-agent orchestration are applied to the NFSe problem using Claude Code / OpenCode style workflows.

## Problem
The project needs to demonstrate AI themes in a way that is grounded in a real engineering workflow rather than abstract examples.

## Outcome
A documented orchestration where specialized agents collaborate on OpenAPI sync, XSD analysis, serializer generation, validation and spec governance.

## In Scope
- Assistant mode
- Single-agent mode
- Multi-agent mode
- Skills
- MCP integration points
- Commands
- Scheduling ideas

## Agents
### OpenApiAgent
Reads the YAML contract and updates or reviews Swagger-facing code.

### XsdAnalysisAgent
Reads the XSD set and extracts structure, mandatory groups and schema constraints.

### SerializerAgent
Implements or refines `ServiceInvoice -> XML` logic using current business rules.

### TestAgent
Produces or updates validation assets: snapshots, schema validation, consistency checks.

### SpecAgent
Maintains `.openspec` artifacts, decisions and evolution trail.

## MCP Candidates
- Filesystem / docs / local specs
- GitHub
- Browser / fetch
- Sentry / observability
- Custom spec MCP

## Commands
- explore current contracts
- propose a change
- apply a change
- archive a change
- generate docs sync report
- generate serializer report

## Scheduling Ideas
- Nightly drift check between YAML and DTOs
- Nightly XSD artifact regeneration check
- Scheduled validation of XML samples against current schemas

## Acceptance Criteria
- The demo can show the same problem through assistant, single-agent and multi-agent modes.
- Each agent has a clear responsibility.
- The orchestration can be explained with concrete repository artifacts.

## Demo Narrative
1. Assistant explains the contract.
2. Single agent updates Swagger from YAML.
3. Multi-agent flow analyzes XSD, updates serializer and validates outputs.
4. SpecAgent records the change in `.openspec`.
