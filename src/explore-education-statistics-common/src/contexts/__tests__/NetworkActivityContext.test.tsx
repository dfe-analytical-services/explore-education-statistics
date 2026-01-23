import {
  NetworkActivityContextProvider,
  networkActivityRequestInterceptor,
  networkActivityResponseInterceptor,
  NetworkActivityState,
  useNetworkActivityContext,
} from '@common/contexts/NetworkActivityContext';
import Client from '@common/services/api/Client';
import delay from '@common/utils/delay';
import { act, renderHook, waitFor } from '@testing-library/react';
import { AxiosError } from 'axios';
import React, { FC, ReactNode } from 'react';
import xhrMock from 'xhr-mock';

describe('useNetworkActivityContext', () => {
  interface Props {
    children: ReactNode;
  }
  const wrapper: FC<Props> = ({ children }) => (
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

    const { result } = renderHook(() => useNetworkActivityContext(), {
      wrapper,
    });

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');

    await act(async () => {
      client.get('/test');
    });

    await waitFor(() => {
      expect(result.current).toEqual<NetworkActivityState>({
        status: 'active',
        requestCount: 1,
      });
      expect(document.body).toHaveAttribute('data-network-activity', 'active');
    });
  });

  // EES-4936 This test previously used `waitForNextUpdate` to check for the state change,
  // this has been removed from the new version of renderHook
  test.skip('toggles status back to `idle` when no more requests within the idle timeout', async () => {
    const client = createTestClient();

    const { result } = renderHook(() => useNetworkActivityContext(), {
      wrapper,
    });

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

    await waitFor(() => {
      expect(result.current).toEqual<NetworkActivityState>({
        status: 'idle',
        requestCount: 0,
      });
      expect(document.body).toHaveAttribute('data-network-activity', 'idle');
    });
  });

  test('updates `requestCount` correctly when multiple concurrent requests', async () => {
    const client = createTestClient();

    xhrMock.get('/test', async (_, res) => {
      await delay(150);
      return res.status(200);
    });
    xhrMock.get('/test-slow', async (_, res) => {
      await delay(300);
      return res.status(200);
    });
    xhrMock.get('/test-slower', async (_, res) => {
      await delay(450);
      return res.status(200);
    });

    const { result } = renderHook(() => useNetworkActivityContext(), {
      wrapper,
    });

    expect(result.current).toEqual<NetworkActivityState>({
      status: 'idle',
      requestCount: 0,
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');

    let request: Promise<unknown>;

    await act(async () => {
      request = Promise.all([
        client.get('/test'),
        client.get('/test-slow'),
        client.get('/test-slower'),
      ]);
    });

    await waitFor(() => {
      expect(result.current).toEqual<NetworkActivityState>({
        status: 'active',
        requestCount: 3,
      });
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'active');

    await waitFor(() => {
      expect(result.current).toEqual<NetworkActivityState>({
        status: 'active',
        requestCount: 2,
      });
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'active');

    await waitFor(() => {
      expect(result.current).toEqual<NetworkActivityState>({
        status: 'active',
        requestCount: 1,
      });
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'active');

    await act(async () => {
      await request;
    });

    await waitFor(() => {
      expect(result.current).toEqual<NetworkActivityState>({
        status: 'idle',
        requestCount: 0,
      });
    });
    expect(document.body).toHaveAttribute('data-network-activity', 'idle');
  });

  test('updates `requestCount` correctly when error response', async () => {
    const client = createTestClient();

    const { result } = renderHook(() => useNetworkActivityContext(), {
      wrapper,
    });

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

    let error: AxiosError | undefined;

    await act(async () => {
      try {
        await request;

        await waitFor(() => {
          expect(result.current).toEqual<NetworkActivityState>({
            status: 'active',
            requestCount: 1,
          });
          expect(document.body).toHaveAttribute(
            'data-network-activity',
            'active',
          );
        });
      } catch (err) {
        error = err as AxiosError;
      }
    });

    expect(error).toBeInstanceOf(Error);
    expect(error?.response?.status).toBe(500);
    expect(error?.response?.data).toEqual({
      message: 'Something went wrong',
    });

    await waitFor(() => {
      expect(result.current).toEqual<NetworkActivityState>({
        status: 'idle',
        requestCount: 0,
      });
      expect(document.body).toHaveAttribute('data-network-activity', 'idle');
    });
  });
});

function createTestClient(): Client {
  return new Client({
    requestInterceptors: [networkActivityRequestInterceptor],
    responseInterceptors: [networkActivityResponseInterceptor],
  });
}
