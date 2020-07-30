import useStorageItem from '@common/hooks/useStorageItem';
import _storageService from '@common/services/storageService';
import { renderHook } from '@testing-library/react-hooks';

jest.mock('@common/services/storageService');

const storageService = _storageService as jest.Mocked<typeof _storageService>;

describe('useStorageItem', () => {
  test('sets `value` to `initialValue`', () => {
    const { result } = renderHook(() => useStorageItem('testKey', 'initial'));

    const [value] = result.current;

    expect(value).toBe('initial');
  });

  test('sets value to existing storage value', async () => {
    storageService.getSync.mockReturnValue('storage value');

    const { result } = renderHook(() => useStorageItem('testKey'));

    const [value] = result.current;

    expect(value).toBe('storage value');
  });

  test('preferentially sets value to existing storage value instead of `initialValue`', async () => {
    storageService.getSync.mockReturnValue('storage value');

    const { result } = renderHook(() => useStorageItem('testKey', 'initial'));

    const [value] = result.current;

    expect(value).toBe('storage value');
  });

  test('can set new `value`', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useStorageItem('testKey', 'initial'),
    );

    const [, setValue] = result.current;

    setValue('test');

    await waitForNextUpdate();

    const [value] = result.current;

    expect(value).toBe('test');
  });

  test('can clear `value`', async () => {
    const { result, waitForNextUpdate } = renderHook(() =>
      useStorageItem('testKey', 'initial'),
    );

    const [, , clearValue] = result.current;

    clearValue();

    await waitForNextUpdate();

    const [value] = result.current;

    expect(value).toBeUndefined();
  });
});
