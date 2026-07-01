# Running Tests

## Prerequisites

- .NET 10 SDK
- Restore packages: `dotnet restore`

## Commands

```bash
# Run all tests
dotnet test

# Run single project
dotnet test tests/Domain.Tests
dotnet test tests/Application.Tests
dotnet test tests/Infrastructure.Tests
dotnet test tests/API.Tests

# Run with filter
dotnet test --filter "FullyQualifiedName~Booking"
dotnet test --filter "Category=Unit"

# Run with verbose output
dotnet test -v n

# Build without running
dotnet build
dotnet build tests/Domain.Tests
```

## CI (GitHub Actions)

Tests run automatically on push and pull requests via `.github/workflows/ci.yml`.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| `GetValue<T>()` cannot be mocked | Use `ConfigurationBuilder` + `AddInMemoryCollection` |
| `FirstOrDefaultAsync` not found | Add `using Microsoft.EntityFrameworkCore` |
| `IsActive` unmapped | Rewrite query with `!IsRevoked && !IsExpired` |
| `ControllerContext` set but `HttpContext` null | Use `DefaultHttpContext` |
