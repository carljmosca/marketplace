<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Programmatic Session & Library API Specification

This reference defines the Java library API contract and optional HTTP/JSON REST contracts for executing DSL commands across stateful sessions.

## 1. Reusable Java Library API (`DslEngine`)

The core library (`<domain>-dsl.jar`) provides a clean, thread-safe facade for executing and validating DSL scripts in host applications:

```java
public class DslEngine {
    private final DslSessionRepository sessionRepository;
    private final FsmDefinition fsmDefinition;

    public DslEngine(DslSessionRepository sessionRepository) {
        this.sessionRepository = sessionRepository;
        this.fsmDefinition = new FsmDefinition();
    }

    /**
     * Validates a complete script for ANTLR syntax errors and FSM semantic transition violations.
     */
    public ValidationResult validate(String script) { ... }

    /**
     * Executes a complete script in a temporary isolated session.
     */
    public EvaluationResult execute(String script) { ... }

    /**
     * Creates a new stateful session.
     */
    public DslSession createSession() { ... }

    /**
     * Executes a single command in an active session and advances the state machine.
     */
    public EvaluationResult executeInSession(UUID sessionId, String command) { ... }

    /**
     * Returns valid next actions/commands from the current state in the session.
     */
    public List<String> getAvailableTransitions(UUID sessionId) { ... }
}
```

### Result Types

```java
public record ValidationResult(
    boolean valid,
    String message,
    List<SyntaxError> errors
) {}

public record SyntaxError(
    int line,
    int column,
    String errorType,
    String message
) {}

public record EvaluationResult(
    UUID sessionId,
    String status,              // "SUCCESS" | "FAILURE"
    String currentState,
    String message,
    Map<String, Object> data,
    List<String> availableTransitions,
    List<String> errors
) {}
```

## 2. Session Storage & Lifecycle (`DslSessionRepository`)

```java
public interface DslSessionRepository {
    DslSession createSession();
    Optional<DslSession> findById(UUID sessionId);
    void save(DslSession session);
    void delete(UUID sessionId);
}
```

Default in-memory implementation (`InMemoryDslSessionRepository`) uses a `ConcurrentHashMap<UUID, DslSession>` with optional sliding expiration eviction for inactive sessions.

## 3. Optional HTTP / JSON REST API Endpoints

When hosting the DSL as a web service or Spring Boot microservice, map endpoints directly to `DslEngine`:

### Create Session
- **`POST /api/dsl/sessions`**
- **Response**: `201 Created`
  ```json
  {
    "sessionId": "4a7b9c21-1234-5678-abcd-ef0123456789",
    "status": "SUCCESS",
    "currentState": "INITIAL",
    "message": "Session initialized",
    "availableTransitions": ["CREATE_ACCOUNT", "OPEN_TRANSACTION"],
    "createdAt": "2026-09-04T12:00:00Z"
  }
  ```

### Execute Command in Session
- **`POST /api/dsl/sessions/{sessionId}/execute`**
- **Request Body**:
  ```json
  {
    "command": "debit 150.00 USD to \"1010\""
  }
  ```
- **Response**: `200 OK` (or `422 Unprocessable Entity`)
  ```json
  {
    "sessionId": "4a7b9c21-1234-5678-abcd-ef0123456789",
    "status": "SUCCESS",
    "currentState": "DRAFT",
    "message": "Debit posting recorded",
    "data": { "totalDebits": 150.00, "totalCredits": 0.00 },
    "availableTransitions": ["DEBIT", "CREDIT", "POST", "CANCEL"],
    "errors": []
  }
  ```

### Get Session State
- **`GET /api/dsl/sessions/{sessionId}`**

### Close Session
- **`DELETE /api/dsl/sessions/{sessionId}`**
