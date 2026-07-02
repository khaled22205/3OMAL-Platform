# AI Assistant Security Model

## Authentication

- JWT Bearer token required for all REST endpoints and SignalR connections
- SignalR token passed via `access_token` query parameter (reuses existing configuration)
- Token validated on every hub method call

## Authorization Rules

| Rule | Enforced In | Mechanism |
|------|-------------|-----------|
| Only authenticated users can use AI | Controller + Hub | `[Authorize]` attribute |
| User can only access own conversations | Service | `conversation.UserId == userId` check |
| Guest can only ask platform questions | Controller | `GetUserRole()` returns "Guest" for unauthenticated |
| Admin can query analytics | Service | Role injected into system prompt |
| Worker can see own data only | Service | Role injected into system prompt |
| Customer can see own data only | Service | Role injected into system prompt |

## Defense in Depth

1. **API Layer** — `[Authorize]` attribute on controller endpoints
2. **Application Layer** — `IAiAssistantService` receives user role and enforces access
3. **Prompt Layer** — `AiContextBuilder` injects role constraints into the system prompt
4. **Knowledge Layer** — `KnowledgeService` limits retrieved data based on role (future enhancement)

## Data Protection

- User prompts are NOT logged in plain text (only metadata)
- JWT tokens, passwords, API keys are NEVER logged
- Prompts are sanitized before sending to Gemini (SQL keywords, internal IDs stripped)
- The AI NEVER generates SQL or database modification commands

## API Key Security

- Gemini API key stored in `appsettings.json` (or environment variables / User Secrets)
- NOT hardcoded in source code
- NOT exposed to the frontend
- NOT logged by Serilog

## Rate Limiting

- Rate limiting applied to AI endpoints (configurable in `AiAssistant` config section)
- Default: 10 requests/minute per user, 100 requests/day
- Existing global rate limiting (60 req/min) also applies
