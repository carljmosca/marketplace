<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Model Context Protocol (MCP) Architecture for Java DSLs

This reference outlines the module layout, server capabilities, tools, resources, and prompt endpoints required to implement a Model Context Protocol (MCP) server for a Java 21 DSL.

The resulting MCP server enables AI agents (such as Claude Code, Cursor, Copilot, Gemini, and Antigravity) and downstream AIUP construction skills to author, validate, and execute DSL code programmatically.

## Module Structure

```text
<domain>-mcp/
├── pom.xml
└── src/main/java/.../mcp/
    ├── DslMcpServerApplication.java  # Main entry point & transport configuration (stdio / SSE)
    ├── config/
    │   └── McpServerConfig.java      # Server capability registration (Tools, Resources, Prompts)
    ├── tools/
    │   ├── DslValidationTools.java   # Syntax and FSM semantic validation tools
    │   ├── DslExecutionTools.java    # Stateful and stateless execution tools
    │   └── DslTransitionTools.java  # Next available transition inspection tools
    ├── resources/
    │   ├── DslGrammarResource.java   # Exposes .g4 grammar and lexer/parser rules
    │   ├── DslFsmResource.java       # Exposes state graph, events, and business rules
    │   └── DslExamplesResource.java  # Exposes canonical valid DSL scripts per use case
    └── prompts/
        └── DslPromptTemplates.java   # Structured prompts for DSL script authoring & debugging
```

## Maven Dependencies

The MCP server can be implemented using the official Java Model Context Protocol SDK (`io.modelcontextprotocol.sdk:mcp`) or the Spring AI MCP Server Starter (`org.springframework.ai:spring-ai-mcp-server-spring-boot-starter`).

```xml
<dependencies>
    <!-- Core DSL Engine (Grammar, AST Visitor, FSM) -->
    <dependency>
        <groupId>${project.groupId}</groupId>
        <artifactId><domain>-core</artifactId>
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
| `validate_dsl` | `script` (string) | Parses DSL text with ANTLR and validates statement sequence against FSM rules. Returns syntax/semantic errors with line/column coordinates. |
| `execute_dsl` | `sessionId` (optional UUID), `command` (string) | Executes a DSL command against a stateful session or temporary sandbox. Returns execution status, updated state, and output data. |
| `get_available_transitions` | `sessionId` (optional UUID), `currentState` (optional string) | Returns valid next commands and allowed transitions based on the active FSM state and business rule guards. |
| `explain_verb` | `verb` (string) | Returns documentation, syntax variants, parameters, and business rules for a specific domain keyword/action. |

#### Example Tool Implementation (`validate_dsl`)

```java
@McpTool(name = "validate_dsl", description = "Validates DSL syntax and FSM lifecycle transitions")
public ValidationResult validateDsl(@McpParam(name = "script") String script) {
    List<ValidationError> errors = new ArrayList<>();

    // 1. ANTLR syntax validation
    CharStream charStream = CharStreams.fromString(script);
    OrderDslLexer lexer = new OrderDslLexer(charStream);
    CommonTokenStream tokens = new CommonTokenStream(lexer);
    OrderDslParser parser = new OrderDslParser(tokens);

    parser.removeErrorListeners();
    parser.addErrorListener(new BaseErrorListener() {
        @Override
        public void syntaxError(Recognizer<?, ?> recognizer, Object offendingSymbol,
                                int line, int charPositionInLine, String msg,
                                RecognitionException e) {
            errors.add(new ValidationError(line, charPositionInLine, "SYNTAX", msg));
        }
    });

    OrderDslParser.ScriptContext tree = parser.script();

    // 2. FSM semantic validation
    if (errors.isEmpty()) {
        FsmValidator validator = new FsmValidator();
        errors.addAll(validator.validateTransitions(tree));
    }

    boolean valid = errors.isEmpty();
    return new ValidationResult(valid, valid ? "Script is valid" : "Validation failed", errors);
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

Standard I/O transport is packaged as a runnable fat JAR. In `~/.claude.json` or project `.mcp.json`:

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

For team environments or web-based AIUP pipelines:

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

