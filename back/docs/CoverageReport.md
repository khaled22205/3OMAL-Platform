# Coverage Report

## Current Coverage (287 tests)

| Layer | Tests | Key Coverage Areas |
|-------|-------|--------------------|
| Domain | 86 | Booking state machine (all transitions), Payment validation, Review rating bounds, RefreshToken lifecycle, WorkerProfile defaults, CommissionCalculator happy/pathologic, StringHelper edge cases, DomainException |
| Application | 54 | 7 validators (boundary/edge cases on all fields), 10 entity-to-DTO mappings (null handling, collection flattening) |
| Infrastructure | 46 | JWT token generation/claims/expiry/revocation/short-key rejection, AuthService register/login/refresh/changePassword flows, CurrentUserService all 4 methods, FileService save/delete/getUrl |
| API | 101 | 9 controllers (happy path + error responses), BaseApiController helpers + `GetUserId()`, ExceptionHandlingMiddleware (DomainException, ValidationException, Unauthorized, unhandled) |

## Test Distribution

```
Domain:       86  (30%)
Application:  54  (19%)
Infrastructure:46 (16%)
API:         101  (35%)
```

## Gaps / Future Work

- Integration tests (real DB, real JWT signing)
- Performance / load tests
- End-to-end API tests with `WebApplicationFactory`
