<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# aiup-dsl

`aiup-dsl` is the AI Unified Process construction plugin for Java-based Domain-Specific Languages (DSLs) leveraging ANTLR 4
and Finite State Machines (FSMs). It turns the domain entity model and use case specifications produced by
[`aiup-core`](../aiup-core/) into an ANTLR 4 grammar, a lightweight Java 21 finite state machine, an interactive natural
language REPL, a session-aware Spring Boot JSON REST API, a Model Context Protocol (MCP) server, an Eclipse LSP4J language
server, and a Visual Studio Code extension.

This plugin is designed to continue from the specifications produced by `aiup-core`. For the complete AI Unified Process workflow,
use it alongside `aiup-core`.

## Skills and workflow

| Phase        | Skill                                                                                 | Result                                                                               |
|--------------|---------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------|
| Construction | [`/implement-dsl`](skills/implement-dsl/SKILL.md)                                     | ANTLR grammar, Java 21 FSM, evaluation engine, interactive REPL, session JSON REST API |
| Construction | [`/implement-dsl-mcp`](skills/implement-dsl-mcp/SKILL.md)                             | Model Context Protocol (MCP) server for AI agents to validate, query, and execute DSL |
| Construction | [`/implement-dsl-lsp`](skills/implement-dsl-lsp/SKILL.md)                             | Eclipse LSP4J server daemon with ANTLR diagnostics and FSM-driven completions        |
| Construction | [`/implement-dsl-vscode-extension`](skills/implement-dsl-vscode-extension/SKILL.md) | VS Code extension client packaging and launching the Java LSP server                |

```text
Construction
────────────────────────────────────────────────────────────────────────────────────────
/implement-dsl  →  /implement-dsl-mcp  →  /implement-dsl-lsp  →  /implement-dsl-vscode-extension
```

The linked `SKILL.md` files are the authoritative reference for detailed inputs, outputs, and behavior.

## Installation

### Tessl

Initialize the project once:

```sh
tessl init --agent agents
```

`agents` is the vendor-neutral layout; use `claude-code`, `cursor`, `gemini`, `codex`, `copilot`, or `copilot-vscode`
for a specific host. Then install the plugins:

```sh
tessl install ai-unified-process/aiup-core
tessl install ai-unified-process/aiup-dsl
```

Depending on the configured agent, skills may be exposed as slash commands or invoked by intent, for example
"implement DSL from use cases".

### Claude Code

```text
/plugin marketplace add ai-unified-process/marketplace
/plugin install aiup-core
/plugin install aiup-dsl
```

See the marketplace [installation guides](../docs/getting-started.md) for other agents and manual adoption.

## Prerequisites

- `aiup-core` and reviewed `docs/entity_model.md` plus `docs/use_cases/UC-*.md` artifacts.
- Java 21+ and Maven (or Gradle).
- Node.js and npm (for compiling the VS Code extension).

## Inputs and generated artifacts

The plugin consumes the core documentation under `docs/`. It creates the DSL grammar, state machine, REST API, LSP server, and editor tooling:

```text
your-solution/
├── docs/
│   ├── entity_model.md
│   ├── use_cases/UC-*.md
│   └── test_cases/TC-*.md
├── <domain>-core/                    # /implement-dsl: grammar, FSM, and REPL
│   ├── src/main/antlr4/             # ANTLR 4 Lexer and Parser grammar (.g4)
│   └── src/main/java/.../
│       ├── fsm/                     # Java 21 sealed state machine & transition table
│       ├── engine/                  # ANTLR visitor & command execution engine
│       └── repl/                    # JLine interactive natural language shell
├── <domain>-api/                     # /implement-dsl: session-aware Spring Boot JSON API
│   └── src/main/java/.../
│       ├── controller/              # Session-based REST endpoints
│       └── session/                 # DslSessionRepository & session lifecycle
├── <domain>-mcp/                     # /implement-dsl-mcp: Model Context Protocol server
│   └── src/main/java/.../           # MCP tools, resources, prompts (stdio / SSE)
├── <domain>-lsp/                     # /implement-dsl-lsp: Eclipse LSP4J server
│   └── src/main/java/.../           # LSP4J handlers, diagnostics, completions, hover
└── <domain>-vscode/                  # /implement-dsl-vscode-extension: VS Code extension
    ├── package.json                 # Language contribution & extension manifest
    ├── syntaxes/                    # TextMate syntax highlighting grammar
    └── src/extension.ts             # LanguageClient launcher for Java LSP server
```

