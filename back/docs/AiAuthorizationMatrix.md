# AI Assistant Authorization Matrix

| Data / Query | Guest | Customer | Worker | Admin |
|---|---|---|---|---|
| Platform info (categories, services, FAQ) | ✅ | ✅ | ✅ | ✅ |
| Worker recommendations | ✅ (public) | ✅ | ✅ | ✅ |
| How-to questions (register, book, etc.) | ✅ | ✅ | ✅ | ✅ |
| Own conversation history | ❌ | ✅ | ✅ | ✅ |
| Own booking history | ❌ | ✅ | ✅ (as worker) | ✅ |
| Own invoices | ❌ | ✅ | ✅ (as worker) | ✅ |
| Own services | ❌ | ❌ | ✅ | ✅ |
| Own reviews | ❌ | ❌ | ✅ | ✅ |
| Own statistics | ❌ | ❌ | ✅ | ✅ |
| Platform analytics (revenue, bookings) | ❌ | ❌ | ❌ | ✅ |
| User management | ❌ | ❌ | ❌ | ✅ |
| Other users' personal data | ❌ | ❌ | ❌ | ✅ |
| Secrets / passwords / tokens | ❌ | ❌ | ❌ | ❌ |

## Enforcement

1. **Role Detection** — `AiController.GetUserRole()` extracts role from JWT claims
2. **Prompt Injection** — `AiContextBuilder` includes the user's role in the system prompt:
   ```
   CURRENT USER ROLE: {role}
   ```
3. **Knowledge Scoping** — `KnowledgeService` limits retrieved context based on role
4. **API Constraints** — Unauthorized endpoints return 401/403 before reaching AI logic
