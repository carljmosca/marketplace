---
name: implement-dsl-lsp
description: >
  Implements a Language Server Protocol (LSP) server daemon in Java using Eclipse
  LSP4J for a Domain-Specific Language (DSL). Connects the ANTLR 4 grammar and
  Finite State Machine to provide real-time editor features: syntax and state-aware
  diagnostics, context-sensitive code completions, hover documentation, and document
  symbols over standard I/O (stdio). Use when the user asks to "implement LSP",
  "create language server", "build DSL LSP", or mentions LSP server implementation.
---

<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Implement DSL Language Server Protocol (Java / Eclipse LSP4J)

## Goal

Implement a Language Server Protocol (LSP) server daemon for the DSL created by `/implement-dsl` in Java using
**Eclipse LSP4J** (`org.eclipse.lsp4j`), packaged as a runnable fat JAR communicating over standard I/O (`System.in`/`System.out`).

## If an Implementation Already Exists

Before generating new files, check whether an LSP server module or class already exists (e.g., in `<domain>-lsp/` or
`src/main/java/.../lsp/`):
- If it exists, update the existing language server rather than generating parallel files.
- Keep existing custom handlers and transport bindings intact.
- Reconcile diagnostic listeners, completion providers, and hover providers with recent changes to the ANTLR grammar
  or FSM state definitions.

## Workflow & Conventions

1. **Locate DSL Core & Grammar Artifacts**:
   - Inspect the ANTLR 4 grammar (`.g4`), generated lexer/parser, and Java 21 FSM state definitions produced by `/implement-dsl`.
   - Identify the domain verbs, keywords, states, and transition rules.

2. **Project Setup & Dependencies**:
   - Create or configure the LSP module (e.g. `<domain>-lsp/`) with `pom.xml`:
     ```xml
     <dependency>
         <groupId>org.eclipse.lsp4j</groupId>
         <artifactId>org.eclipse.lsp4j</artifactId>
         <version>0.24.0</version>
     </dependency>
     ```
   - Add the `<domain>-core` dependency containing the ANTLR parser and FSM.
   - Configure `maven-shade-plugin` or `spring-boot-maven-plugin` to produce a runnable standalone fat JAR (`<domain>-lsp.jar`).

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
     - Tokenize and parse document using the ANTLR lexer/parser.
     - Report syntax errors with 0-based LSP `Position` and `Range` coordinates.
     - Perform semantic & FSM validation: walk statements in sequential order against the FSM. If a statement triggers
       an invalid transition from the current state, publish a diagnostic with severity `DiagnosticSeverity.Error`.

5. **State-Aware Completions (`textDocument/completion`)**:
   - Analyze text preceding cursor position to determine expected token or statement type.
   - Simulate document execution up to the cursor to determine the active FSM state.
   - Suggest *only* the commands and transitions valid in the current FSM state (via `OrderState.validTransitions()`),
     alongside standard keywords and snippets.

6. **Hover Documentation (`textDocument/hover`)**:
   - Provide markdown hover cards showing command signatures, parameter explanations, business rules (`BR-*`), and
     state transition diagrams.

7. **Compilation & Packaging**:
   - Run `mvn clean package` to produce the standalone runnable JAR.
   - Verify that the fat JAR launches without classpath errors:
     `java -jar target/<domain>-lsp.jar` (listening on stdio).

8. **Next Step Guidance**:
   - Conclude by summarizing the LSP server capabilities and guiding the user to the VS Code extension skill:
     ```text
     The Eclipse LSP4J language server has been implemented and packaged into target/<domain>-lsp.jar.
     To package and launch this LSP server within Visual Studio Code, run:
       /implement-dsl-vscode-extension
     ```

