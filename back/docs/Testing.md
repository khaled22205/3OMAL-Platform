# Testing Strategy

## Overview

The 3OMAL Platform uses **xUnit**, **Moq**, and **FluentAssertions** for unit testing across all Clean Architecture layers.

## Test Projects (287 tests total)

| Project | Tests | Layer |
|---------|-------|-------|
| Domain.Tests | 86 | Domain entities, services, exceptions |
| Application.Tests | 54 | Validators, mappers |
| Infrastructure.Tests | 46 | JWT, auth, file, current user services |
| API.Tests | 101 | Controllers, middleware |

## Principles

- **AAA pattern**: Arrange, Act, Assert
- **One assertion goal per test** (use multiple `Should().` calls only when testing the same logical outcome)
- **All external dependencies mocked** — never touch real databases, file systems, or HTTP
- **InMemory EF Core** for database-dependent Infrastructure tests
- **Real `ConfigurationBuilder`** for config-dependent tests (Moq can't intercept extension methods like `GetValue<T>()`)

## Running Tests

```bash
dotnet test                       # all projects
dotnet test tests/Domain.Tests    # single project
dotnet test --filter "Booking"    # by name
```
