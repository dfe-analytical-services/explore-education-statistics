import useAsyncCallback, {
  AsyncStateSetterParam,
} from '@common/hooks/useAsyncCallback';
import { renderHook } from '@testing-library/react-hooks';

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

  test('returns correct state when callback invoked', () => {
    const { result } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    const [, run] = result.current;

    run();

    const [state] = result.current;

    expect(state.isLoading).toBe(true);
    expect(state.value).toBeUndefined();
    expect(state.error).toBeUndefined();
  });

  test('returns correct state when callback promise is resolved', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    const [, run] = result.current;

    run();

    await waitForNextUpdate();

    const [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBe('some value');
    expect(state.error).toBeUndefined();
  });

  test('returns correct state when callback promise is rejected', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncCallback(() => Promise.reject(new Error('some error'))),
    );

    const [, run] = result.current;

    run();

    await waitForNextUpdate();

    const [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBeUndefined();
    expect(state.error).toEqual(new Error('some error'));
  });

  test('can manually set `isLoading` state using setter', async () => {
    const { result, waitFor } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    state.setState({
      isLoading: false,
    });

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBeUndefined();
      expect(state.error).toBeUndefined();
    });
  });

  test('setting `isLoading` state as true always unsets `value`', async () => {
    const { result, waitFor } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    state.setState({
      isLoading: true,
      value: 'a custom value',
    } as AsyncStateSetterParam<string>);

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(true);
      expect(state.value).toBeUndefined();
      expect(state.error).toBeUndefined();
    });
  });

  test('setting `isLoading` state as true always unsets `error`', async () => {
    const { result, waitFor } = renderHook(() =>
      useAsyncCallback<string>(() =>
        Promise.reject(new Error('initial error')),
      ),
    );

    let [state] = result.current;

    state.setState({
      isLoading: true,
      error: new Error('some error'),
    } as AsyncStateSetterParam<string>);

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(true);
      expect(state.value).toBeUndefined();
      expect(state.error).toBeUndefined();
    });
  });

  test('can manually set `value` state using setter', async () => {
    const { result, waitFor } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    state.setState({
      value: 'a custom value',
    });

    await waitFor(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBe('a custom value');
      expect(state.error).toBeUndefined();
    });
  });

  test('can manually set `error` state using setter', async () => {
    const { result, waitFor } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    state.setState({
      error: new Error('some error'),
    });

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

  test('setting `keepStaleValue = true` keeps the initial value whilst running for first time', async () => {
    jest.useFakeTimers();

    const { result, waitForNextUpdate } = renderHook(() =>
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

    run();

    jest.advanceTimersByTime(400);

    [state] = result.current;

    expect(state.isLoading).toBe(true);
    expect(state.value).toBe('first value');
    expect(state.error).toBeUndefined();

    jest.advanceTimersByTime(100);

    await waitForNextUpdate();

    [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBe('second value');
    expect(state.error).toBeUndefined();

    jest.useRealTimers();
  });

  test('setting `keepStaleValue = true` keeps the last value when re-running', async () => {
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

    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncCallback(task, [], {
        keepStaleValue: true,
      }),
    );

    const [, run] = result.current;

    run();

    await waitForNextUpdate();

    let [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBe('first value');
    expect(state.error).toBeUndefined();

    run();

    [state] = result.current;

    expect(state.isLoading).toBe(true);
    expect(state.value).toBe('first value');
    expect(state.error).toBeUndefined();

    jest.advanceTimersByTime(500);

    await waitForNextUpdate();

    [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBe('second value');
    expect(state.error).toBeUndefined();

    jest.useRealTimers();
  });
});
