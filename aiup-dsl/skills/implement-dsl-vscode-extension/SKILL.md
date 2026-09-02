---
name: implement-dsl-vscode-extension
description: >
  Implements a Visual Studio Code extension that packages and launches the Java
  Eclipse LSP4J server for a Domain-Specific Language (DSL). Generates the
  extension manifest (package.json), TextMate syntax grammar (.tmLanguage.json),
  language configuration, and a TypeScript LSP client launcher using
  vscode-languageclient to spawn the runnable Java LSP JAR over stdio. Use when
  the user asks to "create VS Code extension", "build DSL extension", "implement
  VS Code tooling", or mentions VS Code extension generation.
---

<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Implement DSL Visual Studio Code Extension

## Goal

Implement a Visual Studio Code extension project in `<domain>-vscode/` that configures syntax highlighting, editor
settings, and launches the Java Eclipse LSP4J language server daemon created by `/implement-dsl-lsp`.

## If an Implementation Already Exists

Before generating new files, check whether `<domain>-vscode/` or a VS Code extension directory already exists:
- If it exists, update the existing extension configuration in place.
- Reconcile keyword tokens in `syntaxes/<domain>.tmLanguage.json` with recent grammar updates.
- Update `package.json` command or configuration settings as needed.
- Recompile with `npm run compile`.

## Workflow & Conventions

1. **Locate LSP Server JAR & Grammar**:
   - Verify the location of the compiled Java LSP server JAR (e.g. `../<domain>-lsp/target/<domain>-lsp.jar`).
   - Identify language ID (e.g. `orderdsl`), file extension (e.g. `.orderdsl` or `.dsl`), and syntax tokens from the ANTLR grammar.

2. **Scaffold Extension Project (`<domain>-vscode/`)**:
   - Create directory structure:
     ```text
     <domain>-vscode/
     ├── package.json
     ├── tsconfig.json
     ├── language-configuration.json
     ├── syntaxes/
     │   └── <domain>.tmLanguage.json
     └── src/
         └── extension.ts
     ```

3. **Extension Manifest (`package.json`)**:
   - Register the language, file extensions, grammars, and settings:
     ```json
     {
       "name": "<domain>-vscode",
       "displayName": "<Domain> DSL Support",
       "version": "0.1.0",
       "engines": { "vscode": "^1.90.0" },
       "categories": ["Programming Languages"],
       "main": "./out/extension.js",
       "activationEvents": ["onLanguage:<domain>"],
       "contributes": {
         "languages": [{
           "id": "<domain>",
           "aliases": ["<Domain> DSL", "<domain>"],
           "extensions": [".<domain>", ".dsl"],
           "configuration": "./language-configuration.json"
         }],
         "grammars": [{
           "language": "<domain>",
           "scopeName": "source.<domain>",
           "path": "./syntaxes/<domain>.tmLanguage.json"
         }],
         "configuration": {
           "title": "<Domain> DSL",
           "properties": {
             "<domain>.lsp.jarPath": {
               "type": "string",
               "default": "",
               "description": "Custom path to the runnable Java LSP server JAR."
             },
             "<domain>.lsp.javaPath": {
               "type": "string",
               "default": "java",
               "description": "Path to the java runtime binary."
             }
           }
         }
       }
     }
     ```

4. **TextMate Syntax Highlighting (`syntaxes/<domain>.tmLanguage.json`)**:
   - Define token regexes for keywords (`keyword.control`), strings (`string.quoted.double`), numbers (`constant.numeric`),
     and comments (`comment.line.double-slash`).
   - Consult [references/extension-structure.md](references/extension-structure.md) for full syntax template.

5. **Language Configuration (`language-configuration.json`)**:
   - Define line comments (`//`), block comments (`/* */`), brackets (`{}`, `[]`, `()`), and auto-closing pairs.

6. **Java LSP Client Launcher (`src/extension.ts`)**:
   - Use `vscode-languageclient/node` to resolve the Java runtime and launch the LSP server JAR via stdio:
     ```typescript
     import * as path from 'path';
     import * as vscode from 'vscode';
     import { LanguageClient, LanguageClientOptions, ServerOptions } from 'vscode-languageclient/node';

     let client: LanguageClient;

     export function activate(context: vscode.ExtensionContext) {
         const config = vscode.workspace.getConfiguration('<domain>');
         const javaPath = config.get<string>('lsp.javaPath') || 'java';
         const customJar = config.get<string>('lsp.jarPath');
         const serverJar = customJar || context.asAbsolutePath(path.join('server', '<domain>-lsp.jar'));

         const serverOptions: ServerOptions = {
             run: { command: javaPath, args: ['-jar', serverJar] },
             debug: { command: javaPath, args: ['-jar', serverJar] }
         };

         const clientOptions: LanguageClientOptions = {
             documentSelector: [{ scheme: 'file', language: '<domain>' }],
             synchronize: {
                 fileEvents: vscode.workspace.createFileSystemWatcher('**/*.<domain>')
             }
         };

         client = new LanguageClient('<domain>LanguageServer', '<Domain> Language Server', serverOptions, clientOptions);
         client.start();
     }

     export function deactivate(): Thenable<void> | undefined {
         return client ? client.stop() : undefined;
     }
     ```

7. **Compilation & Packaging**:
   - Run `npm install` and `npm run compile`.
   - Package into a `.vsix` extension file using `npx @vscode/vsce package`.

8. **Next Step Guidance**:
   - Conclude with instructions for running the extension locally in VS Code (press `F5` to open an Extension Development Host)
     or installing the generated `.vsix` file.

