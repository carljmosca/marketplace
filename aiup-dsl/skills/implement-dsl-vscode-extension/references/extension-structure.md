<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Visual Studio Code Extension Structure & TextMate Grammar

This reference provides a template for the VS Code extension manifest, build configuration, and TextMate syntax highlighting.

## `package.json` Template

```json
{
  "name": "my-dsl-vscode",
  "displayName": "My Domain Specific Language",
  "description": "Language support and LSP tooling for My DSL",
  "version": "0.1.0",
  "publisher": "my-org",
  "engines": {
    "vscode": "^1.90.0"
  },
  "categories": [
    "Programming Languages"
  ],
  "main": "./out/extension.js",
  "activationEvents": [
    "onLanguage:mydsl"
  ],
  "contributes": {
    "languages": [
      {
        "id": "mydsl",
        "aliases": ["My DSL", "mydsl"],
        "extensions": [".mydsl", ".dsl"],
        "configuration": "./language-configuration.json"
      }
    ],
    "grammars": [
      {
        "language": "mydsl",
        "scopeName": "source.mydsl",
        "path": "./syntaxes/mydsl.tmLanguage.json"
      }
    ],
    "commands": [
      {
        "command": "mydsl.restartServer",
        "title": "My DSL: Restart Language Server"
      }
    ]
  },
  "scripts": {
    "vscode:prepublish": "npm run compile",
    "compile": "tsc -p ./",
    "watch": "tsc -watch -p ./"
  },
  "devDependencies": {
    "@types/node": "^20.x",
    "@types/vscode": "^1.90.0",
    "typescript": "^5.4.0"
  },
  "dependencies": {
    "vscode-languageclient": "^9.0.1"
  }
}
```

## `syntaxes/mydsl.tmLanguage.json` Template

```json
{
  "$schema": "https://raw.githubusercontent.com/martinring/tmlanguage/master/tmlanguage.json",
  "name": "My DSL",
  "scopeName": "source.mydsl",
  "patterns": [
    { "include": "#comments" },
    { "include": "#strings" },
    { "include": "#numbers" },
    { "include": "#keywords" }
  ],
  "repository": {
    "comments": {
      "patterns": [
        {
          "name": "comment.line.double-slash.mydsl",
          "match": "//.*$"
        },
        {
          "name": "comment.block.mydsl",
          "begin": "/\\*",
          "end": "\\*/"
        }
      ]
    },
    "strings": {
      "patterns": [
        {
          "name": "string.quoted.double.mydsl",
          "begin": "\"",
          "end": "\"",
          "patterns": [
            {
              "name": "constant.character.escape.mydsl",
              "match": "\\\\."
            }
          ]
        }
      ]
    },
    "numbers": {
      "patterns": [
        {
          "name": "constant.numeric.mydsl",
          "match": "\\b\\d+(\\.\\d+)?\\b"
        }
      ]
    },
    "keywords": {
      "patterns": [
        {
          "name": "keyword.control.mydsl",
          "match": "\\b(?i:create|new|make|add|include|submit|send|cancel|abort|for|with|at|order|item|customer|quantity|price)\\b"
        }
      ]
    }
  }
}
```

## `language-configuration.json` Template

```json
{
  "comments": {
    "lineComment": "//",
    "blockComment": ["/*", "*/"]
  },
  "brackets": [
    ["{", "}"],
    ["[", "]"],
    ["(", ")"]
  ],
  "autoClosingPairs": [
    { "open": "{", "close": "}" },
    { "open": "[", "close": "]" },
    { "open": "(", "close": ")" },
    { "open": "\"", "close": "\"", "notIn": ["string"] }
  ],
  "surroundingPairs": [
    ["{", "}"],
    ["[", "]"],
    ["(", ")"],
    ["\"", "\""]
  ]
}
```

