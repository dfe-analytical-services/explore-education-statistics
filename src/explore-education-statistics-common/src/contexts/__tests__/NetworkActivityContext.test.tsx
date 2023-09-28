import {
  NetworkActivityContextProvider,
  networkActivityRequestInterceptor,
  networkActivityResponseInterceptor,
  NetworkActivityState,
  useNetworkActivityContext,
} from '@common/contexts/NetworkActivityContext';
import Client from '@common/services/api/Client';
import { renderHook } from '@testing-library/react-hooks';
import { AxiosError } from 'axios';
import React, { FC } from 'react';
import xhrMock from 'xhr-mock';

describe('useNetworkActivityContext', () => {
  const wrapper: FC = ({ children }) => (
    <NetworkActivityContextProvider>{children}</NetworkActivityContextProvider>
  );

  beforeEach(() => {
    xhrMock.setup();
    xhrMock.get('/test', (_, res) => {
      return res.status(200);
    });
  });

  afterEach(() => {
    xhrMock.teardown();
  });

  test('toggles status to `active` when request starts', async () => {
    const client = createTestClient();

    const { result, waitForNextUpdate } = renderHook(
      () => useNetworkActivityContext(),
      { wrapper },
    );

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');

    const request = client.get('/test');

    await waitForNextUpdate();

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'active',
      requestCount: 1,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'active');

    await request;
  });

  test('toggles status back to `idle` when no more requests within the idle timeout', async () => {
    const client = createTestClient();

    const { result, waitForNextUpdate } = renderHook(
      () => useNetworkActivityContext(),
      { wrapper },
    );

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');

    await client.get('/test');

    // Request has completed, but status does not switch to `idle` until timeout expires
    expect(result.current).toEqual<NetworkActivityState>({
      status: 'active',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'active');

    await waitForNextUpdate();

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');
  });

  test('updates `requestCount` correctly when multiple concurrent requests', async () => {
    const client = createTestClient();

    const { result, waitForNextUpdate } = renderHook(
      () => useNetworkActivityContext(),
      { wrapper },
    );

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');

    const request = Promise.all([
      client.get('/test'),
      client.get('/test'),
      client.get('/test'),
    ]);

    await waitForNextUpdate();

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'active',
      requestCount: 3,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'active');

    await request;
    await waitForNextUpdate();

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');
  });

  test('updates `requestCount` correctly when error response', async () => {
    const client = createTestClient();

    const { result, waitForNextUpdate } = renderHook(
      () => useNetworkActivityContext(),
      { wrapper },
    );

    xhrMock.get('/test-error', (req, res) => {
      return res.status(500).body({
        message: 'Something went wrong',
      });
    });

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');

    const request = client.get('/test-error');
    await waitForNextUpdate();

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'active',
      requestCount: 1,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'active');

    try {
      await request;
    } catch (err) {
      const error = err as AxiosError;

      expect(error).toBeInstanceOf(Error);
      expect(error.response?.status).toBe(500);
      expect(error.response?.data).toEqual({
        message: 'Something went wrong',
      });
    }

    await waitForNextUpdate();

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');
  });
});

function createTestClient(): Client {
  return new Client({
    requestInterceptors: [networkActivityRequestInterceptor],
    responseInterceptors: [networkActivityResponseInterceptor],
  });
}
