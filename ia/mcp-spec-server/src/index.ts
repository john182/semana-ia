import { readFileSync } from "node:fs";
import path from "node:path";

type ToolRequest = {
  tool: string;
  args?: Record<string, unknown>;
};

function readSpecFile(fileName: string): string {
  const specPath = path.join(process.cwd(), "openspec", fileName);
  return readFileSync(specPath, "utf-8");
}

function getSpec(featureId: string) {
  const spec = readSpecFile("nfse-serializer.spec.md");
  return { featureId, spec };
}

function listAcceptanceCriteria(featureId: string) {
  const criteria = readSpecFile("acceptance-criteria.md");
  return { featureId, criteria };
}

function suggestCommitMessage(featureId: string, changesSummary: string) {
  return {
    featureId,
    conventionalCommit: `feat(${featureId.toLowerCase()}): ${changesSummary}`
  };
}

function generateBddTests(featureId: string, targetLanguage: string) {
  return {
    featureId,
    targetLanguage,
    testingConventions: {
      framework: "xUnit",
      assertions: "Shouldly",
      forbidAssertions: ["Assert.Equal", "Assert.NotNull", "Assert.Null", "Assert.StartsWith", "Assert.Contains"],
      namingPattern: "Given_<context>_Should_<expected_behavior>",
      structure: ["Arrange", "Act", "Assert"]
    },
    requiredScenarios: [
      "documento mínimo válido",
      "documento completo com todos os blocos opcionais",
      "tomador com CPF",
      "tomador com CNPJ",
      "endereço nacional",
      "endereço estrangeiro",
      "totais aproximados por valor",
      "totais aproximados por percentual",
      "sem totais aproximados"
    ],
    examples: [
      {
        testName: "Given_MinimalValidDocument_Should_GenerateValidDpsStructure"
      },
      {
        testName: "Given_CompleteDocument_Should_ContainAllOptionalBlocks"
      }
    ]
  };
}

function reviewCodeAgainstSpec(featureId: string, changedFiles: string[]) {
  return {
    featureId,
    changedFiles,
    checklist: [
      "Há modelo canônico intermediário?",
      "Regras de negócio estão separadas da montagem XML?",
      "O endpoint da API está alinhado à spec?",
      "Há testes BDD + 3A cobrindo o fluxo mínimo?"
    ]
  };
}

function handle(request: ToolRequest) {
  switch (request.tool) {
    case "get_spec":
      return getSpec(String(request.args?.featureId ?? "FEATURE-NFSE-SERIALIZER-001"));
    case "list_acceptance_criteria":
      return listAcceptanceCriteria(String(request.args?.featureId ?? "FEATURE-NFSE-SERIALIZER-001"));
    case "suggest_commit_message":
      return suggestCommitMessage(
        String(request.args?.featureId ?? "FEATURE-NFSE-SERIALIZER-001"),
        String(request.args?.changesSummary ?? "evolve nfse poc")
      );
    case "generate_bdd_tests":
      return generateBddTests(
        String(request.args?.featureId ?? "FEATURE-NFSE-SERIALIZER-001"),
        String(request.args?.targetLanguage ?? "csharp")
      );
    case "review_code_against_spec":
      return reviewCodeAgainstSpec(
        String(request.args?.featureId ?? "FEATURE-NFSE-SERIALIZER-001"),
        Array.isArray(request.args?.changedFiles) ? request.args!.changedFiles as string[] : []
      );
    default:
      return { error: `Unknown tool: ${request.tool}` };
  }
}

process.stdin.setEncoding("utf-8");
let buffer = "";
process.stdin.on("data", chunk => {
  buffer += chunk;
  const lines = buffer.split("\n");
  buffer = lines.pop() ?? "";
  for (const line of lines) {
    if (!line.trim()) continue;
    try {
      const request = JSON.parse(line) as ToolRequest;
      const result = handle(request);
      process.stdout.write(JSON.stringify(result) + "\n");
    } catch (error) {
      process.stdout.write(JSON.stringify({ error: String(error) }) + "\n");
    }
  }
});
