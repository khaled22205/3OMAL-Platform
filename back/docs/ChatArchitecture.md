# Chat System Architecture

## Overview

The chat system implements a real-time one-to-one messaging system between Customers and Workers, similar to WhatsApp (without groups). It uses SignalR for real-time communication and REST APIs for data persistence.

## Architecture Diagram

```
┌──────────────┐     REST/JSON      ┌──────────────────┐
│   Angular    │ ◄────────────────► │  ChatController  │
│   Frontend   │     HTTP API       │  (REST)          │
│              │                    │                  │
│  SignalR     │ ◄── WebSocket ───► │  ChatHub         │
│  Client      │     SignalR        │  (SignalR)       │
└──────────────┘                    └────────┬─────────┘
                                             │
                                    ┌────────┴─────────┐
                                    │   IChatService    │
                                    │   (Application)   │
                                    └────────┬─────────┘
                                             │
                                    ┌────────┴─────────┐
                                    │   ChatService     │
                                    │   (Infrastructure)│
                                    └────────┬─────────┘
                                             │
                                    ┌────────┴─────────┐
                                    │    AppDbContext    │
                                    │    EF Core/SQL    │
                                    └──────────────────┘
```

## Layers

### Domain Layer
- `Conversation` — Core aggregate root, tracks participants and last message
- `ConversationParticipant` — Join entity with read tracking
- `Message` — Individual messages with type, status timestamps
- `MessageAttachment` — File metadata for attachments
- `MessageType` — Enum supporting Text, Image, File, Video, Emoji, Hyperlink, Location

### Application Layer
- `IChatService` — Service interface defining all chat operations
- DTOs — `ConversationResponse`, `MessageResponse`, `UserBriefResponse`, `AttachmentResponse`, etc.
- Validators — FluentValidation for SendMessage, CreateConversation, EditMessage
- Extension methods in `MappingHelper.cs` for entity→DTO mapping

### Infrastructure Layer
- `ChatService` — Full implementation with EF Core, authorization checks, Serilog logging
- `ConnectionManager` — Thread-safe ConcurrentDictionary tracking userId ↔ connectionId mappings
- EF Core configurations in `AppDbContext.OnModelCreating()`

### API Layer
- `ChatController` — REST endpoints for CRUD, search, pagination
- `ChatHub` — SignalR hub for real-time events

## Key Design Decisions

1. **Service pattern over CQRS** — Follows existing project conventions (IFavoriteService → FavoriteService)
2. **Manual mapping** — Extension methods in MappingHelper.cs, no AutoMapper
3. **Soft deletes** — Conversations and Messages use ISoftDelete
4. **Pagination** — Server-side pagination for conversations and messages
5. **SignalR groups** — Each conversation gets a `conv_{id}` group for targeted broadcasting
