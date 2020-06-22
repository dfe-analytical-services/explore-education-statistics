import usePrevious from '@common/hooks/usePrevious';
import { renderHook } from '@testing-library/react-hooks';

describe('usePrevious', () => {
  test('returns undefined for initial render', () => {
    const { result } = renderHook(() => usePrevious('first'));

    expect(result.current).toBeUndefined();
  });

  test('returns previous value when updated with new value', async () => {
    const { result, rerender } = renderHook(({ value }) => usePrevious(value), {
      initialProps: {
        value: 'first',
      },
    });

    rerender({
      value: 'second',
    });

    expect(result.current).toBe('first');
  });
});
