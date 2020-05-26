import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { renderHook } from '@testing-library/react-hooks';

describe('useDebounceCallback', () => {
  beforeEach(() => {
    jest.useFakeTimers();
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

  test('calling run function repeatedly with different callbacks resets the timeout', () => {
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
