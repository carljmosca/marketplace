<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# ANTLR 4 Grammar Design Guidelines for Java DSLs

This reference provides conventions for designing clear, expressive domain grammars with ANTLR 4.

## Grammar File Structure

Create `<Domain>.g4` (or separate `<Domain>Lexer.g4` and `<Domain>Parser.g4`) under `src/main/antlr4/`:

```antlr
grammar OrderDsl;

// Top-level script or single statement
script: statement* EOF;

statement
    : createOrderStmt
    | addItemStmt
    | submitOrderStmt
    | cancelOrderStmt
    ;

createOrderStmt
    : (CREATE | NEW | MAKE) ORDER FOR? CUSTOMER customerId=STRING (WITH? NOTE note=STRING)?
    ;

addItemStmt
    : (ADD | INCLUDE) ITEM itemId=STRING (WITH? QUANTITY qty=INT)? (AT? PRICE price=DECIMAL)?
    ;

submitOrderStmt
    : (SUBMIT | SEND) ORDER (FOR? APPROVAL)?
    ;

cancelOrderStmt
    : (CANCEL | ABORT) ORDER (REASON reason=STRING)?
    ;

// Keywords (Case-Insensitive Recommended)
CREATE: [Cc][Rr][Ee][Aa][Tt][Ee];
NEW: [Nn][Ee][Ww];
MAKE: [Mm][Aa][Kk][Ee];
ORDER: [Oo][Rr][Dd][Ee][Rr];
CUSTOMER: [Cc][Uu][Ss][Tt][Oo][Mm][Ee][Rr];
ITEM: [Ii][Tt][Ee][Mm];
QUANTITY: [Qq][Uu][Aa][Nn][Tt][Ii][Tt][Yy];
PRICE: [Pp][Rr][Ii][Cc][Ee];
SUBMIT: [Ss][Uu][Bb][Mm][Ii][Tt];
SEND: [Ss][Ee][Nn][Dd];
APPROVAL: [Aa][Pp][Pp][Rr][Oo][Vv][Aa][Ll];
CANCEL: [Cc][Aa][Nn][Cc][Ee][Ll];
ABORT: [Aa][Bb][Oo][Rr][Tt];
REASON: [Rr][Ee][Aa][Ss][Oo][Nn];
WITH: [Ww][Ii][Tt][Hh];
FOR: [Ff][Oo][Rr];
AT: [Aa][Tt];

// Literals & Identifiers
STRING: '"' (~["\r\n\\] | '\\' .)* '"';
INT: [0-9]+;
DECIMAL: [0-9]+ '.' [0-9]+;
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

// Comments & Whitespace
COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
WS: [ \t\r\n]+ -> skip;
```

## Error Listener Integration

Implement a custom `ANTLRErrorListener` to collect syntax errors as structured Java objects rather than printing directly to console:

```java
public class DslSyntaxErrorListener extends BaseErrorListener {
    private final List<SyntaxDiagnostic> diagnostics = new ArrayList<>();

    @Override
    public void syntaxError(Recognizer<?, ?> recognizer, Object offendingSymbol,
                            int line, int charPositionInLine, String msg,
                            RecognitionException e) {
        diagnostics.add(new SyntaxDiagnostic(line, charPositionInLine, msg));
    }

    public List<SyntaxDiagnostic> getDiagnostics() { return diagnostics; }
    public boolean hasErrors() { return !diagnostics.isEmpty(); }
}
```

