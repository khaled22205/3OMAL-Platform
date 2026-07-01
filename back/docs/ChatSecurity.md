# Chat Security Model

## Authentication

- JWT Bearer token required for all REST endpoints and SignalR connections
- SignalR token passed via `access_token` query parameter (configured in `Program.cs` `OnMessageReceived` event)
- Token validated on every hub method call

## Authorization Rules

| Rule | Enforced In | Mechanism |
|------|-------------|-----------|
| Only authenticated users connect | Hub + Controller | `[Authorize]` attribute |
| Only Customer can create conversations | Controller | `[Authorize(Roles = "Customer")]` on `POST /conversations` |
| User can only read own conversations | Service | `IsConversationParticipantAsync()` check |
| User can only send messages in own conversations | Service | Membership validation before insert |
| User can only edit own messages | Service | `message.SenderId == userId` check |
| User can only delete own messages | Service | `message.SenderId == userId` check |
| User cannot join arbitrary groups | Hub | `IsConversationParticipantAsync()` before `Groups.AddToGroupAsync` |

## Defense in Depth

Every authorization check exists in both:
1. **SignalR Hub** — Immediate gateway check
2. **Application Service** — Business logic layer check

This ensures that even if someone bypasses the Hub, the service layer still enforces rules.

## Data Protection

- Message content is NOT logged (Serilog excludes `Content` field)
- JWT tokens, passwords, and refresh tokens are NEVER logged
- All sensitive data is redacted by existing `RequestLoggingMiddleware`

## Rate Limiting

- Existing rate limiting in `appsettings.json` applies to REST endpoints
- SignalR has built-in `MaximumReceiveMessageSize` (configured to 128KB)

## CORS

- CORS policy `AllowAll` updated to support `AllowCredentials()` (required for SignalR WebSocket)
- `SetIsOriginAllowed(_ => true)` for development flexibility
