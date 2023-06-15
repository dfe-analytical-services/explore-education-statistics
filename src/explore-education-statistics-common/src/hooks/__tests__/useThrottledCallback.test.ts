import useThrottledCallback from '@common/hooks/useThrottledCallback';
import { renderHook } from '@testing-library/react-hooks';

describe('useThrottledCallback', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
    jest.runOnlyPendingTimers();
  });

  test('does not run callback until specified timeout', () => {
    const callback = jest.fn();

    const { result } = renderHook(() => useThrottledCallback(callback, 10));
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
      useThrottledCallback(callback, 10),
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

  test('calling run function does not reset the timeout', () => {
    const callback = jest.fn();

    const { result } = renderHook(() => useThrottledCallback(callback, 10));
    const [run] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(5);
    expect(callback).not.toHaveBeenCalled();

    run();

    jest.advanceTimersByTime(5);
    expect(callback).toHaveBeenCalledTimes(1);

    jest.advanceTimersByTime(5);
    expect(callback).toHaveBeenCalledTimes(1);
  });

  test('calling run function repeatedly does not trigger callback more than once per timeout', () => {
    const callback = jest.fn();

    const { result } = renderHook(() => useThrottledCallback(callback, 10));
    const [run] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(5);
    expect(callback).not.toHaveBeenCalled();

    run();
    run();
    run();
    run();

    jest.advanceTimersByTime(5);
    expect(callback).toHaveBeenCalledTimes(1);
  });

  test('calling run function repeatedly and changing the callback does not reset the timeout', () => {
    const callback1 = jest.fn();

    const { result, rerender } = renderHook(
      ({ cb }) => useThrottledCallback(cb, 10),
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
    expect(callback2).toHaveBeenCalledTimes(1);

    jest.advanceTimersByTime(5);
    expect(callback1).not.toHaveBeenCalled();
    expect(callback2).toHaveBeenCalledTimes(1);
  });

  test('changing timeout prevents a pending callback from running', () => {
    const callback = jest.fn();

    const { result, rerender } = renderHook(
      ({ timeout }) => useThrottledCallback(callback, timeout),
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

  test('calling cancel function prevents the callback from running', () => {
    const callback = jest.fn();

    const { result } = renderHook(() => useThrottledCallback(callback, 10));
    const [run, cancel] = result.current;

    run();

    expect(callback).not.toHaveBeenCalled();
    jest.advanceTimersByTime(9);

    cancel();

    jest.advanceTimersByTime(20);
    expect(callback).not.toHaveBeenCalled();
  });
});
