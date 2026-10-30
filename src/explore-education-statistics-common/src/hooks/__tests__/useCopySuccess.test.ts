import useCopySuccess from '@common/hooks/useCopySuccess';
import { act, renderHook } from '@testing-library/react';

describe('useCopySuccess', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  test('is false initially', () => {
    const { result } = renderHook(() => useCopySuccess());

    const [copySuccess] = result.current;
    expect(copySuccess).toBe(false);
  });

  test('is true after showing success, then resets after the delay', () => {
    const { result } = renderHook(() => useCopySuccess(100));

    act(() => {
      const [, showCopySuccess] = result.current;
      showCopySuccess();
    });

    expect(result.current[0]).toBe(true);

    act(() => {
      jest.advanceTimersByTime(99);
    });
    expect(result.current[0]).toBe(true);

    act(() => {
      jest.advanceTimersByTime(1);
    });
    expect(result.current[0]).toBe(false);
  });

  test('showing success again restarts the delay', () => {
    const { result } = renderHook(() => useCopySuccess(100));

    act(() => {
      result.current[1]();
    });

    act(() => {
      jest.advanceTimersByTime(90);
    });

    act(() => {
      result.current[1]();
    });

    act(() => {
      jest.advanceTimersByTime(90);
    });
    expect(result.current[0]).toBe(true);

    act(() => {
      jest.advanceTimersByTime(10);
    });
    expect(result.current[0]).toBe(false);
  });

  test('clears the reset timeout on unmount', () => {
    const { result, unmount } = renderHook(() => useCopySuccess(100));

    act(() => {
      result.current[1]();
    });

    unmount();

    expect(jest.getTimerCount()).toBe(0);
  });
});
