export type UserRole = 'Admin' | 'Worker' | 'Customer' | 'Guest';

export interface AiConversationSummary {
  id: number;
  userId: number | null;
  sessionId: string | null;
  userRole: UserRole;
  title: string;
  language: string;
  isArchived: boolean;
  isHidden: boolean;
  lastMessage: AiMessage | null;
  messageCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface AiConversationDetail {
  id: number;
  userId: number | null;
  sessionId: string | null;
  userRole: UserRole;
  title: string;
  language: string;
  isArchived: boolean;
  isHidden: boolean;
  messages: AiMessage[];
  createdAt: string;
  updatedAt: string | null;
}

export interface AiMessage {
  id: number;
  conversationId: number;
  role: 'User' | 'Assistant' | 'System';
  content: string;
  sources: AiSourceReference[];
  createdAt: string;
}

export interface AiSourceReference {
  sourceType: string;
  sourceId: number;
  title: string | null;
  excerpt: string | null;
  relevanceScore: number;
}

export interface AiStreamChunk {
  conversationId: number;
  messageId?: number;
  content: string;
  isComplete: boolean;
  sources?: AiSourceReference[];
  error?: string;
}

export interface StartConversationRequest {
  title?: string;
  firstMessage?: string;
  sessionId?: string;
}

export interface SendAiMessageRequest {
  conversationId: number;
  content: string;
  sessionId?: string;
}

export interface AiSuggestedPrompts {
  prompts: string[];
}
