import { ErrorControlContextProvider } from '@common/contexts/ErrorControlContext';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { act, renderHook, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React, { FC, ReactNode } from 'react';

describe('useAsyncHandledRetry', () => {
  interface Props {
    children: ReactNode;
  }
  test('calls `handleError` if callback promise is rejected', async () => {
    const handleError = jest.fn();

    const wrapper: FC<Props> = ({ children }) => (
      <ErrorControlContextProvider
        value={{
          handleError,
          errorPages: {
            forbidden: noop,
          },
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );

    const { result } = renderHook(
      () => useAsyncHandledRetry(() => Promise.reject(new Error('some error'))),
      { wrapper },
    );

    await waitFor(() => {
      expect(handleError).toHaveBeenCalled();
    });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBeUndefined();
  });

  test('does not call `handleError` if callback promise is resolved', async () => {
    const handleError = jest.fn();

    const wrapper: FC<Props> = ({ children }) => (
      <ErrorControlContextProvider
        value={{
          handleError,
          errorPages: {
            forbidden: noop,
          },
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );

    const { result } = renderHook(
      () => useAsyncHandledRetry(() => Promise.resolve('some value')),
      { wrapper },
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(handleError).not.toHaveBeenCalled();
    expect(result.current.value).toBe('some value');
  });

  test('cannot set error via initial state', async () => {
    const wrapper: FC<Props> = ({ children }) => (
      <ErrorControlContextProvider
        value={{
          handleError: noop,
          errorPages: {
            forbidden: noop,
          },
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );

    const { result } = renderHook(
      () =>
        useAsyncHandledRetry(() => Promise.resolve(), [], {
          error: new Error('Test error'),
        } as never),
      { wrapper },
    );

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });
    expect(result.current.value).toBeUndefined();
    expect((result.current as { error?: Error }).error).not.toBeDefined();
  });

  test('cannot set error via setter', async () => {
    const wrapper: FC<Props> = ({ children }) => (
      <ErrorControlContextProvider
        value={{
          handleError: noop,
          errorPages: {
            forbidden: noop,
          },
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );

    const { result } = renderHook(
      () => useAsyncHandledRetry(() => Promise.resolve()),
      { wrapper },
    );

    await act(async () => {
      await result.current.setState({ error: new Error('test') } as never);
    });

    await waitFor(() => {
      expect((result.current as { error?: Error }).error).not.toBeDefined();
    });
  });
});
