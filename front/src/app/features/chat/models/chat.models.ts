export interface UserBriefResponse {
  userId: number;
  firstName: string;
  lastName: string;
  photo: string | null;
}

export interface ConversationResponse {
  id: number;
  otherUser: UserBriefResponse;
  lastMessage: MessageResponse | null;
  unreadCount: number;
  lastMessageAt: string | null;
}

export interface MessageResponse {
  id: number;
  conversationId: number;
  senderId: number;
  senderName: string;
  messageType: string;
  content: string | null;
  replyToMessageId: number | null;
  replyToContent: string | null;
  attachments: AttachmentResponse[];
  createdAt: string;
  deliveredAt: string | null;
  readAt: string | null;
  editedAt: string | null;
  isEdited: boolean;
  isDeleted: boolean;
}

export interface AttachmentResponse {
  id: number;
  fileName: string;
  filePath: string;
  contentType: string;
  fileSize: number;
  attachmentType: string;
}

export interface SendMessageRequest {
  conversationId: number;
  messageType: string;
  content: string | null;
  replyToMessageId: number | null;
}

export interface CreateConversationRequest {
  participantUserId: number;
}

export interface EditMessageRequest {
  content: string;
}

export interface MarkAsReadRequest {
  conversationId: number;
  messageIds: number[];
}

export interface UnreadCountResponse {
  count: number;
}

export type { PagedResult, WrappedResponse } from '../../../core/models/api.models';
