# Test Architecture

## Directory Layout

```
tests/
├── Domain.Tests/           # Domain layer
│   ├── BookingTests.cs      # State machine transitions
│   ├── CommissionCalculatorTests.cs
│   ├── DomainExceptionTests.cs
│   ├── PaymentTests.cs
│   ├── RefreshTokenTests.cs
│   ├── ReviewTests.cs
│   ├── StringHelperTests.cs
│   └── WorkerProfileTests.cs
├── Application.Tests/      # Application layer
│   ├── MappingHelperTests.cs  # All 10 entity-to-DTO mappings
│   └── ValidatorTests.cs      # All 7 validators with boundary/edge cases
├── Infrastructure.Tests/   # Infrastructure layer
│   ├── AuthServiceTests.cs
│   ├── CurrentUserServiceTests.cs
│   ├── FileServiceTests.cs
│   └── JwtServiceTests.cs
└── API.Tests/              # API / Presentation layer
    ├── Controllers/
    │   ├── AdminControllerTests.cs
    │   ├── AuthControllerTests.cs
    │   ├── BaseApiControllerTests.cs
    │   ├── BookingsControllerTests.cs
    │   ├── CategoriesControllerTests.cs
    │   ├── FavoritesControllerTests.cs
    │   ├── PaymentsControllerTests.cs
    │   ├── ReviewsControllerTests.cs
    │   ├── ServicesControllerTests.cs
    │   └── WorkersControllerTests.cs
    └── Middleware/
        └── ExceptionHandlingMiddlewareTests.cs
```
