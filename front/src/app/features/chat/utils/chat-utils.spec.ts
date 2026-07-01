import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import {
  formatChatTime,
  formatConversationTime,
  formatFileSize,
  isEmojiOnly,
  truncateText,
} from './chat-utils';

describe('chat-utils', () => {
  describe('formatChatTime', () => {
    beforeEach(() => {
      vi.useFakeTimers();
      vi.setSystemTime(new Date('2025-06-15T12:00:00Z'));
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('returns الآن for less than 1 minute', () => {
      expect(formatChatTime(new Date('2025-06-15T12:00:00Z').toISOString())).toBe('الآن');
    });

    it('returns minutes format for less than 1 hour', () => {
      expect(formatChatTime(new Date('2025-06-15T11:55:00Z').toISOString())).toBe('منذ 5 د');
    });

    it('returns time string for today', () => {
      const result = formatChatTime(new Date('2025-06-15T08:00:00Z').toISOString());
      expect(result).toBeTruthy();
      expect(typeof result).toBe('string');
    });

    it('returns أمس for yesterday', () => {
      expect(formatChatTime(new Date('2025-06-14T10:00:00Z').toISOString())).toBe('أمس');
    });

    it('returns weekday for less than 7 days', () => {
      const result = formatChatTime(new Date('2025-06-12T10:00:00Z').toISOString());
      expect(result).toBeTruthy();
    });

    it('returns date for older messages', () => {
      const result = formatChatTime(new Date('2025-05-01T10:00:00Z').toISOString());
      expect(result).toBeTruthy();
    });
  });

  describe('formatConversationTime', () => {
    beforeEach(() => {
      vi.useFakeTimers();
      vi.setSystemTime(new Date('2025-06-15T12:00:00Z'));
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('returns time string for today', () => {
      const result = formatConversationTime(new Date('2025-06-15T08:00:00Z').toISOString());
      expect(result).toBeTruthy();
    });

    it('returns أمس for yesterday', () => {
      expect(formatConversationTime(new Date('2025-06-14T10:00:00Z').toISOString())).toBe('أمس');
    });

    it('returns date for older conversations', () => {
      const result = formatConversationTime(new Date('2025-05-01T10:00:00Z').toISOString());
      expect(result).toBeTruthy();
    });
  });

  describe('formatFileSize', () => {
    it('returns bytes format', () => {
      expect(formatFileSize(500)).toBe('500 B');
    });

    it('returns KB format', () => {
      expect(formatFileSize(1500)).toBe('1.5 KB');
    });

    it('returns MB format', () => {
      expect(formatFileSize(2_000_000)).toBe('1.9 MB');
    });

    it('handles zero bytes', () => {
      expect(formatFileSize(0)).toBe('0 B');
    });

    it('handles exactly 1 KB', () => {
      expect(formatFileSize(1024)).toBe('1.0 KB');
    });
  });

  describe('isEmojiOnly', () => {
    it('detects single emoji', () => {
      expect(isEmojiOnly('😊')).toBe(true);
    });

    it('detects multiple emojis', () => {
      expect(isEmojiOnly('👍❤️😀')).toBe(true);
    });

    it('detects emoji with spaces', () => {
      expect(isEmojiOnly('  😊  ')).toBe(true);
    });

    it('returns false for text with emoji', () => {
      expect(isEmojiOnly('hello 😊')).toBe(false);
    });

    it('returns false for plain text', () => {
      expect(isEmojiOnly('hello')).toBe(false);
    });

    it('returns false for empty string', () => {
      expect(isEmojiOnly('')).toBe(false);
    });
  });

  describe('truncateText', () => {
    it('returns full text when shorter than max', () => {
      expect(truncateText('hello', 10)).toBe('hello');
    });

    it('returns exact text when equal to max', () => {
      expect(truncateText('hello', 5)).toBe('hello');
    });

    it('truncates and adds ellipsis', () => {
      expect(truncateText('hello world', 5)).toBe('hello...');
    });

    it('handles empty string', () => {
      expect(truncateText('', 5)).toBe('');
    });
  });
});
