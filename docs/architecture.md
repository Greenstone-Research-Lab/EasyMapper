# Architecture

EasyMapper is convention-first and configuration-optional. The public facade stays small while
the execution pipeline separates discovery, planning, compilation, caching, and execution.

```text
EasyMapper facade / MapTo extension
                 |
               Mapper
                 |
         MappingPlanProvider
          /              \
 MappingPlanCache    MappingPlanCompiler
                          |
               compiled mapping delegate
```

## Design rules

- Reflection is used only when a type pair is first inspected.
- Repeated mapping executes a cached delegate and performs no property lookup.
- Call-specific fluent rules produce an isolated plan and never mutate global behavior.
- Mapper options are snapshotted at construction, making mapper instances deterministic and thread-safe.
- Public APIs expose mapping concepts; reflection and expression implementation details remain internal.

## SOLID responsibilities

- `Mapper` coordinates a mapping operation.
- `MappingPlanProvider` owns plan reuse.
- `MappingPlanCompiler` discovers properties and compiles assignment expressions.
- `MappingDefinition` stores fluent exceptions to conventions.
- `ScalarConverter` owns supported cross-type scalar conversions.

Source-generation support can later provide plans through the same boundary without changing the
zero-configuration public API.
