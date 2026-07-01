# Testing Strategy

## Overview
xUnit + Moq + FluentAssertions for backend. Vitest for frontend. Playwright for E2E.

## Projects
| Project | Type | Tests |
|---------|------|-------|
| Domain.Tests | Unit | 86 |
| Application.Tests | Unit | 54 |
| Infrastructure.Tests | Unit | 46 |
| API.Tests | Unit | 101 |
| Integration.Tests | Integration | NEW |
| SignalR.Tests | Integration | NEW |

## Coverage
Backend: coverlet (OpenCover format, min 80% on Domain + Application)
Frontend: vitest --coverage

## CI
GitHub Actions: push + PR to main. Parallel backend + frontend jobs.
