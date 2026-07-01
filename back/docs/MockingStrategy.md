# Mocking Strategy

## Framework

**Moq** for all interface mocking. **Never mock concrete classes**.

## What to Mock

| Component | What's Mocked | Why |
|-----------|--------------|-----|
| Controllers | `IService` interfaces (e.g. `IBookingService`) | Controller only orchestrates |
| AuthService | `IIdentityService`, `IJwtService`, `ILogger<AuthService>` | All external |
| JwtService | Nothing (uses `IConfiguration` + `AppDbContext`) | Use real builder + InMemory |
| CurrentUserService | `IHttpContextAccessor` | Accessor is a seam |
| FileService | `IWebHostEnvironment` | Don't touch disk |

## Anti-Patterns to Avoid

- ❌ `Mock<IConfiguration>` with `.Setup(x => x.GetValue<T>(...))` — **will fail** because `GetValue<T>` is an extension method
- ✅ Use `new ConfigurationBuilder().AddInMemoryCollection(...)` instead

- ❌ Mocking `DbSet<T>` directly — fragile and error-prone
- ✅ Use `UseInMemoryDatabase` with a real `DbContext`

## Verification Pattern

```csharp
// Verify the method was called with expected args
_depMock.Verify(x => x.SomeMethod(expectedArg), Times.Once);

// Verify no unexpected calls
_depMock.VerifyNoOtherCalls();
```
