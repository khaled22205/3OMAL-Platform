# AI Assistant Database Schema

## AiConversations

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| UserId | int (FK → Users) | Owner of the conversation |
| Title | nvarchar(200) | Auto-generated from first message |
| Language | nvarchar(10) | Detected: "ar" or "en" |
| IsDeleted | bit | Soft delete |
| DeletedAt | datetime2 | |
| CreatedAt | datetime2 | |
| UpdatedAt | datetime2 | |

Indexes: `UserId`, `CreatedAt`

## AiMessages

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| ConversationId | int (FK → AiConversations) | Parent conversation |
| Role | nvarchar(20) | "User" / "Assistant" / "System" |
| Content | nvarchar(max) | Message body |
| SourcesJson | nvarchar(max) | JSON array of knowledge source refs |
| PromptTokens | int | Token usage for this message |
| ResponseTokens | int | Token usage for this message |
| IsDeleted | bit | Soft delete |
| DeletedAt | datetime2 | |
| CreatedAt | datetime2 | |

Indexes: `ConversationId`, `(ConversationId, CreatedAt)`

## AiContextReferences

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| MessageId | int (FK → AiMessages) | Parent message |
| SourceType | nvarchar(50) | "category", "service", "worker", "faq", "doc" |
| SourceId | int | ID of the referenced entity |
| Title | nvarchar(300) | Human-readable title |
| Excerpt | nvarchar(max) | Text excerpt used in context |
| RelevanceScore | float | Similarity score (0-1) |
| CreatedAt | datetime2 | |

Indexes: `MessageId`

## AiUsageLogs

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| UserId | int? (FK → Users) | Nullable for guests |
| Role | nvarchar(20) | User role at time of request |
| PromptTokens | int | |
| ResponseTokens | int | |
| RetrievalDurationMs | int | Time spent retrieving knowledge |
| TotalDurationMs | int | Total request time |
| Model | nvarchar(100) | Gemini model used |
| IsError | bit | Whether the request failed |
| ErrorMessage | nvarchar(1000) | Error description if failed |
| CreatedAt | datetime2 | |

Indexes: `UserId`, `CreatedAt`

## Relationships

```
AiConversation 1 ──── * AiMessage
AiMessage 1 ──── * AiContextReference
AiUsageLog * ──── 0..1 User (via UserId)
```
