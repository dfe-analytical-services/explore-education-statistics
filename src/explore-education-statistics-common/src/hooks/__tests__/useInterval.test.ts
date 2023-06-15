import useInterval from '@common/hooks/useInterval';
import { renderHook } from '@testing-library/react-hooks';

describe('useInterval', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
    jest.runOnlyPendingTimers();
  });

  test('calls callback at regular intervals', () => {
    const callback = jest.fn();

    renderHook(() => useInterval(callback, 100));

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(100);
    expect(callback).toHaveBeenCalledTimes(1);

    jest.advanceTimersByTime(100);
    expect(callback).toHaveBeenCalledTimes(2);

    jest.advanceTimersByTime(100);
    expect(callback).toHaveBeenCalledTimes(3);
  });

  test('callback parameter can be changed to another callback', () => {
    const initialCallback = jest.fn();
    const updatedCallback = jest.fn();

    const { rerender } = renderHook(
      ({ callback }) => useInterval(callback, 100),
      {
        initialProps: {
          callback: initialCallback,
        },
      },
    );

    jest.advanceTimersByTime(100);
    expect(initialCallback).toHaveBeenCalledTimes(1);
    expect(updatedCallback).toHaveBeenCalledTimes(0);

    rerender({
      callback: updatedCallback,
    });

    expect(initialCallback).toHaveBeenCalledTimes(1);
    expect(updatedCallback).toHaveBeenCalledTimes(0);

    jest.advanceTimersByTime(100);
    expect(initialCallback).toHaveBeenCalledTimes(1);
    expect(updatedCallback).toHaveBeenCalledTimes(1);
  });

  test('delay parameter can be changed', () => {
    const callback = jest.fn();

    const { rerender } = renderHook(
      ({ delay }) => useInterval(callback, delay),
      {
        initialProps: {
          delay: 100,
        },
      },
    );

    jest.advanceTimersByTime(100);
    expect(callback).toHaveBeenCalledTimes(1);

    rerender({
      delay: 200,
    });

    jest.advanceTimersByTime(199);
    expect(callback).toHaveBeenCalledTimes(1);

    jest.advanceTimersByTime(1);
    expect(callback).toHaveBeenCalledTimes(2);
  });

  test('callback is no longer called when interval is cancelled', () => {
    const callback = jest.fn();

    const { result } = renderHook(() => useInterval(callback, 100));

    jest.advanceTimersByTime(100);
    expect(callback).toHaveBeenCalledTimes(1);

    const [cancel] = result.current;

    cancel();

    jest.advanceTimersByTime(100);
    expect(callback).toHaveBeenCalledTimes(1);
  });

  test('callback is no longer called when unmounted', () => {
    const callback = jest.fn();

    const { unmount } = renderHook(() => useInterval(callback, 100));

    expect(callback).not.toHaveBeenCalled();

    jest.advanceTimersByTime(100);
    expect(callback).toHaveBeenCalledTimes(1);

    unmount();

    jest.advanceTimersByTime(100);
    expect(callback).toHaveBeenCalledTimes(1);
  });
});
