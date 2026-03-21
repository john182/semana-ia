import { existsSync, readFileSync, readdirSync, statSync } from "node:fs";
import path from "node:path";
import { McpServer, ResourceTemplate } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const server = new McpServer({
  name: "spec-assistant",
  version: "1.2.0"
});

function projectRoot(): string {
  return process.cwd();
}

function changeRoot(changeName: string): string {
  return path.join(projectRoot(), "openspec", "changes", changeName);
}

function ensureExists(targetPath: string, label: string): void {
  if (!existsSync(targetPath)) {
    throw new Error(`${label} not found: ${targetPath}`);
  }
}

function readUtf8File(targetPath: string): string {
  ensureExists(targetPath, "File");
  return readFileSync(targetPath, "utf-8");
}

function safeReadIfExists(targetPath: string): string | null {
  if (!existsSync(targetPath)) {
    return null;
  }

  return readFileSync(targetPath, "utf-8");
}

function listFilesRecursively(baseDir: string): string[] {
  if (!existsSync(baseDir)) {
    return [];
  }

  const entries = readdirSync(baseDir);
  const files: string[] = [];

  for (const entry of entries) {
    const fullPath = path.join(baseDir, entry);
    const relativePath = path.relative(projectRoot(), fullPath);
    const entryStat = statSync(fullPath);

    if (entryStat.isDirectory()) {
      files.push(...listFilesRecursively(fullPath));
      continue;
    }

    files.push(relativePath);
  }

  return files.sort();
}

function getChangeArtifacts(changeName: string) {
  const root = changeRoot(changeName);

  ensureExists(root, "Change directory");

  return {
    root,
    proposalPath: path.join(root, "proposal.md"),
    tasksPath: path.join(root, "tasks.md"),
    specsDir: path.join(root, "specs")
  };
}

function formatTextResult(title: string, payload: unknown) {
  return {
    content: [
      {
        type: "text" as const,
        text: `# ${title}\n\n${JSON.stringify(payload, null, 2)}`
      }
    ]
  };
}

function toSpecAssistantUri(kind: string, changeName: string, extraPath?: string): string {
  const encodedChange = encodeURIComponent(changeName);

  if (!extraPath) {
    return `spec-assistant://${kind}/${encodedChange}`;
  }

  const normalized = extraPath.replace(/\\/g, "/");
  return `spec-assistant://${kind}/${encodedChange}/${normalized}`;
}

function firstParam(value: string | string[]): string {
  return Array.isArray(value) ? value[0] : value;
}

function joinParam(value: string | string[]): string {
  return Array.isArray(value) ? value.join("/") : value;
}

/**
 * Resources
 */

server.registerResource(
  "change-proposal",
  new ResourceTemplate("spec-assistant://proposal/{changeName}", { list: undefined }),
  {
    title: "OpenSpec Change Proposal",
    description: "Conteúdo do proposal.md de uma change do OpenSpec"
  },
  async (uri, { changeName }) => {
    const resolvedChangeName = firstParam(changeName);
    const { proposalPath } = getChangeArtifacts(resolvedChangeName);

    return {
      contents: [
        {
          uri: uri.href,
          text: readUtf8File(proposalPath),
          mimeType: "text/markdown"
        }
      ]
    };
  }
);

server.registerResource(
    "change-tasks",
    new ResourceTemplate("spec-assistant://tasks/{changeName}", { list: undefined }),
    {
      title: "OpenSpec Change Tasks",
      description: "Conteúdo do tasks.md de uma change do OpenSpec"
    },
    async (uri, { changeName }) => {
      const resolvedChangeName = firstParam(changeName);
      const { tasksPath } = getChangeArtifacts(resolvedChangeName);

      return {
        contents: [
          {
            uri: uri.href,
            text: readUtf8File(tasksPath),
            mimeType: "text/markdown"
          }
        ]
      };
    }
);

server.registerResource(
  "change-spec-file",
  new ResourceTemplate("spec-assistant://spec-file/{changeName}/{filePath*}", { list: undefined }),
  {
    title: "OpenSpec Change Spec File",
    description: "Lê um arquivo específico dentro de openspec/changes/<change>/specs"
  },
  async (uri, { changeName, filePath }) => {
    const resolvedChangeName = firstParam(changeName);
    const resolvedFilePath = joinParam(filePath);
    const { specsDir } = getChangeArtifacts(resolvedChangeName);
    const targetPath = path.join(specsDir, resolvedFilePath);

    return {
      contents: [
        {
          uri: uri.href,
          text: readUtf8File(targetPath),
          mimeType: "text/markdown"
        }
      ]
    };
  }
);

/**
 * Tools
 */

server.registerTool(
    "get_change_summary",
    {
      title: "Get Change Summary",
      description: "Lê proposal, tasks e arquivos de spec de uma change do OpenSpec.",
      inputSchema: {
        changeName: z.string().min(1)
      }
    },
    async ({ changeName }) => {
      const { proposalPath, tasksPath, specsDir } = getChangeArtifacts(changeName);

      const proposal = safeReadIfExists(proposalPath);
      const tasks = safeReadIfExists(tasksPath);
      const specFiles = listFilesRecursively(specsDir);

      return formatTextResult("Change Summary", {
        changeName,
        proposalResource: toSpecAssistantUri("proposal", changeName),
        tasksResource: toSpecAssistantUri("tasks", changeName),
        proposalPath: path.relative(projectRoot(), proposalPath),
        tasksPath: path.relative(projectRoot(), tasksPath),
        specFiles,
        specResources: specFiles.map((relativePath) => {
          const relativeToSpecs = relativePath
              .replace(/^openspec[\\/]+changes[\\/]+/, "")
              .split(/[\\/]/)
              .slice(3)
              .join("/");

          return toSpecAssistantUri("spec-file", changeName, relativeToSpecs);
        }),
        proposal,
        tasks
      });
    }
);

server.registerTool(
    "get_change_proposal",
    {
      title: "Get Change Proposal",
      description: "Lê o proposal.md de uma change.",
      inputSchema: {
        changeName: z.string().min(1)
      }
    },
    async ({ changeName }) => {
      const { proposalPath } = getChangeArtifacts(changeName);

      return formatTextResult("Change Proposal", {
        changeName,
        resourceUri: toSpecAssistantUri("proposal", changeName),
        proposalPath: path.relative(projectRoot(), proposalPath),
        proposal: readUtf8File(proposalPath)
      });
    }
);

server.registerTool(
    "get_change_tasks",
    {
      title: "Get Change Tasks",
      description: "Lê o tasks.md de uma change.",
      inputSchema: {
        changeName: z.string().min(1)
      }
    },
    async ({ changeName }) => {
      const { tasksPath } = getChangeArtifacts(changeName);

      return formatTextResult("Change Tasks", {
        changeName,
        resourceUri: toSpecAssistantUri("tasks", changeName),
        tasksPath: path.relative(projectRoot(), tasksPath),
        tasks: readUtf8File(tasksPath)
      });
    }
);

server.registerTool(
    "list_change_spec_files",
    {
      title: "List Change Spec Files",
      description: "Lista os arquivos de spec delta dentro de openspec/changes/<change>/specs.",
      inputSchema: {
        changeName: z.string().min(1)
      }
    },
    async ({ changeName }) => {
      const { specsDir } = getChangeArtifacts(changeName);
      const files = listFilesRecursively(specsDir);

      return formatTextResult("Change Spec Files", {
        changeName,
        specsDir: path.relative(projectRoot(), specsDir),
        files,
        resources: files.map((relativePath) => {
          const relativeToSpecs = relativePath
              .replace(/^openspec[\\/]+changes[\\/]+/, "")
              .split(/[\\/]/)
              .slice(3)
              .join("/");

          return toSpecAssistantUri("spec-file", changeName, relativeToSpecs);
        })
      });
    }
);

server.registerTool(
    "suggest_change_commit_message",
    {
      title: "Suggest Change Commit Message",
      description: "Sugere commit semântico com base na change.",
      inputSchema: {
        changeName: z.string().min(1),
        commitType: z.enum(["feat", "fix", "refactor", "test", "docs", "chore"]).default("feat"),
        summary: z.string().min(1).default("apply approved change")
      }
    },
    async ({ changeName, commitType, summary }) => {
      return formatTextResult("Suggested Commit Message", {
        changeName,
        conventionalCommit: `${commitType}: ${summary} (${changeName})`
      });
    }
);

server.registerTool(
    "review_change_against_spec",
    {
      title: "Review Change Against Spec",
      description: "Gera checklist de revisão técnica alinhado à change.",
      inputSchema: {
        changeName: z.string().min(1),
        changedFiles: z.array(z.string()).default([])
      }
    },
    async ({ changeName, changedFiles }) => {
      return formatTextResult("Review Checklist", {
        changeName,
        changedFiles,
        checklist: [
          "A implementação respeitou o proposal e o tasks.md?",
          "Os critérios de aceite foram cobertos?",
          "O escopo permaneceu dentro da change?",
          "Houve reutilização antes de criar novos métodos auxiliares?",
          "Foi evitada duplicação de lógica para CEP, telefone, documento, datas e normalizações?",
          "Os testes unitários cobrem o comportamento alterado?",
          "Se houve XML, os testes cobrem estrutura, nós condicionais e schema quando aplicável?"
        ]
      });
    }
);

async function main(): Promise<void> {
  const transport = new StdioServerTransport();
  await server.connect(transport);
}

main().catch((error) => {
  console.error("[spec-assistant] Failed to start MCP server:", error);
  process.exit(1);
});