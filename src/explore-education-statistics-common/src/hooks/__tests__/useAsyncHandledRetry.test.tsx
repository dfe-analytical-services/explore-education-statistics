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
});
