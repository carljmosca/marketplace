---
name: implement-dsl-mcp
description: >
  Implements a Model Context Protocol (MCP) server in Java for a Domain-Specific
  Language (DSL). Connects the ANTLR 4 grammar, AST execution engine, and Java 21
  Finite State Machine to expose MCP tools (validate_dsl, execute_dsl,
  get_available_transitions), MCP resources (dsl://grammar, dsl://fsm/states,
  dsl://examples), and MCP prompts to AI agents (Claude Code, Cursor, Copilot,
  Gemini) and AIUP workflows. Use when the user asks to "implement MCP server",
  "create DSL MCP", "build MCP tools for DSL", or mentions Model Context Protocol.
---

<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Implement DSL Model Context Protocol (MCP) Server

## Goal

Implement a Model Context Protocol (MCP) server for the Domain-Specific Language created by `/implement-dsl` in Java,
packaged as a runnable fat JAR supporting standard I/O (`stdio`) and/or HTTP/SSE transport.

The resulting MCP server enables AI agents (Claude Code, Cursor, Gemini, Copilot, Antigravity) and downstream AIUP
construction skills to discover grammar rules, validate scripts, execute commands, inspect lifecycle state machines, and
generate compliant domain scripts.

## If an Implementation Already Exists

Before generating new files, check whether an MCP server module or class already exists (e.g., in `<domain>-mcp/` or
`src/main/java/.../mcp/`):
- If it exists, update the existing MCP server rather than generating parallel files.
- Keep existing tool signatures, resource URIs, and custom prompt templates intact.
- Reconcile validation logic, grammar resources, and state transition tools with recent changes to the ANTLR grammar
  or FSM state definitions.

## Workflow & Conventions

1. **Locate DSL Core & Grammar Artifacts**:
   - Inspect the ANTLR 4 grammar (`.g4`), AST visitor, and Java 21 FSM state definitions produced by `/implement-dsl`.
   - Identify domain verbs, keywords, state transitions, business rules (`BR-*`), and use case specifications (`UC-*.md`).

2. **Project Setup & Dependencies (`<domain>-mcp/`)**:
   - Create or configure the MCP module with `pom.xml`:
     ```xml
     <dependencies>
         <dependency>
             <groupId>${project.groupId}</groupId>
             <artifactId><domain>-core</artifactId>
             <version>${project.version}</version>
         </dependency>
         <dependency>
             <groupId>io.modelcontextprotocol.sdk</groupId>
             <artifactId>mcp-core</artifactId>
             <version>0.7.0</version>
         </dependency>
     </dependencies>
     ```
   - Configure `maven-shade-plugin` or `spring-boot-maven-plugin` to package a runnable standalone fat JAR (`<domain>-mcp.jar`).

3. **Implement MCP Tools**:
   - `validate_dsl(script: string)`:
     - Parses the provided DSL script using the ANTLR lexer/parser.
     - Performs semantic validation by walking statements against the Java 21 FSM transition table.
     - Returns `{ "valid": boolean, "errors": [ { "line": int, "column": int, "message": string } ] }`.
   - `execute_dsl(sessionId?: string, command: string)`:
     - Evaluates a DSL command against an existing session or an ephemeral sandbox session.
     - Advances the FSM and returns `{ "status": "SUCCESS"|"FAILURE", "currentState": string, "availableTransitions": string[], "message": string }`.
   - `get_available_transitions(sessionId?: string, currentState?: string)`:
     - Returns the list of valid next actions/commands given the current lifecycle state.
   - `explain_verb(verb: string)`:
     - Returns usage documentation, syntax examples, and business rules associated with a domain command.

4. **Implement MCP Resources**:
   - Expose grammar and state machine metadata for agent context enrichment:
     - `dsl://grammar` (`text/plain`): the raw `.g4` ANTLR grammar and syntax summary.
     - `dsl://fsm/states` (`application/json`): all states, allowed transitions, and associated business rules.
     - `dsl://fsm/diagram` (`text/vnd.mermaid`): Mermaid diagram representing valid lifecycle transitions.
     - `dsl://examples/{useCase}` (`text/plain`): canonical DSL code examples matching specification use cases (`UC-XXX`).

5. **Implement MCP Prompts**:
   - Register structured prompt workflows:
     - `generate-dsl-script`: guides agents to draft a DSL script from a user story or use case description.
     - `troubleshoot-dsl-error`: prompts the agent with script context and syntax/FSM error diagnostics to guide automatic repair.

6. **Transport & Entry Point (`DslMcpServerApplication.java`)**:
   - Configure standard I/O (`System.in`/`System.out`) as the primary transport for local AI CLI agents:
     ```java
     public class DslMcpServerApplication {
         public static void main(String[] args) {
             McpSyncServer syncServer = McpServer.sync(new StdioServerTransport())
                 .serverInfo("<domain>-dsl-mcp", "1.0.0")
                 .capabilities(ServerCapabilities.builder()
                     .tools(true)
                     .resources(true, false)
                     .prompts(true)
                     .build())
                 .build();

             registerTools(syncServer);
             registerResources(syncServer);
             registerPrompts(syncServer);

             syncServer.listen();
         }
     }
     ```
   - Optionally support HTTP / SSE transport via Spring Boot or embedded HTTP server for remote team environments.
   - Consult [references/mcp-architecture.md](references/mcp-architecture.md) for detailed implementation patterns.

7. **Compilation & Verification**:
   - Build the module:
     ```sh
     mvn clean package
     ```
   - Verify tool execution using the MCP Inspector or direct execution:
     ```sh
     npx @modelcontextprotocol/inspector java -jar target/<domain>-mcp.jar
     ```

8. **Next Step Guidance**:
   - Provide the configuration snippet for adding the server to the user's project `.mcp.json` or `claude.json`:
     ```json
     {
       "mcpServers": {
         "<domain>-dsl": {
           "type": "stdio",
           "command": "java",
           "args": ["-jar", "<domain>-mcp/target/<domain>-mcp.jar"]
         }
       }
     }
     ```
   - Guide the user to related skills:
     ```text
     The DSL MCP server has been built and packaged into target/<domain>-mcp.jar.
     To implement the Eclipse LSP4J language server daemon, run:
       /implement-dsl-lsp
     To package the VS Code extension, run:
       /implement-dsl-vscode-extension
     ```

