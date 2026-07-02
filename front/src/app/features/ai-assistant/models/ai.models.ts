export interface AiConversationSummary {
  id: number;
  title: string;
  language: string;
  lastMessage: AiMessage | null;
  messageCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface AiConversationDetail {
  id: number;
  title: string;
  language: string;
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
}

export interface SendAiMessageRequest {
  conversationId: number;
  content: string;
}

export interface AiSuggestedPrompts {
  prompts: string[];
}
