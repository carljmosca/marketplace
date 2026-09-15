---
name: implement-dsl-mcp
description: >
  Implements a Model Context Protocol (MCP) server application in Java that
  wraps and leverages the reusable DSL library JAR. Connects the DslEngine,
  grammar, and Finite State Machine to expose MCP tools (validate_dsl,
  execute_dsl, get_available_transitions), MCP resources (dsl://grammar,
  dsl://fsm/states, dsl://examples), and MCP prompts. Generates consumer
  documentation (CONSUMING-THE-DSL.md) and UI/UX integration guidance so
  multiple downstream applications can consume the DSL and its LSP server.
---

<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Implement DSL Model Context Protocol (MCP) Server

## Goal

Implement a **Model Context Protocol (MCP) server application (`<domain>-mcp`)** in Java that imports and leverages the
reusable **`<domain>-dsl.jar`** library created by `/implement-dsl`.

The resulting MCP server application is packaged as a runnable fat JAR (supporting `stdio` and HTTP/SSE). It acts for the
DSL in the exact same manner that the Vaadin MCP server acts for Vaadin — enabling AI agents (Claude Code, Cursor, Gemini,
Copilot, Antigravity) and downstream AIUP construction skills to:
1. Discover grammar rules, validate scripts, execute commands, and inspect lifecycle state machines.
2. Serve as the **Single Source of Truth** for the DSL across **multiple downstream applications** (e.g. admin portals, customer frontends, batch services) without forking or duplicating DSL code.
3. Guide the creation of **UI/UX interfaces** driven by the DSL's FSM state transitions and the embedded **LSP server** (e.g. live in-app Monaco script editors, state-aware action buttons, dynamic forms).

## If an Implementation Already Exists

Before generating new files, check whether an MCP server module or class already exists (e.g., in `<domain>-mcp/` or
`src/main/java/.../mcp/`):
- If it exists, update the existing MCP server rather than generating parallel files.
- Keep existing tool signatures, resource URIs, and custom prompt templates intact.
- Reconcile validation logic, grammar resources, and state transition tools with recent changes to the `<domain>-dsl` library.

## Workflow & Conventions

1. **Locate Reusable DSL Library**:
   - Ensure the `<domain>-dsl` library has been compiled and installed (`mvn clean install`).
   - Identify the public Java APIs exposed by `<domain>-dsl`: `DslEngine`, `DslSessionRepository`, `DslParser`, `FsmDefinition`.

2. **Project Setup & Dependencies (`<domain>-mcp/pom.xml`)**:
   - Create or configure the MCP application module depending on the `<domain>-dsl` library:
     ```xml
     <dependencies>
         <!-- Reusable DSL Core Library -->
         <dependency>
             <groupId>${project.groupId}</groupId>
             <artifactId><domain>-dsl</artifactId>
             <version>${project.version}</version>
         </dependency>
         <!-- MCP Server SDK -->
         <dependency>
             <groupId>io.modelcontextprotocol.sdk</groupId>
             <artifactId>mcp-core</artifactId>
             <version>0.7.0</version>
         </dependency>
     </dependencies>
     ```
   - Configure `maven-shade-plugin` or `spring-boot-maven-plugin` to package a runnable standalone fat JAR (`<domain>-mcp.jar`).

3. **Implement MCP Tools (calling `DslEngine`)**:
   - `validate_dsl(script: string)`:
     - Delegates to `dslEngine.validate(script)`.
     - Returns `{ "valid": boolean, "errors": [ { "line": int, "column": int, "message": string } ] }`.
   - `execute_dsl(sessionId?: string, command: string)`:
     - Delegates to `dslEngine.executeInSession(sessionId, command)` or `dslEngine.execute(command)`.
     - Advances the FSM and returns `{ "status": "SUCCESS"|"FAILURE", "currentState": string, "availableTransitions": string[], "message": string, "data": { ... } }`.
   - `get_available_transitions(sessionId?: string, currentState?: string)`:
     - Queries valid next actions and allowed transitions from the active state machine.
   - `explain_verb(verb: string)`:
     - Returns usage documentation, syntax examples, parameters, and business rules (`BR-*`) for a domain command.

4. **Implement MCP Resources**:
   - Expose grammar, state machine, and UI guidance metadata for agent context enrichment:
     - `dsl://grammar` (`text/plain`): the raw `.g4` ANTLR grammar and syntax summary.
     - `dsl://fsm/states` (`application/json`): all states, allowed transitions, and associated business rules.
     - `dsl://fsm/diagram` (`text/vnd.mermaid`): Mermaid diagram representing valid lifecycle transitions.
     - `dsl://examples/{useCase}` (`text/plain`): canonical DSL code examples matching specification use cases (`UC-XXX`).
     - `dsl://ui-patterns` (`text/markdown`): UI/UX integration guidelines (state-driven buttons, embedded Monaco LSP editor configuration, form fields).

5. **Implement MCP Prompts**:
   - Register structured prompt workflows:
     - `generate-dsl-script`: guides agents to draft a DSL script from a user story or use case description.
     - `troubleshoot-dsl-error`: prompts the agent with script context and syntax/FSM error diagnostics to guide automatic repair.
     - `generate-dsl-ui`: guides agents to generate UI forms and action buttons in host applications (Vaadin, Angular, React) matching active FSM states.

6. **Transport & Entry Point (`DslMcpServerApplication.java`)**:
   - Configure standard I/O (`System.in`/`System.out`) as the primary transport for local AI CLI agents:
     ```java
     public class DslMcpServerApplication {
         public static void main(String[] args) {
             DslEngine dslEngine = new DslEngine(new InMemoryDslSessionRepository());
             
             McpSyncServer syncServer = McpServer.sync(new StdioServerTransport())
                 .serverInfo("<domain>-dsl-mcp", "1.0.0")
                 .capabilities(ServerCapabilities.builder()
                     .tools(true)
                     .resources(true, false)
                     .prompts(true)
                     .build())
                 .build();

             registerTools(syncServer, dslEngine);
             registerResources(syncServer);
             registerPrompts(syncServer);

             syncServer.listen();
         }
     }
     ```
   - Optionally support HTTP / SSE transport via Spring Boot or embedded HTTP server for remote team environments.
   - Consult [references/mcp-architecture.md](references/mcp-architecture.md) for detailed implementation patterns.

7. **Generate Consumer Guide (`CONSUMING-THE-DSL.md`)**:
   - Automatically generate a `CONSUMING-THE-DSL.md` file in the root or module directory documenting how multiple downstream applications consume this single DSL:
     - **Multi-Application Reuse**: Clear declaration that this DSL is the single canonical source of truth for all applications sharing this domain.
     - **Maven Dependency Setup**: How to add `<domain>-dsl` to application `pom.xml`.
     - **Agent Setup**: How to configure the MCP server in `.mcp.json`.
     - **UI/UX Patterns**:
       - *State-Driven Action Buttons*: How to call `get_available_transitions` to dynamically enable/disable buttons in Vaadin/Angular/React views.
       - *Embedded In-App Editor*: How to connect browser editors (Monaco / Ace) to the `<domain>-lsp` server over WebSocket to provide in-app DSL script editing with autocomplete and error squiggles.
       - *Dynamic Forms*: How to map DSL command parameters to form fields with validation.

8. **Compilation & Verification**:
   - Build the MCP server fat JAR:
     ```sh
     mvn clean package
     ```
   - Verify tool execution using the MCP Inspector:
     ```sh
     npx @modelcontextprotocol/inspector java -jar target/<domain>-mcp.jar
     ```

9. **Next Step Guidance**:
   - Provide the configuration snippet for adding the server to `.mcp.json` or `claude.json`:
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
   - Guide the user to the language server daemon and editor tooling:
     ```text
     The DSL MCP server application has been built and packaged into target/<domain>-mcp.jar.
     Consumer guide generated at CONSUMING-THE-DSL.md.
     To implement the Eclipse LSP4J language server daemon leveraging the DSL library, run:
       /implement-dsl-lsp
     To package the VS Code extension, run:
       /implement-dsl-vscode-extension
     ```
