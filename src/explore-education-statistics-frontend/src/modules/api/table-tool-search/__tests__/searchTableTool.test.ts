/* eslint-disable no-underscore-dangle -- node-mocks-http exposes its results via `_get*` methods */
import { createApiMocks } from '@frontend-test/apiRouteMocks';
import handler from '@frontend/modules/api/table-tool-search/searchTableTool';
import { waitFor } from '@testing-library/react';
import { EventEmitter } from 'events';
import { ReadableStream as NodeReadableStream } from 'stream/web';
import { TextEncoder as NodeTextEncoder } from 'util';

jest.mock('@common/services/logger');

// jsdom doesn't provide these, but the route uses them.
Object.assign(global, {
  ReadableStream: NodeReadableStream,
  TextEncoder: NodeTextEncoder,
});

describe('searchTableTool', () => {
  const originalEnv = process.env;
  const originalFetch = global.fetch;
  const encoder = new TextEncoder();

  type FetchResponse = Partial<Omit<Response, 'body'>> & {
    body?: ReadableStream<Uint8Array> | null;
  };

  function createStream(chunks: string[]): ReadableStream<Uint8Array> {
    return new ReadableStream({
      start(controller) {
        chunks.forEach(chunk => controller.enqueue(encoder.encode(chunk)));
        controller.close();
      },
    });
  }

  /**
   * Creates a stream that only produces a chunk when one is read
   * (no read-ahead), so `pulls` shows how far the handler has read.
   * It never closes if `close` is false.
   */
  function createPullStream(chunks: string[], { close = true } = {}) {
    const state = { pulls: 0 };

    const stream = new ReadableStream<Uint8Array>(
      {
        pull(controller) {
          const chunk = chunks[state.pulls];
          state.pulls += 1;

          if (chunk !== undefined) {
            controller.enqueue(encoder.encode(chunk));
          } else if (close) {
            controller.close();
          }
        },
      },
      { highWaterMark: 0 },
    );

    return { stream, state };
  }

  function mockFetch(response: FetchResponse) {
    const fetchMock = jest.fn().mockResolvedValue(response);
    global.fetch = fetchMock;
    return fetchMock;
  }

  beforeEach(() => {
    process.env = {
      ...originalEnv,
      AZURE_TABLE_TOOL_SEARCH_ENDPOINT: 'https://example.com/search',
      USE_MOCK_TABLE_TOOL_SEARCH_API: 'false',
    };
  });

  afterEach(() => {
    process.env = originalEnv;
    global.fetch = originalFetch;
  });

  test('returns 500 when the search endpoint is not configured', async () => {
    delete process.env.AZURE_TABLE_TOOL_SEARCH_ENDPOINT;

    const { req, res } = createApiMocks({ method: 'POST', body: {} });

    await handler(req, res);

    expect(res._getStatusCode()).toBe(500);
    expect(res._getJSONData()).toEqual({
      message: 'Server configuration error: Missing endpoint',
    });
  });

  test('streams events from the search endpoint', async () => {
    const fetchMock = mockFetch({
      ok: true,
      status: 200,
      body: createStream([
        'data: {"stage":"starting pipeline"}\n\n',
        'data: {"stage":"pipeline complete"}\n\n',
      ]),
    });

    const { req, res } = createApiMocks({
      method: 'POST',
      body: { query: 'pupil absence' },
    });

    await handler(req, res);

    expect(fetchMock).toHaveBeenCalledWith(
      'https://example.com/search',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ query: 'pupil absence' }),
        headers: { 'Content-Type': 'application/json' },
      }),
    );

    expect(res._getStatusCode()).toBe(200);
    expect(res.getHeader('Content-Type')).toBe('text/event-stream');
    expect(res.getHeader('Cache-Control')).toBe('no-cache, no-transform');
    expect(res._getBuffer().toString()).toBe(
      'data: {"stage":"starting pipeline"}\n\n' +
        'data: {"stage":"pipeline complete"}\n\n',
    );
    expect(res._isEndCalled()).toBe(true);
  });

  test('returns the upstream status when the search endpoint fails', async () => {
    mockFetch({
      ok: false,
      status: 503,
      statusText: 'Service Unavailable',
      body: null,
    });

    const { req, res } = createApiMocks({ method: 'POST', body: {} });

    await handler(req, res);

    expect(res._getStatusCode()).toBe(503);
    expect(res._getJSONData()).toEqual({
      message: 'Backend error: Service Unavailable',
      status: 503,
    });
  });

  test('returns 500 when the request to the search endpoint throws', async () => {
    global.fetch = jest.fn().mockRejectedValue(new Error('Network error'));

    const { req, res } = createApiMocks({ method: 'POST', body: {} });

    await handler(req, res);

    expect(res._getStatusCode()).toBe(500);
    expect(res._getJSONData()).toEqual({
      message: 'Internal server error',
      status: 500,
    });
  });

  test('aborts the upstream request when the client disconnects', async () => {
    let upstreamSignal: AbortSignal | undefined;

    global.fetch = jest.fn().mockImplementation((_, init: RequestInit) => {
      upstreamSignal = init.signal ?? undefined;

      // A stream that never closes, like a long-running search.
      return Promise.resolve({
        ok: true,
        status: 200,
        body: new ReadableStream({
          start(controller) {
            controller.enqueue(encoder.encode('data: {}\n\n'));
          },
        }),
      });
    });

    const { req, res } = createApiMocks(
      { method: 'POST', body: {} },
      // Needed for the response to emit 'close'.
      { eventEmitter: EventEmitter },
    );

    // The mock response only exposes written data once it has ended,
    // so spy on writes to see what was streamed before disconnecting.
    const writeSpy = jest.spyOn(res, 'write');

    const handling = handler(req, res);

    // Disconnect once the first event has been streamed.
    await waitFor(() => {
      expect(writeSpy).toHaveBeenCalledTimes(1);
    });
    expect(String(writeSpy.mock.calls[0][0])).toBe('data: {}\n\n');

    res.emit('close');
    await handling;

    expect(upstreamSignal?.aborted).toBe(true);
    expect(res._isEndCalled()).toBe(false);
  });

  test('waits for a slow client to drain before reading more from the search endpoint', async () => {
    const { stream, state } = createPullStream([
      'data: {"stage":"starting pipeline"}\n\n',
      'data: {"stage":"pipeline complete"}\n\n',
    ]);

    mockFetch({ ok: true, status: 200, body: stream });

    const { req, res } = createApiMocks(
      { method: 'POST', body: {} },
      // Needed for the response to emit 'drain'.
      { eventEmitter: EventEmitter },
    );

    // Simulate a slow client: the response buffer is always full.
    const writeSpy = jest.spyOn(res, 'write').mockReturnValue(false);

    const handling = handler(req, res);

    await waitFor(() => {
      expect(writeSpy).toHaveBeenCalledTimes(1);
    });

    // Give the handler a chance to (incorrectly) keep reading.
    await new Promise(resolve => setTimeout(resolve, 50));

    expect(writeSpy).toHaveBeenCalledTimes(1);
    expect(state.pulls).toBe(1);

    res.emit('drain');

    await waitFor(() => {
      expect(writeSpy).toHaveBeenCalledTimes(2);
    });
    expect(state.pulls).toBe(2);

    res.emit('drain');
    await handling;

    expect(writeSpy.mock.calls.map(([chunk]) => String(chunk))).toEqual([
      'data: {"stage":"starting pipeline"}\n\n',
      'data: {"stage":"pipeline complete"}\n\n',
    ]);
    expect(res._isEndCalled()).toBe(true);
  });

  test('stops waiting for a slow client to drain when it disconnects', async () => {
    const { stream, state } = createPullStream(['data: {}\n\n'], {
      close: false,
    });
    let upstreamSignal: AbortSignal | undefined;

    global.fetch = jest.fn().mockImplementation((_, init: RequestInit) => {
      upstreamSignal = init.signal ?? undefined;
      return Promise.resolve({ ok: true, status: 200, body: stream });
    });

    const { req, res } = createApiMocks(
      { method: 'POST', body: {} },
      // Needed for the response to emit 'close'.
      { eventEmitter: EventEmitter },
    );

    const writeSpy = jest.spyOn(res, 'write').mockReturnValue(false);

    const handling = handler(req, res);

    await waitFor(() => {
      expect(writeSpy).toHaveBeenCalledTimes(1);
    });

    // Disconnect while waiting for the response to drain.
    res.emit('close');
    await handling;

    expect(upstreamSignal?.aborted).toBe(true);
    expect(state.pulls).toBe(1);
    expect(writeSpy).toHaveBeenCalledTimes(1);
    expect(res._isEndCalled()).toBe(false);
  });
});
