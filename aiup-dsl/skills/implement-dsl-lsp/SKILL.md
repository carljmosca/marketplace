---
name: implement-dsl-lsp
description: >
  Implements a Language Server Protocol (LSP) server daemon in Java using Eclipse
  LSP4J that wraps and leverages the reusable DSL library JAR. Connects the ANTLR 4
  grammar and Finite State Machine to provide real-time editor features: syntax
  and state-aware diagnostics, context-sensitive code completions, hover
  documentation, and document symbols over standard I/O (stdio).
---

<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Implement DSL Language Server Protocol (Java / Eclipse LSP4J)

## Goal

Implement a **Language Server Protocol (LSP) server daemon (`<domain>-lsp`)** in Java using **Eclipse LSP4J** (`org.eclipse.lsp4j`)
that imports and leverages the reusable **`<domain>-dsl.jar`** library created by `/implement-dsl`.

The resulting LSP daemon is packaged as a runnable fat JAR (`<domain>-lsp.jar`) communicating over standard I/O
(`System.in`/`System.out`) to power rich IDE features in VS Code, Eclipse, IntelliJ, and other LSP clients.

## If an Implementation Already Exists

Before generating new files, check whether an LSP server module or class already exists (e.g., in `<domain>-lsp/` or
`src/main/java/.../lsp/`):
- If it exists, update the existing language server rather than generating parallel files.
- Keep existing custom handlers and transport bindings intact.
- Reconcile diagnostic listeners, completion providers, and hover providers with recent changes to the `<domain>-dsl` library.

## Workflow & Conventions

1. **Locate Reusable DSL Library**:
   - Ensure `<domain>-dsl` is built and installed (`mvn clean install`).
   - Identify AST nodes, parser rules, and FSM transition methods exposed by `<domain>-dsl`.

2. **Project Setup & Dependencies (`<domain>-lsp/pom.xml`)**:
   - Configure the LSP application module depending on the `<domain>-dsl` library:
     ```xml
     <dependencies>
         <!-- Reusable DSL Core Library -->
         <dependency>
             <groupId>${project.groupId}</groupId>
             <artifactId><domain>-dsl</artifactId>
             <version>${project.version}</version>
         </dependency>
         <!-- Eclipse LSP4J -->
         <dependency>
             <groupId>org.eclipse.lsp4j</groupId>
             <artifactId>org.eclipse.lsp4j</artifactId>
             <version>0.24.0</version>
         </dependency>
     </dependencies>
     ```
   - Configure `maven-shade-plugin` to produce a runnable standalone fat JAR (`<domain>-lsp.jar`).

3. **LSP Server Implementation**:
   - Implement `LanguageServer`, `TextDocumentService`, and `WorkspaceService`:
     - `DslLanguageServer`: manages server capabilities, initialize request, and shutdown/exit lifecycle.
     - `DslTextDocumentService`: handles `didOpen`, `didChange`, `completion`, `hover`, `documentSymbol`.
     - `DslWorkspaceService`: handles configuration changes and workspace symbols.
   - Implement the standard I/O launcher (`DslLauncher.java`):
     ```java
     public class DslLauncher {
         public static void main(String[] args) {
             DslLanguageServer server = new DslLanguageServer();
             Launcher<LanguageClient> launcher = LSPLauncher.createServerLauncher(
                 server, System.in, System.out
             );
             server.connect(launcher.getRemoteProxy());
             launcher.startListening();
         }
     }
     ```

4. **Real-Time Diagnostics (`publishDiagnostics`)**:
   - On `textDocument/didOpen` and `textDocument/didChange`:
     - Tokenize and parse document using the ANTLR lexer/parser from `<domain>-dsl`.
     - Report syntax errors with 0-based LSP `Position` and `Range` coordinates.
     - Perform semantic & FSM validation: walk statements in sequential order against the FSM. If a statement triggers
       an invalid transition from the current state or violates a business rule (`BR-*`), publish a diagnostic with severity `DiagnosticSeverity.Error`.

5. **State-Aware Completions (`textDocument/completion`)**:
   - Analyze text preceding cursor position to determine expected token or statement type.
   - Simulate document execution up to the cursor to determine the active FSM state.
   - Suggest *only* the commands and transitions valid in the current FSM state (via `OrderState.validTransitions()`),
     alongside standard keywords and snippets.

6. **Hover Documentation (`textDocument/hover`)**:
   - Provide markdown hover cards showing command signatures, parameter explanations, business rules (`BR-*`), and
     state transition diagrams.

7. **Compilation & Packaging**:
   - Run `mvn clean package` to produce the standalone runnable fat JAR.
   - Verify that the fat JAR launches without classpath errors:
     `java -jar target/<domain>-lsp.jar` (listening on stdio).

8. **Next Step Guidance**:
   - Conclude by summarizing the LSP server capabilities and guiding the user to the VS Code extension skill:
     ```text
     The Eclipse LSP4J language server daemon has been packaged into target/<domain>-lsp.jar.
     To package and launch this LSP server within Visual Studio Code, run:
       /implement-dsl-vscode-extension
     ```
