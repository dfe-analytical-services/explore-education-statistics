import useButton from '@common/hooks/useButton';
import { renderHook } from '@testing-library/react-hooks';

describe('useButton', () => {
  test('returns button props', () => {
    const { result } = renderHook(() =>
      useButton({
        ariaControls: 'test controls',
        ariaExpanded: true,
        children: 'button text',
        className: 'test classname',
        id: 'id',
        testId: 'test id',
        type: 'submit',
        underline: false,
        variant: 'warning',
      }),
    );
    expect(result.current.children).toBe('button text');
    expect(result.current['aria-controls']).toBe('test controls');
    expect(result.current['aria-expanded']).toBe(true);
    expect(result.current.className).toBe('test classname');
    expect(result.current.id).toBe('id');
    expect(result.current['data-testid']).toBe('test id');
    expect(result.current.type).toBe('submit');
    expect(result.current.underline).toBe(false);
    expect(result.current.variant).toBe('warning');
  });

  test('returns correct props when `disabled = true`', () => {
    const { result } = renderHook(() =>
      useButton({
        children: 'button text',
        disabled: true,
      }),
    );
    expect(result.current['aria-disabled']).toBe(true);
    expect(result.current.disabled).toBe(true);
  });

  test('returns correct props when `ariaDisabled = true`', () => {
    const { result } = renderHook(() =>
      useButton({
        children: 'button text',
        ariaDisabled: true,
      }),
    );
    expect(result.current['aria-disabled']).toBe(true);
    expect(result.current.disabled).toBe(undefined);
  });

  test('returns correct props when using both `disabled` and `ariaDisabled`', () => {
    const { result } = renderHook(() =>
      useButton({
        children: 'button text',
        ariaDisabled: true,
        disabled: true,
      }),
    );
    expect(result.current['aria-disabled']).toBe(true);
    expect(result.current.disabled).toBe(undefined);
  });
});
