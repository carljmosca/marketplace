---
name: implement-dsl-api
description: >
  Implements a session-aware Spring Boot JSON REST API application for a
  Domain-Specific Language (DSL). Connects to the reusable DSL library JAR to
  manage stateful sessions, evaluate DSL commands over HTTP, and return rich JSON
  payloads with current state, outputs, and available transitions. Use when the
  user asks to "create DSL REST API", "build DSL session API", "expose DSL over HTTP",
  or mentions DSL web service endpoints.
---

<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Implement DSL Session REST API (Spring Boot)

## Goal

Implement a standalone **Spring Boot JSON REST API application (`<domain>-api`)** that imports and leverages the reusable
**`<domain>-dsl.jar`** library created by `/implement-dsl`.

This service is an **optional integration layer** for environments where web frontends, microservices, or external systems
interact with the DSL over HTTP/JSON rather than embedding the Java library directly or using the Model Context Protocol.

## If an Implementation Already Exists

Before generating new files, check whether an API module or controller already exists (e.g., in `<domain>-api/` or
`src/main/java/.../controller/`):
- If it exists, update the existing REST controller and session repository in place.
- Reconcile request/response DTOs and transition mappings with recent changes to the `<domain>-dsl` library.

## Workflow & Conventions

1. **Locate Reusable DSL Library**:
   - Ensure `<domain>-dsl` has been compiled and installed (`mvn clean install`).
   - Identify `DslEngine`, `DslSession`, and `DslSessionRepository` interfaces provided by `<domain>-dsl`.

2. **Project Setup & Dependencies (`<domain>-api/pom.xml`)**:
   - Create or configure the API module depending on the `<domain>-dsl` library and Spring Boot Web:
     ```xml
     <dependencies>
         <!-- Reusable DSL Core Library -->
         <dependency>
             <groupId>${project.groupId}</groupId>
             <artifactId><domain>-dsl</artifactId>
             <version>${project.version}</version>
         </dependency>
         <!-- Spring Boot Starter Web -->
         <dependency>
             <groupId>org.springframework.boot</groupId>
             <artifactId>spring-boot-starter-web</artifactId>
         </dependency>
         <dependency>
             <groupId>org.springframework.boot</groupId>
             <artifactId>spring-boot-starter-validation</artifactId>
         </dependency>
     </dependencies>
     ```

3. **REST Controller Endpoints (`DslSessionController.java`)**:
   - Expose the standard session lifecycle:
     - `POST /api/dsl/sessions`: Creates a new session, initializes state machine, and returns `sessionId`, `currentState`, and `availableTransitions`.
     - `POST /api/dsl/sessions/{sessionId}/execute`: Accepts `{ "command": "..." }`, executes against the active session's FSM via `DslEngine`, and returns updated state and available transitions.
     - `GET /api/dsl/sessions/{sessionId}`: Inspects current state and variables without state mutation.
     - `DELETE /api/dsl/sessions/{sessionId}`: Terminates and evicts the session.
   - Consult [../implement-dsl/references/session-api-spec.md](../implement-dsl/references/session-api-spec.md) for full request/response schemas.

4. **Standard JSON Response Format**:
   ```json
   {
     "sessionId": "123e4567-e89b-12d3-a456-426614174000",
     "status": "SUCCESS",
     "currentState": "AWAITING_PAYMENT",
     "message": "Posting accepted",
     "data": { "totalDebits": 150.00, "totalCredits": 150.00 },
     "availableTransitions": ["SUBMIT_PAYMENT", "CANCEL_ORDER"],
     "errors": []
   }
   ```

5. **Compilation & Packaging**:
   - Run `mvn clean package` to build the runnable Spring Boot application JAR (`<domain>-api.jar`).
   - Test endpoints with `curl` or automated integration tests.

