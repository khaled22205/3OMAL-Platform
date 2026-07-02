# AI Assistant Architecture

## Overview

The AI Assistant is an integrated platform feature that uses Google Gemini with a lightweight RAG (Retrieval-Augmented Generation) system to answer questions about the 3OMAL platform.

## Architecture Diagram

```
Frontend (Angular 22)
    │
    ├── REST API ──────► AiController ──► IAiAssistantService ──► IAiProvider (Gemini)
    │                                                    │
    └── SignalR ───────► AiChatHub ──────► IAiAssistantService ──► IKnowledgeService
                                                                        │
                                                               ┌────────┴────────┐
                                                               │  TF-IDF Index   │
                                                               │  (In-Memory)    │
                                                               └────────┬────────┘
                                                                        │
                                                               ┌────────┴────────┐
                                                               │   AppDbContext  │
                                                               │   (EF Core)     │
                                                               └─────────────────┘
```

## Layers

### Domain Layer (`Domain/Entities/`)
- `AiConversation` — Aggregate root, tracks user conversations with language detection
- `AiMessage` — Individual messages with role (User/Assistant/System)
- `AiContextReference` — Knowledge source provenance (what data was used to generate the response)
- `AiUsageLog` — Token usage and performance tracking

### Application Layer (`Application/Features/AiAssistant/`)
- `IAiProvider` — Abstract LLM provider (Gemini, future: OpenAI, Claude, etc.)
- `IAiConversationService` — Conversation CRUD interface
- `IAiAssistantService` — Orchestrator: auth → retrieve → build prompt → call AI → persist
- `IEmbeddingService` — Embedding generation and similarity computation
- `IKnowledgeService` — Knowledge retrieval with role-based filtering
- DTOs — Request/Response models for all AI operations

### Infrastructure Layer (`Infrastructure/Services/`)
- `GeminiProvider` — Raw HTTP client for Gemini API with streaming and retry logic
- `AiConversationService` — EF Core persistence for conversations
- `AiAssistantService` — Orchestration of the full AI pipeline
- `TfIdfEmbeddingService` — Lightweight TF-IDF vectorizer + cosine similarity
- `KnowledgeService` — Indexes platform data and retrieves relevant context
- `AiContextBuilder` — Constructs the prompt pipeline

### API Layer (`API/`)
- `AiController` — REST endpoints for conversations and suggestions
- `AiChatHub` — SignalR hub for token-by-token streaming responses

## Key Design Decisions

1. **Provider Abstraction** — `IAiProvider` wraps all LLM communication, making it easy to swap Gemini for another provider
2. **Raw HTTP over SDK** — Gemini is called via `HttpClient` for smaller footprint, easier debugging, and cleaner testing
3. **TF-IDF over External Embeddings** — Initial RAG uses in-memory TF-IDF to avoid external API dependencies. The `IEmbeddingService` interface allows upgrading to Google/OpenAI embeddings later
4. **SignalR Streaming** — Reuses existing SignalR infrastructure for real-time token-by-token responses
5. **Soft Delete** — Conversations and messages follow the existing `ISoftDelete` pattern
