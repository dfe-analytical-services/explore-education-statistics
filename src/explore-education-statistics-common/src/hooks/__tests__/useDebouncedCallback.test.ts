import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { renderHook } from '@testing-library/react-hooks';

describe('useDebouncedCallback', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  test('does not run callback until specified timeout', () => {
    const callback = jest.fn();

    const { result } = renderHook(() => useDebouncedCallback(callback, 10));
    const [run] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(9);
    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(1);
    expect(callback).toHaveBeenCalled();
  });

  test('does not run callback if unmounted', () => {
    const callback = jest.fn();

    const { result, unmount } = renderHook(() =>
      useDebouncedCallback(callback, 10),
    );
    const [run] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(9);
    expect(callback).not.toHaveBeenCalled();

    unmount();

    jest.advanceTimersByTime(1);
    expect(callback).not.toHaveBeenCalled();
  });

  test('calling run function repeatedly resets the timeout', () => {
    const callback = jest.fn();

    const { result } = renderHook(() => useDebouncedCallback(callback, 10));
    const [run] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(5);
    expect(callback).not.toHaveBeenCalled();

    run();

    jest.advanceTimersByTime(5);
    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(5);
    expect(callback).toHaveBeenCalled();
  });

  test('calling run function repeatedly and changing the callback resets the timeout', () => {
    const callback1 = jest.fn();

    const { result, rerender } = renderHook(
      ({ cb }) => useDebouncedCallback(cb, 10),
      {
        initialProps: {
          cb: callback1,
        },
      },
    );
    const [run] = result.current;

    run();

    expect(callback1).not.toHaveBeenCalled();

    jest.advanceTimersByTime(5);
    expect(callback1).not.toHaveBeenCalled();

    const callback2 = jest.fn();

    rerender({ cb: callback2 });

    run();

    jest.advanceTimersByTime(5);
    expect(callback1).not.toHaveBeenCalled();
    expect(callback2).not.toHaveBeenCalled();

    jest.advanceTimersByTime(5);
    expect(callback1).not.toHaveBeenCalled();
    expect(callback2).toHaveBeenCalled();
  });

  test('changing timeout prevents a pending callback from running', () => {
    const callback = jest.fn();

    const { result, rerender } = renderHook(
      ({ timeout }) => useDebouncedCallback(callback, timeout),
      {
        initialProps: {
          timeout: 10,
        },
      },
    );

    const [run] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(5);
    expect(callback).not.toHaveBeenCalled();

    rerender({ timeout: 20 });

    jest.advanceTimersByTime(10);
    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(10);
    expect(callback).not.toHaveBeenCalled();
  });

  test('awaiting run function waits until wrapped function and debounce complete', () => {
    let resolved = false;

    const callback = jest.fn(() => {
      return new Promise<void>(resolve =>
        setTimeout(() => {
          resolve();
          resolved = true;
        }, 50),
      );
    });

    const { result } = renderHook(({ cb }) => useDebouncedCallback(cb, 10), {
      initialProps: {
        cb: callback,
      },
    });

    const [run] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(10);
    expect(callback).toHaveBeenCalled();

    expect(resolved).toBe(false);

    jest.advanceTimersByTime(50);

    expect(resolved).toBe(true);
  });

  test('calling cancel function prevents the callback from running', () => {
    const callback = jest.fn();

    const { result } = renderHook(() => useDebouncedCallback(callback, 10));
    const [run, cancel] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();
    jest.advanceTimersByTime(9);

    cancel();

    jest.advanceTimersByTime(20);
    expect(callback).not.toHaveBeenCalled();
  });
});
