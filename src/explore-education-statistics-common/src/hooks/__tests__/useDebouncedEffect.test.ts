import useDebouncedEffect from '@common/hooks/useDebouncedEffect';
import { renderHook } from '@testing-library/react-hooks';

describe('useDebouncedEffect', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  test('calls effect after specified timeout', () => {
    const effect = jest.fn();

    renderHook(() => useDebouncedEffect(effect, 10));

    expect(effect).not.toHaveBeenCalled();
    jest.advanceTimersByTime(9);

    expect(effect).not.toHaveBeenCalled();
    jest.advanceTimersByTime(1);

    expect(effect).toHaveBeenCalled();
  });

  test('resets timeout if dependencies change', () => {
    const effect = jest.fn();

    const { rerender } = renderHook(
      ({ testDep }) => useDebouncedEffect(effect, 10, [testDep]),
      {
        initialProps: {
          testDep: 'a',
        },
      },
    );

    expect(effect).not.toHaveBeenCalled();

    jest.advanceTimersByTime(9);

    rerender({
      testDep: 'b',
    });

    jest.advanceTimersByTime(1);
    expect(effect).not.toHaveBeenCalled();

    jest.advanceTimersByTime(8);
    expect(effect).not.toHaveBeenCalled();

    jest.advanceTimersByTime(1);
    expect(effect).toHaveBeenCalled();
  });

  test('resets timeout if dependencies change repeatedly', () => {
    const effect = jest.fn();

    const { rerender } = renderHook(
      ({ testDep }) => useDebouncedEffect(effect, 10, [testDep]),
      {
        initialProps: {
          testDep: 'a',
        },
      },
    );

    expect(effect).not.toHaveBeenCalled();

    jest.advanceTimersByTime(9);

    rerender({
      testDep: 'b',
    });

    jest.advanceTimersByTime(9);
    expect(effect).not.toHaveBeenCalled();

    rerender({
      testDep: 'c',
    });

    jest.advanceTimersByTime(9);
    expect(effect).not.toHaveBeenCalled();

    jest.advanceTimersByTime(1);
    expect(effect).toHaveBeenCalled();
  });
});
