import useAsyncCallback, {
  AsyncStateSetterParam,
} from '@common/hooks/useAsyncCallback';
import { act, renderHook, waitFor } from '@testing-library/react';

describe('useAsyncCallback', () => {
  test('returns correct state when callback not invoked', () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    const [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBeUndefined();
    expect(state.error).toBeUndefined();
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('returns correct state when callback invoked', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    const [state, run] = result.current;

    act(() => run());

    expect(state.isLoading).toBe(true);
    expect(state.value).toBeUndefined();
    expect(state.error).toBeUndefined();
  });

  test('returns correct state when callback promise is resolved', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    const [, run] = result.current;

    await act(() => run());

    await waitFor(() => {
      const [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBe('some value');
      expect(state.error).toBeUndefined();
    });
  });

  test('returns correct state when callback promise is rejected', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.reject(new Error('some error'))),
    );

    const [, run] = result.current;

    await act(() => run());

    await waitFor(() => {
      const [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBeUndefined();
      expect(state.error).toEqual(new Error('some error'));
    });
  });

  test('can manually set `isLoading` state using setter', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    await act(() =>
      state.setState({
        isLoading: false,
      }),
    );

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBeUndefined();
      expect(state.error).toBeUndefined();
    });
  });

  test('setting `isLoading` state as true always unsets `value`', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    await act(() =>
      state.setState({
        isLoading: true,
        value: 'a custom value',
      } as AsyncStateSetterParam<string>),
    );

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(true);
      expect(state.value).toBeUndefined();
      expect(state.error).toBeUndefined();
    });
  });

  test('setting `isLoading` state as true always unsets `error`', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback<string>(() =>
        Promise.reject(new Error('initial error')),
      ),
    );

    let [state] = result.current;

    await act(() =>
      state.setState({
        isLoading: true,
        error: new Error('some error'),
      } as AsyncStateSetterParam<string>),
    );

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(true);
      expect(state.value).toBeUndefined();
      expect(state.error).toBeUndefined();
    });
  });

  test('can manually set `value` state using setter', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    await act(() =>
      state.setState({
        value: 'a custom value',
      }),
    );

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBe('a custom value');
      expect(state.error).toBeUndefined();
    });
  });

  test('can manually set `error` state using setter', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    await act(() =>
      state.setState({
        error: new Error('some error'),
      }),
    );

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBeUndefined();
      expect(state.error).toEqual(new Error('some error'));
    });
  });

  test('setting `initialState` sets the initial state', async () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value'), [], {
        initialState: {
          isLoading: false,
          value: 'initial value',
        },
      }),
    );

    const [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBe('initial value');
    expect(state.error).toBeUndefined();
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('setting `keepStaleValue = true` keeps the initial value whilst running for first time', async () => {
    jest.useFakeTimers();

    const { result } = renderHook(() =>
      useAsyncCallback(
        () =>
          new Promise(resolve =>
            setTimeout(() => resolve('second value'), 500),
          ),
        [],
        {
          keepStaleValue: true,
          initialState: {
            isLoading: false,
            value: 'first value',
          },
        },
      ),
    );

    const [, run] = result.current;
    let [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBe('first value');
    expect(state.error).toBeUndefined();

    await act(() => run());

    jest.advanceTimersByTime(400);

    [state] = result.current;

    expect(state.isLoading).toBe(true);
    expect(state.value).toBe('first value');
    expect(state.error).toBeUndefined();

    jest.advanceTimersByTime(100);

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBe('second value');
      expect(state.error).toBeUndefined();
    });

    jest.useRealTimers();
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('setting `keepStaleValue = true` keeps the last value when re-running', async () => {
    jest.useFakeTimers();

    const task = jest.fn();

    task
      .mockImplementationOnce(() => Promise.resolve('first value'))
      .mockImplementationOnce(
        () =>
          new Promise(resolve =>
            setTimeout(() => resolve('second value'), 500),
          ),
      );

    const { result } = renderHook(() =>
      useAsyncCallback(task, [], {
        keepStaleValue: true,
      }),
    );

    const [, run] = result.current;

    await act(() => run());

    let state;

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBe('first value');
      expect(state.error).toBeUndefined();
    });

    await act(() => run());

    [state] = result.current;

    expect(state.isLoading).toBe(true);
    expect(state.value).toBe('first value');
    expect(state.error).toBeUndefined();

    jest.advanceTimersByTime(500);

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBe('second value');
      expect(state.error).toBeUndefined();
    });

    jest.useRealTimers();
  });
});
