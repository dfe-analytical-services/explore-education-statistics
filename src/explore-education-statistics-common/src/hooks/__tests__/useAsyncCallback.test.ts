import useAsyncCallback from '@common/hooks/useAsyncCallback';
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

    const [, callback] = result.current;

    callback();

    const [state] = result.current;

    expect(state.isLoading).toBe(true);
    expect(state.value).toBeUndefined();
    expect(state.error).toBeUndefined();
  });

  test('returns correct state when callback promise is resolved', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    const [, callback] = result.current;

    callback();

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

    const [, callback] = result.current;

    callback();

    await waitForNextUpdate();

    const [state] = result.current;

    expect(state.isLoading).toBe(false);
    expect(state.value).toBeUndefined();
    expect(state.error).toEqual(new Error('some error'));
  });

  test('can manually set loading state using setter', async () => {
    const { result, wait } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    state.setState({
      isLoading: true,
    });

    await wait(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(true);
      expect(state.value).toBeUndefined();
      expect(state.error).toBeUndefined();
    });
  });

  test('can manually set value state using setter', async () => {
    const { result, wait } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    state.setState({
      isLoading: false,
      value: 'a custom value',
    });

    await wait(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBe('a custom value');
      expect(state.error).toBeUndefined();
    });
  });

  test('can manually set error state using setter', async () => {
    const { result, wait } = renderHook(() =>
      useAsyncCallback(() => Promise.resolve('some value')),
    );

    let [state] = result.current;

    state.setState({
      isLoading: false,
      error: new Error('some error'),
    });

    await wait(() => {
      [state] = result.current;

      expect(state.isLoading).toBe(false);
      expect(state.value).toBeUndefined();
      expect(state.error).toEqual(new Error('some error'));
    });
  });
});
