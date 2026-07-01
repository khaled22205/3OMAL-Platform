import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { SignalrService } from './signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import * as signalR from '@microsoft/signalr';

vi.mock('@microsoft/signalr', () => {
  const HubConnectionState = { Disconnected: 0, Connected: 1, Reconnecting: 2 };
  return {
    HubConnectionBuilder: vi.fn(),
    HubConnectionState,
  };
});

describe('SignalrService', () => {
  let service: SignalrService;
  let mockAuthService: { getAccessToken: ReturnType<typeof vi.fn> };
  let mockHubConnection: {
    start: ReturnType<typeof vi.fn>;
    stop: ReturnType<typeof vi.fn>;
    invoke: ReturnType<typeof vi.fn>;
    on: ReturnType<typeof vi.fn>;
    off: ReturnType<typeof vi.fn>;
    state: number;
    onreconnecting: ReturnType<typeof vi.fn>;
    onreconnected: ReturnType<typeof vi.fn>;
    onclose: ReturnType<typeof vi.fn>;
  };
  let mockBuilder: {
    withUrl: ReturnType<typeof vi.fn>;
    withAutomaticReconnect: ReturnType<typeof vi.fn>;
    build: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockAuthService = { getAccessToken: vi.fn() };

    mockHubConnection = {
      start: vi.fn(),
      stop: vi.fn(),
      invoke: vi.fn(),
      on: vi.fn(),
      off: vi.fn(),
      state: signalR.HubConnectionState.Disconnected,
      onreconnecting: vi.fn(),
      onreconnected: vi.fn(),
      onclose: vi.fn(),
    };

    mockBuilder = {
      withUrl: vi.fn().mockReturnThis(),
      withAutomaticReconnect: vi.fn().mockReturnThis(),
      build: vi.fn().mockReturnValue(mockHubConnection),
    };

    vi.mocked(signalR.HubConnectionBuilder).mockImplementation(function() { return mockBuilder as any; });

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        SignalrService,
        { provide: AuthService, useValue: mockAuthService },
      ],
    });

    service = TestBed.inject(SignalrService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('startConnection', () => {
    it('should not connect without token', async () => {
      mockAuthService.getAccessToken.mockReturnValue(null);

      await service.startConnection();

      expect(signalR.HubConnectionBuilder).not.toHaveBeenCalled();
      expect(service.connectionState()).toBe(signalR.HubConnectionState.Disconnected);
    });

    it('should connect with token', async () => {
      mockAuthService.getAccessToken.mockReturnValue('test-token');
      mockHubConnection.start.mockResolvedValue(undefined);

      await service.startConnection();

      expect(signalR.HubConnectionBuilder).toHaveBeenCalled();
      expect(mockBuilder.withUrl).toHaveBeenCalled();
      expect(mockHubConnection.start).toHaveBeenCalled();
      expect(service.connectionState()).toBe(signalR.HubConnectionState.Connected);
    });

    it('should handle connection failure', async () => {
      mockAuthService.getAccessToken.mockReturnValue('test-token');
      mockHubConnection.start.mockRejectedValue(new Error('Connection failed'));

      await service.startConnection();

      expect(service.connectionState()).toBe(signalR.HubConnectionState.Disconnected);
    });
  });

  describe('sendMessage', () => {
    it('should invoke hub method when connected', async () => {
      mockAuthService.getAccessToken.mockReturnValue('test-token');
      mockHubConnection.start.mockResolvedValue(undefined);
      await service.startConnection();

      const request = { conversationId: 1, messageType: 'Text', content: 'hello', replyToMessageId: null };
      mockHubConnection.invoke.mockResolvedValue(undefined);

      await service.sendMessage(request);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('SendMessage', request);
    });

    it('should not invoke when hubConnection is null', async () => {
      const request = { conversationId: 1, messageType: 'Text', content: 'hello', replyToMessageId: null };

      await service.sendMessage(request);

      expect(mockHubConnection.invoke).not.toHaveBeenCalled();
    });
  });
});
