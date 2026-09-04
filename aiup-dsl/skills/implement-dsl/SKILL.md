---
name: implement-dsl
description: >
  Implements a reusable Domain-Specific Language (DSL) library (JAR) in Java 21
  leveraging ANTLR 4 and Finite State Machines (FSM). Takes use case specifications
  (UC-*.md) and entity models (docs/entity_model.md) to generate an ANTLR 4 grammar
  (.g4), a Java 21 sealed state machine, an AST visitor execution engine, a
  programmatic Java API (DslEngine, DslSession), and an interactive natural
  language REPL. Packaged as a standalone reusable library JAR for use by MCP
  servers, LSP daemons, and host applications.
---

<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Implement Reusable DSL Library (Java 21 / ANTLR 4 / FSM)

## Goal

Implement a **reusable Java library (`<domain>-dsl.jar`)** for the specified domain use cases (`UC-XXX.md`) and entity model
(`docs/entity_model.md`) in Java 21, creating:
1. An **ANTLR 4 grammar** (`.g4`) supporting natural, readable domain syntax and command synonyms.
2. A **Java 21 Finite State Machine (FSM)** enforcing lifecycle invariants, state transitions, and business rules (`BR-*`).
3. An **AST Visitor Execution Engine** evaluating statements against active state machine sessions.
4. A **Programmatic Public Java API** (`DslEngine`, `DslSession`, `DslParser`, `EvaluationResult`, `ValidationResult`) for seamless embedding into host applications, MCP servers, and language servers.
5. An **In-Memory Session Repository** (`DslSessionRepository`, `InMemoryDslSessionRepository`) supporting stateful session lifecycles.
6. An **Interactive Natural Language REPL** (using JLine) providing tab completion, command history, and state-aware guidance for CLI usage.

The resulting artifact is a **reusable library JAR** that can be published to a Maven repository and consumed by downstream applications, including the Model Context Protocol (MCP) server, the Language Server Protocol (LSP) daemon, and host backend systems.

## If an Implementation Already Exists

Before writing new code, check whether the DSL grammar or state machine already exists under `src/main/antlr4/` or
`src/main/java/`:
- If the grammar or state machine already exists, **reconcile it with the specification instead of creating duplicate files**.
- Compare changes in use cases (new steps, renamed verbs, changed business rules, new states).
- Update the ANTLR `.g4` file to include new verbs/rules.
- Update the Java 21 sealed state and event hierarchy to reflect new states or transitions.
- Update the visitor evaluator and programmatic API handlers in place.

## Workflow & Conventions

1. **Read Specifications & Domain Models**:
   - Read `docs/requirements.md` to extract functional requirements (`FR-*`), business rules, and constraints (`C-*`).
   - Read `docs/entity_model.md` to understand entities, attributes, and relationships.
   - Read `docs/use_cases.puml` and `docs/use_cases/UC-XXX-*.md` to understand user actions, main success flows, alternative flows, preconditions, and postconditions.

2. **Project Setup (`<domain>-dsl/pom.xml`)**:
   - Configure the module as a reusable Java library:
     ```xml
     <groupId>com.example</groupId>
     <artifactId><domain>-dsl</artifactId>
     <version>0.1.0-SNAPSHOT</version>
     <packaging>jar</packaging>

     <dependencies>
         <!-- ANTLR 4 Runtime -->
         <dependency>
             <groupId>org.antlr</groupId>
             <artifactId>antlr4-runtime</artifactId>
             <version>4.13.2</version>
         </dependency>
         <!-- JLine 3 (optional CLI/REPL support) -->
         <dependency>
             <groupId>org.jline</groupId>
             <artifactId>jline</artifactId>
             <version>3.26.3</version>
             <optional>true</optional>
         </dependency>
     </dependencies>
     ```

3. **Domain Grammar Design (`src/main/antlr4/<Domain>.g4`)**:
   - Structure grammar with clear lexer and parser rules.
   - Support expressive, human-readable domain phrasing (e.g. `debit 150.00 USD to "1010"`).
   - Provide synonyms for common verbs where appropriate (e.g. `create | new | make`).
   - Include custom `ANTLRErrorListener` to capture syntax errors with exact line and column numbers.
   - Consult [references/grammar-guidelines.md](references/grammar-guidelines.md) for patterns.

4. **Java 21 Finite State Machine (`fsm/`)**:
   - Model states using Java 21 sealed interfaces and records:
     ```java
     public sealed interface OrderState permits DraftState, SubmittedState, ApprovedState, CancelledState {}
     public record DraftState(String orderId, String customerId, List<Item> items) implements OrderState {}
     ```
   - Model events/triggers using sealed event records corresponding to DSL commands.
   - Implement an explicit transition table or evaluator returning a `TransitionResult`:
     ```java
     public record TransitionResult(OrderState newState, boolean success, String message, List<String> availableTransitions) {}
     ```
   - Enforce business rules (`BR-*`) as transition guards: if a rule fails, return a failed transition result without advancing the state.
   - Consult [references/fsm-architecture.md](references/fsm-architecture.md) for detailed patterns.

5. **AST Visitor & Execution Engine (`engine/`)**:
   - Extend the ANTLR-generated `<Domain>BaseVisitor<ExecutionResult>`.
   - Dispatch parsed AST nodes to the active session's FSM instance.
   - Collect domain outputs, error messages, and mutated session state.

6. **Programmatic Public Java API (`api/`)**:
   - Expose clean facade classes for downstream library consumers:
     ```java
     public class DslEngine {
         public ValidationResult validate(String script);
         public EvaluationResult execute(String script);
         public EvaluationResult executeInSession(UUID sessionId, String command);
         public List<String> getAvailableTransitions(UUID sessionId);
     }
     ```
   - Implement `DslSessionRepository` and `InMemoryDslSessionRepository` with `ConcurrentHashMap<UUID, DslSession>` and sliding expiration cleanup.
   - Consult [references/session-api-spec.md](references/session-api-spec.md) for API contracts and session management.

7. **Interactive Natural Language REPL (`repl/`)**:
   - Build an interactive terminal shell using JLine 3.
   - Provide command history, line editing, and ANSI-colored prompt displaying current session state.
   - Implement tab completion that queries the active session's FSM for `availableTransitions`.

8. **Compilation & Packaging**:
   - Run `mvn clean install` to compile grammar, run unit tests, and install `<domain>-dsl.jar` into the local Maven repository.

9. **Next Step Guidance**:
   - Guide the user to implement the Model Context Protocol server:
     ```text
     The reusable DSL library has been built and installed as <domain>-dsl.jar.
     To implement the Model Context Protocol (MCP) server application that wraps this library for AI agents, run:
       /implement-dsl-mcp
     To implement the Eclipse LSP4J language server daemon, run:
       /implement-dsl-lsp
     ```
