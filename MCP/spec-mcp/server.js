import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
import fs from "fs";
import path from "path";
import pdfParse from "pdf-parse";

const SPECS_DIR = path.join(import.meta.dirname, "specs");

// Load and cache all PDFs on startup
const specCache = {};

async function loadSpecs() {
  const files = fs.readdirSync(SPECS_DIR).filter(f => f.endsWith(".pdf"));
  for (const file of files) {
    const buffer = fs.readFileSync(path.join(SPECS_DIR, file));
    const data = await pdfParse(buffer);
    specCache[file] = data.text;
  }
  console.error(`Loaded ${files.length} spec(s): ${files.join(", ")}`);
}

const server = new McpServer({
  name: "spec-reader",
  version: "1.0.0",
});

// Tool 1: list available specs
server.tool("list_specs", "List all available spec PDF files", {}, async () => {
  const files = Object.keys(specCache);
  return {
    content: [{ type: "text", text: files.length
      ? `Available specs:\n${files.map(f => `- ${f}`).join("\n")}`
      : "No spec files found in specs/ directory." }]
  };
});

// Tool 2: search across all specs
server.tool(
  "search_spec",
  "Search for a keyword or phrase across all spec PDFs",
  { query: z.string().describe("Search term or phrase") },
  async ({ query }) => {
    const results = [];
    const q = query.toLowerCase();

    for (const [file, text] of Object.entries(specCache)) {
      const lines = text.split("\n");
      const matches = lines
        .map((line, i) => ({ line: line.trim(), i }))
        .filter(({ line }) => line.toLowerCase().includes(q))
        .slice(0, 10); // max 10 matches per file

      if (matches.length > 0) {
        results.push(`\n### ${file}\n` + matches
          .map(({ line, i }) => `  [line ${i}] ${line}`)
          .join("\n"));
      }
    }

    return {
      content: [{
        type: "text",
        text: results.length
          ? results.join("\n")
          : `No results found for "${query}".`
      }]
    };
  }
);

// Tool 3: get a chunk of text around a keyword
server.tool(
  "get_context",
  "Get surrounding context (±10 lines) around a keyword in a spec",
  {
    file: z.string().describe("Spec filename (e.g. api-spec.pdf)"),
    keyword: z.string().describe("Keyword to locate"),
  },
  async ({ file, keyword }) => {
    const text = specCache[file];
    if (!text) return { content: [{ type: "text", text: `File "${file}" not found.` }] };

    const lines = text.split("\n");
    const idx = lines.findIndex(l => l.toLowerCase().includes(keyword.toLowerCase()));
    if (idx === -1) return { content: [{ type: "text", text: `"${keyword}" not found in ${file}.` }] };

    const start = Math.max(0, idx - 10);
    const end = Math.min(lines.length, idx + 10);
    const excerpt = lines.slice(start, end).join("\n");

    return { content: [{ type: "text", text: `Context from ${file} around "${keyword}":\n\n${excerpt}` }] };
  }
);

await loadSpecs();
const transport = new StdioServerTransport();
await server.connect(transport);