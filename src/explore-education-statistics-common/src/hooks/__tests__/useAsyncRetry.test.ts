import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { renderHook } from '@testing-library/react-hooks';

describe('useAsyncRetry', () => {
  test('returns correct state when callback has not completed', () => {
    const { result } = renderHook(() =>
      useAsyncRetry(() => Promise.resolve('some value')),
    );

    expect(result.current.isLoading).toBe(true);
    expect(result.current.value).toBeUndefined();
    expect(result.current.error).toBeUndefined();
  });

  test('returns correct state when callback promise is resolved', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncRetry(() => Promise.resolve('some value')),
    );

    await waitForNextUpdate();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBe('some value');
    expect(result.current.error).toBeUndefined();
  });

  test('returns correct state when callback promise is rejected', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncRetry(() => Promise.reject(new Error('some error'))),
    );

    await waitForNextUpdate();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBeUndefined();
    expect(result.current.error).toEqual(new Error('some error'));
  });

  test('resets state when `retry` is called', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncRetry(() => Promise.resolve('some value')),
    );

    await waitForNextUpdate();

    result.current.retry();

    expect(result.current.isLoading).toBe(true);
    expect(result.current.value).toBeUndefined();
    expect(result.current.error).toBeUndefined();
  });

  test('returns correct state when `retry` has succeeded', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncRetry(() => Promise.resolve('some value')),
    );

    await waitForNextUpdate();

    result.current.retry();

    await waitForNextUpdate();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBe('some value');
    expect(result.current.error).toBeUndefined();
  });

  test('returns correct state when `retry` has failed', async () => {
    const callback = jest.fn(() => Promise.resolve('some value'));

    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncRetry(callback),
    );

    await waitForNextUpdate();

    callback.mockImplementation(() => Promise.reject(new Error('some error')));

    result.current.retry();

    await waitForNextUpdate();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBeUndefined();
    expect(result.current.error).toEqual(new Error('some error'));
  });

  test('resets to initial state when dependencies change', async () => {
    const { result, rerender, waitForNextUpdate } = renderHook(
      ({ deps }) => useAsyncRetry(() => Promise.resolve('some value'), deps),
      {
        initialProps: {
          deps: ['first-dep'],
        },
      },
    );

    await waitForNextUpdate();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBe('some value');
    expect(result.current.error).toBeUndefined();

    rerender({ deps: ['second-dep'] });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.value).toBeUndefined();
    expect(result.current.error).toBeUndefined();

    await waitForNextUpdate();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBe('some value');
    expect(result.current.error).toBeUndefined();
  });

  test('does not reset to initial state if dependencies do not change', async () => {
    const { result, rerender, waitForNextUpdate } = renderHook(
      ({ deps }) => useAsyncRetry(() => Promise.resolve('some value'), deps),
      {
        initialProps: {
          deps: ['first-dep'],
        },
      },
    );

    await waitForNextUpdate();

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBe('some value');
    expect(result.current.error).toBeUndefined();

    rerender({ deps: ['first-dep'] });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.value).toBe('some value');
    expect(result.current.error).toBeUndefined();
  });
});
