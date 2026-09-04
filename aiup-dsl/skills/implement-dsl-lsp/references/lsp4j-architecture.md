<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Eclipse LSP4J Architecture for Java DSLs

This reference outlines the class structure and handlers required to implement a Language Server Protocol (LSP) daemon in Java using Eclipse LSP4J that consumes a reusable DSL library (`<domain>-dsl.jar`).

## Multi-Module Structure

```text
<domain>-parent/
├── <domain>-dsl/                   # Reusable core library (JAR)
│   ├── pom.xml
│   └── src/main/java/...           # DslEngine, ANTLR parser, FSM
└── <domain>-lsp/                   # Standalone LSP daemon application
    ├── pom.xml                     # Depends on <domain>-dsl + org.eclipse.lsp4j
    └── src/main/java/.../lsp/
        ├── DslLauncher.java            # Standard I/O Launcher entry point
        ├── DslLanguageServer.java      # Server capability negotiation & lifecycle
        ├── DslTextDocumentService.java # Document open/change/completion/hover/diagnostics
        ├── DslWorkspaceService.java    # Workspace configuration & folder management
        ├── validator/
        │   └── DslDocumentValidator.java # ANTLR syntax + FSM state diagnostics (via DslEngine)
        └── completion/
            └── DslCompletionProvider.java # FSM-aware completion item generation
```

## Maven Dependencies (`<domain>-lsp/pom.xml`)

```xml
<dependencies>
    <!-- Reusable DSL Library -->
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

## Capability Negotiation (`DslLanguageServer`)

Configure capabilities in `initialize()`:

```java
@Override
public CompletableFuture<InitializeResult> initialize(InitializeParams params) {
    ServerCapabilities capabilities = new ServerCapabilities();
    capabilities.setTextDocumentSync(TextDocumentSyncKind.Full);
    capabilities.setCompletionProvider(new CompletionOptions(true, List.of(" ", ".")));
    capabilities.setHoverProvider(true);
    capabilities.setDocumentSymbolProvider(true);

    return CompletableFuture.completedFuture(new InitializeResult(capabilities));
}
```

## Document Diagnostics (`DslDocumentValidator`)

Convert ANTLR errors and FSM transition violations into LSP `Diagnostic` items:

```java
public List<Diagnostic> validateDocument(String text) {
    List<Diagnostic> diagnostics = new ArrayList<>();
    
    // 1. ANTLR Syntax Check via reusable DSL library
    CharStream charStream = CharStreams.fromString(text);
    OrderDslLexer lexer = new OrderDslLexer(charStream);
    CommonTokenStream tokens = new CommonTokenStream(lexer);
    OrderDslParser parser = new OrderDslParser(tokens);
    
    parser.removeErrorListeners();
    parser.addErrorListener(new BaseErrorListener() {
        @Override
        public void syntaxError(Recognizer<?, ?> recognizer, Object offendingSymbol,
                                int line, int charPositionInLine, String msg,
                                RecognitionException e) {
            Position pos = new Position(line - 1, charPositionInLine);
            Range range = new Range(pos, new Position(line - 1, charPositionInLine + 1));
            diagnostics.add(new Diagnostic(range, msg, DiagnosticSeverity.Error, "dsl-parser"));
        }
    });

    OrderDslParser.ScriptContext tree = parser.script();

    // 2. FSM State Transition Validation (if syntax clean)
    if (diagnostics.isEmpty()) {
        diagnostics.addAll(validateFsmTransitions(tree));
    }

    return diagnostics;
}
```

## State-Aware Completion (`DslCompletionProvider`)

```java
public List<CompletionItem> getCompletions(String text, Position position) {
    List<CompletionItem> items = new ArrayList<>();
    
    // Determine FSM state up to current line using reusable DSL engine
    OrderState currentState = computeStateUpTo(text, position.getLine());
    
    // Suggest only valid transitions from the current state
    for (String transition : currentState.validTransitions()) {
        CompletionItem item = new CompletionItem(transition.toLowerCase());
        item.setKind(CompletionItemKind.Keyword);
        item.setDetail("Valid transition from " + currentState.name());
        items.add(item);
    }

    return items;
}
```
