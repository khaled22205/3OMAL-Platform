# SignalR Communication Flow

## Connection Lifecycle

```
Client                          Server
  │                               │
  ├── Connect (JWT in query) ────►│
  │                               ├── OnConnectedAsync
  │                               │   ├── ConnectionManager.AddConnection
  │                               │   └── Broadcast UserOnline(userId)
  │◄──── Connection Established ──┤
  │                               │
  │── Disconnect ────────────────►│
  │                               ├── OnDisconnectedAsync
  │                               │   ├── ConnectionManager.RemoveConnection
  │                               │   └── If no more connections:
  │                               │       └── Wait 5s, then broadcast UserOffline
  │◄──── UserOffline(userId) ─────┤
```

## Sending a Message

```
Client A                       Hub                          Client B
  │                             │                               │
  ├── SendMessage(request) ────►│                               │
  │                             ├── Validate auth               │
  │                             ├── Validate membership         │
  │                             ├── ChatService.SendMessage()   │
  │                             │   └── Save to DB              │
  │                             ├── Broadcast ─────────────────►│
  │◄──── NewMessage(response) ──┤   NewMessage(response)        │
```

## Read Receipts

```
Client B                       Hub                          Client A
  │                             │                               │
  ├── MarkAsRead(convId, ids) ─►│                               │
  │                             ├── Validate membership         │
  │                             ├── ChatService.MarkAsRead()    │
  │                             │   └── Update DB               │
  │                             ├── Broadcast ─────────────────►│
  │◄── MessagesRead(...) ───────┤   MessagesRead(...)           │
  │                             │                    [Shows blue ✓✓]
```

## Typing Indicator

```
Client A                       Hub                          Client B
  │                             │                               │
  ├── StartTyping(convId) ─────►│                               │
  │                             ├── Broadcast ─────────────────►│
  │                             │   UserTyping(convId, userId)  │
  │                             │                    [Shows dots]
  ├── StopTyping(convId) ──────►│                               │
  │                             ├── Broadcast ─────────────────►│
  │                             │   UserStoppedTyping(...)      │
```

## Editing a Message

```
Client A                       Hub                          Client B
  │                             │                               │
  ├── EditMessage(id, req) ────►│                               │
  │                             ├── Validate ownership          │
  │                             ├── ChatService.EditMessage()   │
  │                             ├── Broadcast ─────────────────►│
  │◄── MessageEdited(response) ─┤   MessageEdited(response)     │
```

## Deleting a Message

```
Client A                       Hub                          Client B
  │                             │                               │
  ├── DeleteMessage(id) ───────►│                               │
  │                             ├── Validate ownership          │
  │                             ├── Get conversationId         │
  │                             ├── ChatService.DeleteMessage() │
  │                             ├── Broadcast ─────────────────►│
  │◄── MessageDeleted(id, uid) ─┤   MessageDeleted(id, uid)    │
```

## Reconnection Flow

```
Client                          Server
  │                               │
  ├── Connection Lost ✗          │
  │                               │
  ├── [Auto Reconnect #1] ──────►│   (after 0ms)
  │   └── Fail                   │
  ├── [Auto Reconnect #2] ──────►│   (after 2s)
  │   └── Fail                   │
  ├── [Auto Reconnect #3] ──────►│   (after 5s)
  │   └── Success ✓              │
  │                               ├── OnConnectedAsync
  │                               ├── Broadcast UserOnline
  │                               │
  ├── JoinConversationGroup ─────►│   (re-join all active groups)
```

## Hub Methods Reference

| Method | Direction | Description |
|--------|-----------|-------------|
| `JoinConversationGroup` | Client→Server | Join SignalR group for conversation |
| `LeaveConversationGroup` | Client→Server | Leave conversation group |
| `SendMessage` | Client→Server | Send new message |
| `EditMessage` | Client→Server | Edit own message |
| `DeleteMessage` | Client→Server | Soft delete own message |
| `MarkAsRead` | Client→Server | Mark messages as read |
| `StartTyping` | Client→Server | Start typing indicator |
| `StopTyping` | Client→Server | Stop typing indicator |
| `NewMessage` | Server→Client | New message broadcast |
| `MessageEdited` | Server→Client | Message edit broadcast |
| `MessageDeleted` | Server→Client | Message delete broadcast |
| `MessagesRead` | Server→Client | Read receipts broadcast |
| `UserTyping` | Server→Client | Typing indicator broadcast |
| `UserStoppedTyping` | Server→Client | Stop typing broadcast |
| `UserOnline` | Server→Client | User came online |
| `UserOffline` | Server→Client | User went offline |
