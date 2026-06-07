# SchemaFirst Generator

Companion implementation for the paper:  
**"A Single-Source-of-Truth Approach to Avoiding Data Inconsistencies in Multi-Tier Architectures"**  
Marcel Genger, Sebastian Huber

---

## Overview

The SchemaFirst generator reads a `.schema` file (DSL) and automatically generates artifacts for every registered target language or format. The included visitors produce SQL DDL, C# entity classes and DTOs, and TypeScript interfaces — but the architecture is open: any output target can be added by implementing a single interface.

---

## Project Structure

```
SchemaFirst.sln
├── SchemaFirst.Generator/        # Core: Lexer, Parser, AST, Visitors
│   ├── Lexer/
│   │   ├── Token.cs              # Token types
│   │   └── Lexer.cs              # Tokenizer
│   ├── Parser/
│   │   └── Parser.cs             # Recursive descent parser
│   ├── Ast/
│   │   └── AstNodes.cs           # AST nodes (SchemaNode, EntityNode, ...)
│   ├── Visitors/
│   │   ├── IVisitor.cs           # Visitor interface + TypeMap
│   │   ├── SqlVisitor.cs         # → SQL DDL
│   │   ├── CSharpVisitor.cs      # → C# class + record DTO
│   │   └── TypeScriptVisitor.cs  # → TypeScript interface + DTO
│   └── SchemaGenerator.cs        # Orchestration
├── SchemaFirst.Cli/               # Command-line tool
│   └── Program.cs
└── SchemaFirst.Example/           # Example from the paper
    ├── schemas/User.schema
    └── generated/                 # Populated when running the CLI
```

---

## Quick Start

### Using the CLI

```bash
# Build the solution
dotnet build

# Single file
dotnet run --project SchemaFirst.Cli -- \
    SchemaFirst.Example/schemas/User.schema \
    SchemaFirst.Example/generated/

# Multiple files
dotnet run --project SchemaFirst.Cli -- \
    SchemaFirst.Example/schemas/User.schema \
    SchemaFirst.Example/schemas/Product.schema \
    SchemaFirst.Example/generated/

# Entire folder (all *.schema files)
dotnet run --project SchemaFirst.Cli -- \
    SchemaFirst.Example/schemas/ \
    SchemaFirst.Example/generated/
```

**Output (folder mode, two files):**
```
Processing User.schema...
  ✓  SQL DDL         → generated/User.generated.sql
  ✓  C#              → generated/User.generated.cs
  ✓  TypeScript      → generated/User.generated.ts
Processing Product.schema...
  ✓  SQL DDL         → generated/Product.generated.sql
  ✓  C#              → generated/Product.generated.cs
  ✓  TypeScript      → generated/Product.generated.ts

Done. 2 file(s) processed.
```

### Running the Example Project

```bash
dotnet run --project SchemaFirst.Example
```

---

## DSL Syntax

```
entity <Name> {
    <fieldName> : <Type> [annotations...]
}
```

### Types

| DSL       | SQL            | C#         | TypeScript |
|-----------|----------------|------------|------------|
| `Int`     | `INTEGER`      | `int`      | `number`   |
| `Text`    | `VARCHAR(255)` | `string`   | `string`   |
| `Boolean` | `BOOLEAN`      | `bool`     | `boolean`  |
| `Date`    | `DATE`         | `DateOnly` | `Date`     |
| `Decimal` | `DECIMAL(10,2)`| `decimal`  | `number`   |

### Annotations

| Annotation        | Meaning |
|-------------------|---------|
| `@primaryKey`     | Primary key (implies `@required`) |
| `@required`       | NOT NULL in SQL, non-nullable in target language |
| `@exposed`        | Field is included in the generated DTO |
| `@default(value)` | Default value (SQL DEFAULT, initializer in target language) |

### Example

```
entity User {
  id           : Int     @primaryKey
  username     : Text    @required  @exposed
  email        : Text    @required  @exposed
  birthdate    : Date               @exposed
  isActive     : Boolean @default(true)
  passwordHash : Text    @required
}
```

Generates:
- **SQL:** `CREATE TABLE User (...)` with `PRIMARY KEY (id)`
- **C#:** `class User` (all fields) + `record UserDto(string Username, string Email, DateOnly? Birthdate)`
- **TypeScript:** `interface User` (all fields) + `interface UserDto` (exposed fields only)

---

## Adding a New Target

1. Create a class implementing `IVisitor` (e.g. `JavaVisitor.cs`)
2. Register it in the `SchemaGenerator` constructor — done.  
   The DSL and all other visitors remain unchanged.

```csharp
var generator = new SchemaGenerator(new IVisitor[]
{
    new SqlVisitor(),
    new CSharpVisitor(),
    new TypeScriptVisitor(),
    new JavaVisitor(),   // new
});
```
