---
name: implement-dsl
description: >
  Implements a Domain-Specific Language (DSL) in Java 21 leveraging ANTLR 4 and
  Finite State Machines (FSM). Takes use case specifications (UC-*.md) and entity
  models (docs/entity_model.md) to generate an ANTLR 4 grammar (.g4), a lightweight
  Java 21 sealed state machine, an AST visitor execution engine, an interactive
  natural language REPL, and a session-aware Spring Boot JSON REST API. Use when
  the user asks to "implement a DSL", "create ANTLR grammar", "build state machine",
  "create DSL API", or mentions DSL implementation.
---

<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Implement DSL (Java 21 / ANTLR 4 / FSM)

## Goal

Implement a Domain-Specific Language (DSL) for the specified domain use cases (`UC-XXX.md`) and entity model
(`docs/entity_model.md`) in Java 21, creating:
1. An **ANTLR 4 grammar** (`.g4`) supporting natural, readable domain syntax and command synonyms.
2. A **Java 21 Finite State Machine (FSM)** enforcing lifecycle invariants, state transitions, and business rules (`BR-*`).
3. An **AST Visitor Execution Engine** evaluating statements against active state machine sessions.
4. An **Interactive Natural Language REPL** (using JLine) providing tab completion, command history, and state-aware guidance.
5. A **Session-Aware Spring Boot JSON REST API** providing stateful session management with rich JSON request/response payloads (`availableTransitions`) for headless UI/UX frontends.

## If an Implementation Already Exists

Before writing new code, check whether the DSL grammar or state machine already exists under `src/main/antlr4/` or
`src/main/java/`:
- If the grammar or state machine already exists, **reconcile it with the specification instead of creating duplicate files**.
- Compare changes in use cases (new steps, renamed verbs, changed business rules, new states).
- Update the ANTLR `.g4` file to include new verbs/rules.
- Update the Java 21 sealed state and event hierarchy to reflect new states or transitions.
- Update the visitor evaluator and API handlers in place.

## Workflow & Conventions

1. **Read Specifications & Domain Models**:
   - Read `docs/requirements.md` to extract functional requirements (`FR-*`), business rules, and constraints (`C-*`).
   - Read `docs/entity_model.md` to understand entities, attributes, and relationships.
   - Read `docs/use_cases.puml` and `docs/use_cases/UC-XXX-*.md` to understand user actions, main success flows, alternative flows, preconditions, and postconditions.

2. **Domain Grammar Design (`src/main/antlr4/<Domain>.g4`)**:
   - Structure grammar with clear lexer and parser rules.
   - Support expressive, human-readable domain phrasing (e.g. `create order for customer "Alice"`).
   - Provide synonyms for common verbs where appropriate (e.g. `create | new | make`).
   - Include custom `ANTLRErrorListener` to capture syntax errors with exact line and column numbers.
   - Consult [references/grammar-guidelines.md](references/grammar-guidelines.md) for patterns.

3. **Java 21 Finite State Machine (`fsm/`)**:
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

4. **AST Visitor & Execution Engine (`engine/`)**:
   - Extend the ANTLR-generated `<Domain>BaseVisitor<ExecutionResult>`.
   - Dispatch parsed AST nodes to the active session's FSM instance.
   - Collect domain outputs, error messages, and mutated session state.

5. **Interactive Natural Language REPL (`repl/`)**:
   - Build an interactive terminal shell using JLine 3.
   - Provide command history, line editing, and ANSI-colored prompt displaying the current session state.
   - Implement tab completion that queries the active session's FSM for `availableTransitions`.
   - Provide helpful hints or prompt for missing parameters when a command is incomplete.

6. **Session-Aware Spring Boot JSON REST API (`api/`)**:
   - Controller endpoints managing stateful sessions:
     - `POST /api/dsl/sessions`: creates a new session, initializes FSM to start state, returns `sessionId`, `currentState`, and initial `availableTransitions`.
     - `POST /api/dsl/sessions/{sessionId}/execute`: receives JSON payload `{ "command": "..." }`, evaluates against the session's FSM, updates state, and returns response JSON.
     - `GET /api/dsl/sessions/{sessionId}`: inspects current state, variables, and `availableTransitions` without state mutation.
     - `DELETE /api/dsl/sessions/{sessionId}`: removes the session.
   - Standard JSON response format:
     ```json
     {
       "sessionId": "123e4567-e89b-12d3-a456-426614174000",
       "status": "SUCCESS",
       "currentState": "AWAITING_PAYMENT",
       "message": "Item added successfully",
       "data": { "item": "Widget", "quantity": 2, "total": 39.98 },
       "availableTransitions": ["SUBMIT_PAYMENT", "CANCEL_ORDER"],
       "errors": []
     }
     ```
   - Implement `DslSessionRepository` with default `ConcurrentHashMap<UUID, DslSession>` and sliding expiration cleanup.
   - Consult [references/session-api-spec.md](references/session-api-spec.md) for endpoint contracts.

7. **Compilation & Verification**:
   - Run Maven (`mvn clean compile test`) or Gradle (`./gradlew test`) to verify grammar generation and clean compilation.

8. **Next Step Guidance**:
   - Conclude by summarizing the generated DSL components and guiding the user to the next phase:
     ```text
     The DSL grammar, FSM, and session API have been implemented.
     To generate the Language Server Protocol (LSP) daemon for IDE integration, run:
       /implement-dsl-lsp
     ```

