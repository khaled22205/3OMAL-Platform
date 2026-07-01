# Unit Testing Guide

## Naming Convention

`{Method}_{Scenario}_{ExpectedResult}`

Examples:
- `GenerateTokens_Should_return_access_token_with_correct_claims`
- `RegisterAsync_Should_return_failure_when_email_already_exists`

## Structure

```csharp
public class SomeTests
{
    // Dependencies: mocks first, real dependencies second
    private readonly Mock<IDependency> _depMock = new();
    private readonly RealDependency _realDep;

    public SomeTests()
    {
        // Constructor: set up real dependencies (InMemory DB, ConfigurationBuilder, etc.)
    }

    private Target CreateService() => new(_depMock.Object, _realDep);

    [Fact]
    public void Method_Scenario_Expected() { /* AAA */ }
}
```

## Patterns

### Mock setup
```csharp
_depMock.Setup(x => x.SomeMethodAsync(It.IsAny<int>())).ReturnsAsync(result);
```

### Verify calls
```csharp
_depMock.Verify(x => x.SomeMethod(It.IsAny<int>()), Times.Once);
```

### Exceptions
```csharp
act.Should().Throw<DomainException>().WithMessage("*already*");
```

### EF Core InMemory
```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
_context = new AppDbContext(options);
```

### IConfiguration (cannot mock — use real builder)
```csharp
_config = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Jwt:Key"] = "key",
    })
    .Build();
```

### Testing protected members (API controllers)
```csharp
public class TestableController : BaseApiController
{
    public int TestGetUserId() => GetUserId();
}
```
