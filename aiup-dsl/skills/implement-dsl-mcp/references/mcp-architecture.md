<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Model Context Protocol (MCP) Architecture for Java DSLs

This reference outlines how to build an MCP application that wraps and exposes a reusable Java DSL library (`<domain>-dsl.jar`) to AI agents and downstream AIUP workflows.

Like the Vaadin MCP server provides tools and context to build Vaadin web applications, the DSL MCP server equips LLMs with grammar syntax, semantic validation, and runtime execution tools to build applications that consume the DSL.

## Multi-Module Layout

```text
<domain>-parent/
├── <domain>-dsl/                     # Reusable core library (JAR)
│   ├── pom.xml
│   └── src/main/java/...             # DslEngine, Grammar, FSM, AST visitor
└── <domain>-mcp/                     # Standalone MCP server application
    ├── pom.xml                       # Depends on <domain>-dsl
    └── src/main/java/.../mcp/
        ├── DslMcpServerApplication.java  # Main entry point & transport configuration (stdio / SSE)
        ├── config/
        │   └── McpServerConfig.java      # Server capability registration (Tools, Resources, Prompts)
        ├── tools/
        │   ├── DslValidationTools.java   # Syntax and FSM semantic validation tools (delegates to DslEngine)
        │   ├── DslExecutionTools.java    # Stateful and stateless execution tools (delegates to DslEngine)
        │   └── DslTransitionTools.java  # Next available transition inspection tools
        ├── resources/
        │   ├── DslGrammarResource.java   # Exposes .g4 grammar and lexer/parser rules
        │   ├── DslFsmResource.java       # Exposes state graph, events, and business rules
        │   └── DslExamplesResource.java  # Exposes canonical valid DSL scripts per use case
        └── prompts/
            └── DslPromptTemplates.java   # Structured prompts for DSL script authoring & debugging
```

## Maven Dependencies (`<domain>-mcp/pom.xml`)

```xml
<dependencies>
    <!-- Reusable DSL Library Dependency -->
    <dependency>
        <groupId>${project.groupId}</groupId>
        <artifactId><domain>-dsl</artifactId>
        <version>${project.version}</version>
    </dependency>

    <!-- MCP Server SDK (stdio & SSE transport support) -->
    <dependency>
        <groupId>io.modelcontextprotocol.sdk</groupId>
        <artifactId>mcp-core</artifactId>
        <version>0.7.0</version>
    </dependency>

    <!-- Spring Boot Starter (optional, for HTTP/SSE transport) -->
    <dependency>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter</artifactId>
    </dependency>
</dependencies>
```

## Exposed MCP Capabilities

### 1. Tools

Tools are callable functions that allow an AI agent to validate, evaluate, and inspect DSL statements:

| Tool Name | Parameters | Description |
|-----------|------------|-------------|
| `validate_dsl` | `script` (string) | Delegates to `DslEngine.validate(script)`. Returns syntax and FSM semantic errors with line/column coordinates. |
| `execute_dsl` | `sessionId` (optional UUID), `command` (string) | Delegates to `DslEngine.executeInSession(sessionId, command)` or `DslEngine.execute(command)`. Returns execution status, updated state, and output data. |
| `get_available_transitions` | `sessionId` (optional UUID), `currentState` (optional string) | Returns valid next commands and allowed transitions based on active FSM state and business rule guards. |
| `explain_verb` | `verb` (string) | Returns documentation, syntax variants, parameters, and business rules for a specific domain keyword/action. |

#### Example Tool Implementation (`validate_dsl`)

```java
@McpTool(name = "validate_dsl", description = "Validates DSL syntax and FSM lifecycle transitions")
public ValidationResult validateDsl(@McpParam(name = "script") String script) {
    // Directly delegate to the reusable DSL library's engine
    return dslEngine.validate(script);
}
```

### 2. Resources

Resources expose contextual documentation, grammar specifications, and state charts directly to the agent's context:

| URI | MIME Type | Description |
|-----|-----------|-------------|
| `dsl://grammar` | `text/plain` | The complete ANTLR 4 grammar specification (`.g4`) with rule annotations. |
| `dsl://fsm/states` | `application/json` | JSON description of all FSM states, allowed transitions, and business rule IDs (`BR-*`). |
| `dsl://fsm/diagram` | `text/vnd.mermaid` | Mermaid state diagram representing the full lifecycle. |
| `dsl://examples/{useCase}` | `text/plain` | Canonical valid DSL scripts for use cases (e.g. `UC-001`). |
| `dsl://schema` | `application/json` | Entity model dictionary and attribute definitions. |

### 3. Prompts

Prompts provide pre-engineered prompt workflows for agents working with the DSL:

| Prompt Name | Arguments | Description |
|-------------|-----------|-------------|
| `generate-dsl-script` | `useCaseDescription` (string), `targetState` (optional) | Guides the LLM to generate a compliant DSL script fulfilling a specified business scenario. |
| `troubleshoot-dsl-error` | `script` (string), `errorMessage` (string) | Analyzes a failed execution and provides corrected DSL statements. |

## Transports & Client Integration

### Stdio Transport (Local Developer & CLI Agents)

Packaged as a runnable fat JAR. In `~/.claude.json` or project `.mcp.json`:

```json
{
  "mcpServers": {
    "<domain>-dsl": {
      "type": "stdio",
      "command": "java",
      "args": ["-jar", "/path/to/<domain>-mcp/target/<domain>-mcp.jar"]
    }
  }
}
```

### Streamable HTTP / SSE Transport (Remote / Shared Service)

```json
{
  "mcpServers": {
    "<domain>-dsl": {
      "type": "http",
      "url": "http://localhost:8080/mcp"
    }
  }
}
```
