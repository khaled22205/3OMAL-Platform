# Test Architecture

## Directory Layout (Updated)

```
tests/
+-- Domain.Tests/              # Domain layer (86 tests)
|   +-- BookingTests.cs
|   +-- CommissionCalculatorTests.cs
|   +-- DomainExceptionTests.cs
|   +-- PaymentTests.cs
|   +-- RefreshTokenTests.cs
|   +-- ReviewTests.cs
|   +-- StringHelperTests.cs
|   +-- WorkerProfileTests.cs
+-- Application.Tests/         # Application layer (54 tests)
|   +-- MappingHelperTests.cs
|   +-- ValidatorTests.cs
|   +-- ChatMappingTests.cs
|   +-- ChatValidatorTests.cs
+-- Infrastructure.Tests/      # Infrastructure layer (46 tests)
|   +-- AuthServiceTests.cs
|   +-- CurrentUserServiceTests.cs
|   +-- FileServiceTests.cs
|   +-- JwtServiceTests.cs
|   +-- ConnectionManagerTests.cs
+-- API.Tests/                 # API controller tests (101 tests)
|   +-- Controllers/
|   |   +-- AdminControllerTests.cs
|   |   +-- AuthControllerTests.cs
|   |   +-- BaseApiControllerTests.cs
|   |   +-- BookingsControllerTests.cs
|   |   +-- CategoriesControllerTests.cs
|   |   +-- ChatControllerTests.cs
|   |   +-- FavoritesControllerTests.cs
|   |   +-- PaymentsControllerTests.cs
|   |   +-- ReviewsControllerTests.cs
|   |   +-- ServicesControllerTests.cs
|   |   +-- WorkersControllerTests.cs
|   +-- Middleware/
|       +-- ExceptionHandlingMiddlewareTests.cs
+-- Integration.Tests/         # NEW - Full-stack integration tests
|   +-- CustomWebApplicationFactory.cs
|   +-- Auth/
|   |   +-- AuthIntegrationTests.cs
|   +-- Controllers/
|   |   +-- CategoriesControllerIntegrationTests.cs
|   +-- Database/
|       +-- MigrationSmokeTests.cs
+-- SignalR.Tests/             # NEW - Real-time hub tests
|   +-- ChatHubTests.cs
+-- TestCommon/                # NEW - Shared test utilities
    +-- Builders/
    |   +-- BookingBuilder.cs
    |   +-- CategoryBuilder.cs
    |   +-- ConversationBuilder.cs
    |   +-- MessageBuilder.cs
    |   +-- UserBuilder.cs
    |   +-- WorkerProfileBuilder.cs
    +-- Factories/
    |   +-- TestDbContextFactory.cs
    |   +-- TestDataFactory.cs
    +-- Fixtures/
        +-- IntegrationTestFixture.cs
```

## Test Principles
- AAA pattern: Arrange, Act, Assert
- One assertion goal per test
- All external dependencies mocked
- InMemory EF Core for database tests
- Real ConfigurationBuilder for config tests

## Running Tests
```bash
dotnet test                              # all projects
dotnet test tests/Domain.Tests           # single project
dotnet test tests/Integration.Tests      # integration tests
dotnet test --filter "FullyQualifiedName~Chat"  # by name
```
