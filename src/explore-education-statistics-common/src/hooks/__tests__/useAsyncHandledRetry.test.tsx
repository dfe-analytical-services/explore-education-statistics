import { ErrorControlContextProvider } from '@common/contexts/ErrorControlContext';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { renderHook } from '@testing-library/react-hooks';
import noop from 'lodash/noop';
import React, { FunctionComponent } from 'react';

describe('useAsyncHandledRetry', () => {
  test('calls `handleApiErrors` if callback promise is rejected', async () => {
    const handleApiErrors = jest.fn();

    const wrapper: FunctionComponent = ({ children }) => (
      <ErrorControlContextProvider
        value={{
          handleApiErrors,
          handleManualErrors: {
            forbidden: noop,
          },
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );

    const { result, waitForNextUpdate } = renderHook(
      () => useAsyncHandledRetry(() => Promise.reject(new Error('some error'))),
      { wrapper },
    );

    await waitForNextUpdate();

    expect(handleApiErrors).toHaveBeenCalled();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBeUndefined();
  });

  test('does not call `handleApiErrors` if callback promise is resolved', async () => {
    const handleApiErrors = jest.fn();

    const wrapper: FunctionComponent = ({ children }) => (
      <ErrorControlContextProvider
        value={{
          handleApiErrors,
          handleManualErrors: {
            forbidden: noop,
          },
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );

    const { result, waitForNextUpdate } = renderHook(
      () => useAsyncHandledRetry(() => Promise.resolve('some value')),
      { wrapper },
    );

    await waitForNextUpdate();

    expect(handleApiErrors).not.toHaveBeenCalled();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBe('some value');
  });

  test('cannot set error via initial state', async () => {
    const wrapper: FunctionComponent = ({ children }) => (
      <ErrorControlContextProvider
        value={{
          handleApiErrors: noop,
          handleManualErrors: {
            forbidden: noop,
          },
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );

    const { result, waitForValueToChange } = renderHook(
      () =>
        useAsyncHandledRetry(() => Promise.resolve(), [], {
          error: new Error('Test error'),
        } as never),
      { wrapper },
    );

    await waitForValueToChange(() => result.current);

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBeUndefined();
    expect((result.current as { error?: Error }).error).not.toBeDefined();
  });

  test('cannot set error via setter', async () => {
    const wrapper: FunctionComponent = ({ children }) => (
      <ErrorControlContextProvider
        value={{
          handleApiErrors: noop,
          handleManualErrors: {
            forbidden: noop,
          },
        }}
      >
        {children}
      </ErrorControlContextProvider>
    );

    const { result, waitForNextUpdate } = renderHook(
      () => useAsyncHandledRetry(() => Promise.resolve()),
      { wrapper },
    );

    result.current.setState({ error: new Error('test') } as never);

    await waitForNextUpdate();

    expect((result.current as { error?: Error }).error).not.toBeDefined();
  });
});
