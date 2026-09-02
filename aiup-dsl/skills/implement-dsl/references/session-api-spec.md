<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Session-Aware DSL REST API Specification

This reference defines the HTTP/JSON API contract for executing DSL commands across stateful sessions.

## Endpoints

### 1. Create Session
- **`POST /api/dsl/sessions`**
- **Request Body**: Optional configuration or metadata:
  ```json
  {
    "userId": "user-42"
  }
  ```
- **Response**: `201 Created`
  ```json
  {
    "sessionId": "4a7b9c21-1234-5678-abcd-ef0123456789",
    "status": "SUCCESS",
    "currentState": "INITIAL",
    "message": "Session initialized",
    "availableTransitions": ["CREATE_ORDER"],
    "createdAt": "2026-09-02T12:00:00Z"
  }
  ```

### 2. Execute Command in Session
- **`POST /api/dsl/sessions/{sessionId}/execute`**
- **Request Body**:
  ```json
  {
    "command": "add item \"Widget\" quantity 2 at price 19.99"
  }
  ```
- **Response**: `200 OK` (on success) or `422 Unprocessable Entity` (on validation/state error)
  ```json
  {
    "sessionId": "4a7b9c21-1234-5678-abcd-ef0123456789",
    "status": "SUCCESS",
    "currentState": "DRAFT",
    "message": "Item added",
    "data": {
      "orderId": "ord-101",
      "itemCount": 1,
      "total": 39.98
    },
    "availableTransitions": ["ADD_ITEM", "SUBMIT_ORDER", "CANCEL_ORDER"],
    "errors": []
  }
  ```
- **Error Response Example**:
  ```json
  {
    "sessionId": "4a7b9c21-1234-5678-abcd-ef0123456789",
    "status": "ERROR",
    "currentState": "APPROVED",
    "message": "Invalid transition",
    "availableTransitions": [],
    "errors": [
      "Cannot execute 'add item' in terminal state APPROVED"
    ]
  }
  ```

### 3. Get Session State
- **`GET /api/dsl/sessions/{sessionId}`**
- **Response**: `200 OK`
  ```json
  {
    "sessionId": "4a7b9c21-1234-5678-abcd-ef0123456789",
    "currentState": "DRAFT",
    "availableTransitions": ["ADD_ITEM", "SUBMIT_ORDER", "CANCEL_ORDER"],
    "data": { ... }
  }
  ```

### 4. Close Session
- **`DELETE /api/dsl/sessions/{sessionId}`**
- **Response**: `204 No Content`

## Session Storage Implementation

Provide a clean `DslSessionRepository` interface:

```java
public interface DslSessionRepository {
    DslSession createSession();
    Optional<DslSession> findById(UUID sessionId);
    void save(DslSession session);
    void delete(UUID sessionId);
}
```

With an in-memory implementation (`InMemoryDslSessionRepository`) using `ConcurrentHashMap<UUID, DslSession>` and a scheduled cleanup task removing sessions idle for longer than the configured timeout (default: 30 minutes).

