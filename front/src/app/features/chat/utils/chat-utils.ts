export function formatChatTime(dateStr: string): string {
  const date = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 1) return 'الآن';
  if (diffMins < 60) return `منذ ${diffMins} د`;

  if (diffDays === 0) {
    return date.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' });
  }
  if (diffDays === 1) return 'أمس';
  if (diffDays < 7) return date.toLocaleDateString('ar-EG', { weekday: 'long' });

  return date.toLocaleDateString('ar-EG', { day: 'numeric', month: 'short' });
}

export function formatConversationTime(dateStr: string): string {
  const date = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffDays === 0) {
    return date.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' });
  }
  if (diffDays === 1) return 'أمس';
  return date.toLocaleDateString('ar-EG', { day: 'numeric', month: 'short' });
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1048576).toFixed(1)} MB`;
}

export function isEmojiOnly(text: string): boolean {
  const emojiRegex = /^[\p{Emoji}\p{Emoji_Presentation}\p{Emoji_Modifier}\p{Emoji_Component}\s]+$/u;
  return emojiRegex.test(text.trim());
}

export function truncateText(text: string, maxLen: number): string {
  if (text.length <= maxLen) return text;
  return text.slice(0, maxLen) + '...';
}
