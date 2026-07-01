# Chat System Extension Points

## Adding New Message Types

1. Add value to `Domain/Enums/MessageType.cs` (e.g., `VoiceNote`)
2. Add rendering in `front/src/app/features/chat/components/message-feed.ts`
3. Add upload support in `front/src/app/features/chat/components/message-input.ts`

No backend changes needed — the enum handles it.

## Adding Voice/Video Calls

1. Create `Application/Features/Calls/ICallService.cs` and DTOs
2. Create `Infrastructure/Services/CallService.cs`
3. Create `API/Hubs/CallHub.cs` — separate hub for call signaling
4. Frontend: integrate WebRTC library
5. The existing `ConnectionManager` tracks user presence for call routing

## Adding File Storage Providers (Azure Blob, AWS S3)

1. Implement `IFileService` with a new provider
2. Register in DI switch
3. The `MessageAttachment` entity stores file path — swap provider without schema changes

## Adding Group Chats

1. Add `ConversationType` enum (OneToOne, Group)
2. Modify `ConversationParticipant` to store role (Admin, Member)
3. Add `ConversationName` to Conversation entity
4. Create new SignalR groups per conversation
5. Frontend: add group management UI

## Adding AI Assistant

1. Create `Application/Features/ChatAi/` with AI service interface
2. The existing `SendMessage` hub method can trigger AI processing
3. AI responses sent as `NewMessage` events from a bot user account
4. No UI changes needed — AI messages appear as normal messages

## Adding Push Notifications

1. Create `Application/Common/Interfaces/IPushNotificationService.cs`
2. Implement with Firebase/FCM
3. Hook into `ChatHub.OnDisconnectedAsync` or `ChatService.SendMessageAsync`
4. Send push when user is offline

## Adding Message Reactions

1. Add `MessageReaction` entity (MessageId, UserId, Reaction)
2. Add `MessageReactionResponse` DTO
3. Add `ReactToMessage` hub method
4. Broadcast `MessageReacted` event

## Adding Conversation Pinning

1. Add `IsPinned` and `PinnedAt` to `ConversationParticipant`
2. Sort conversations by pinned status in query
3. Frontend: add pin/unpin button in conversation list

## Adding Archived Conversations

1. Add `IsArchived` and `ArchivedAt` to `ConversationParticipant`
2. Add `ArchiveConversation`/`UnarchiveConversation` service methods
3. Add query filter for archived/active
4. Frontend: add archive section

## Horizontal Scaling (Redis Backplane)

1. Add `Microsoft.AspNetCore.SignalR.StackExchangeRedis` NuGet
2. Configure in `Program.cs`:
```csharp
builder.Services.AddSignalR()
    .AddStackExchangeRedis("connectionString");
```
3. The hub, groups, and connection management work unchanged
