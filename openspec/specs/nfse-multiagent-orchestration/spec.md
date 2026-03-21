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

## Demonstrated Capabilities

### XSD Coverage Report via Multiagent
Three specialized agents (XsdAnalysisAgent, SerializerAgent, SpecAgent) execute in parallel to produce a coverage report (`docs/coverage/xsd-coverage-report.md`) mapping XSD elements to serializer Build* methods with status per block.

### MCP Integration in Analysis
The SpecAgent uses the `spec-assistant` MCP server (configured in `.mcp.json`) to read specs and acceptance criteria during analysis workflows.

### LSP-Assisted Code Navigation
SerializerAgent uses LSP diagnostics / grep semântico to trace Build* methods to the XSD elements they emit, producing a method-to-element mapping.

### Evolution Backlog
The analysis produces a prioritized backlog (`docs/coverage/evolution-backlog.md`) of gaps between XSD and serializer, as input for future changes.

## Acceptance Criteria
- The demo can show the same problem through assistant, single-agent and multi-agent modes.
- Each agent has a clear responsibility.
- The orchestration can be explained with concrete repository artifacts.
- A coverage report exists mapping XSD blocks to serializer methods with coverage status.
- The backlog lists gaps with priority for future evolution.

## Demo Narrative
1. Assistant explains the contract.
2. Single agent updates Swagger from YAML.
3. Multi-agent flow analyzes XSD, updates serializer and validates outputs.
4. SpecAgent records the change in `.openspec`.
5. Three agents run in parallel to produce XSD coverage report.
6. Coverage report and evolution backlog are reviewed.
