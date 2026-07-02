import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { AiSignalrService } from './ai-signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { SessionService } from './session.service';
import * as signalR from '@microsoft/signalr';

vi.mock('@microsoft/signalr', () => {
  const HubConnectionState = { Disconnected: 0, Connected: 1, Reconnecting: 2 };
  return {
    HubConnectionBuilder: vi.fn(),
    HubConnectionState,
    HttpTransportType: { WebSockets: 1, ServerSentEvents: 2, LongPolling: 4 },
  };
});

describe('AiSignalrService', () => {
  let service: AiSignalrService;
  let mockAuthService: { getAccessToken: ReturnType<typeof vi.fn> };
  let mockSessionService: { getSessionId: ReturnType<typeof vi.fn> };
  let mockHubConnection: {
    start: ReturnType<typeof vi.fn>;
    stop: ReturnType<typeof vi.fn>;
    invoke: ReturnType<typeof vi.fn>;
    on: ReturnType<typeof vi.fn>;
    off: ReturnType<typeof vi.fn>;
    state: signalR.HubConnectionState;
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
    mockSessionService = { getSessionId: vi.fn().mockReturnValue('sess-test') };

    mockHubConnection = {
      start: vi.fn().mockImplementation(async function (this: any) {
        mockHubConnection.state = signalR.HubConnectionState.Connected;
      }),
      stop: vi.fn().mockResolvedValue(undefined),
      invoke: vi.fn().mockResolvedValue(undefined),
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

    (signalR.HubConnectionBuilder as unknown as ReturnType<typeof vi.fn>).mockImplementation(function () {
      return mockBuilder;
    });

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AiSignalrService,
        { provide: AuthService, useValue: mockAuthService },
        { provide: SessionService, useValue: mockSessionService },
      ],
    });

    service = TestBed.inject(AiSignalrService);
  });

  afterEach(async () => {
    await service.stopConnection();
  });

  it('startConnection should connect with token when authenticated', async () => {
    mockAuthService.getAccessToken.mockReturnValue('jwt-token-123');
    await service.startConnection();

    expect(mockBuilder.withUrl).toHaveBeenCalledWith(
      expect.stringContaining('/hubs/ai'),
      expect.objectContaining({
        accessTokenFactory: expect.any(Function),
      }),
    );
    // Verify the token factory returns the token
    const urlCall = mockBuilder.withUrl.mock.calls[0];
    const accessTokenFactory = urlCall[1].accessTokenFactory;
    expect(accessTokenFactory()).toBe('jwt-token-123');
  });

  it('startConnection should include sessionId in URL', async () => {
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();

    expect(mockBuilder.withUrl).toHaveBeenCalledWith(
      expect.stringContaining('sessionId=sess-test'),
      expect.any(Object),
    );
  });

  it('startConnection should connect even without token (guest)', async () => {
    mockAuthService.getAccessToken.mockReturnValue(null);
    await service.startConnection();

    expect(mockBuilder.build).toHaveBeenCalled();
    expect(mockHubConnection.start).toHaveBeenCalled();
  });

  it('sendMessage should invoke hub with sessionId', async () => {
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();

    await service.sendMessage(1, 'Hello');
    expect(mockHubConnection.invoke).toHaveBeenCalledWith('SendMessage', 1, 'Hello', 'sess-test');
  });

  it('startConversation should invoke hub with sessionId', async () => {
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();

    await service.startConversation('Title', 'First msg');
    expect(mockHubConnection.invoke).toHaveBeenCalledWith(
      'StartConversation', 'Title', 'First msg', 'sess-test',
    );
  });

  it('onChunk$ should emit when AiResponseChunk received', async () => {
    const chunks: unknown[] = [];
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();
    service.onChunk$.subscribe((c) => chunks.push(c));

    const onHandler = mockHubConnection.on.mock.calls.find(
      (c: unknown[]) => c[0] === 'AiResponseChunk',
    );
    expect(onHandler).toBeDefined();

    const chunk = { conversationId: 1, content: 'Hello', isComplete: false };
    onHandler![1](chunk);
    expect(chunks).toContainEqual(chunk);
  });

  it('onError$ should emit when AiResponseError received', async () => {
    const errors: string[] = [];
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();
    service.onError$.subscribe((e) => errors.push(e));

    const onHandler = mockHubConnection.on.mock.calls.find(
      (c: unknown[]) => c[0] === 'AiResponseError',
    );
    onHandler![1]('Something went wrong');
    expect(errors).toContain('Something went wrong');
  });

  it('onConversationCreated$ should emit when AiConversationCreated received', async () => {
    const convs: unknown[] = [];
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();
    service.onConversationCreated$.subscribe((c) => convs.push(c));

    const onHandler = mockHubConnection.on.mock.calls.find(
      (c: unknown[]) => c[0] === 'AiConversationCreated',
    );
    const conv = { id: 1, title: 'Test' };
    onHandler![1](conv);
    expect(convs).toContainEqual(conv);
  });

  it('deleteConversation should invoke hub', async () => {
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();

    await service.deleteConversation(5);
    expect(mockHubConnection.invoke).toHaveBeenCalledWith('DeleteConversation', 5);
  });

  it('stopConnection should disconnect', async () => {
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();

    await service.stopConnection();
    expect(mockHubConnection.stop).toHaveBeenCalled();
  });

  it('connectionState should update on reconnecting', async () => {
    mockAuthService.getAccessToken.mockReturnValue('jwt');
    await service.startConnection();

    const onHandler = mockHubConnection.onreconnecting.mock.calls[0];
    expect(onHandler).toBeDefined();

    onHandler![0]();
    expect(service.connectionState()).toBe(signalR.HubConnectionState.Reconnecting);
  });
});
