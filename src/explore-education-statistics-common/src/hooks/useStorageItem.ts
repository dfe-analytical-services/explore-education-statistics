import storageService, {
  StorageSetterOptions,
} from '@common/services/storageService';
import { useCallback, useState } from 'react';

export type SetStorageItem<T> = (value: T) => void;
export type RemoveStorageItem = () => void;

export type StorageState<T> = [
  T | undefined,
  SetStorageItem<T>,
  RemoveStorageItem,
];

/**
 * Hook to simultaneously store an item in storage
 * with {@see storageService} and as state.
 *
 * Using the returned setter will re-render the UI as
 * you would expect from a {@see useState} hook.
 *
 * Be aware that also like {@see useState}, you cannot change
 * any of the hook parameters after the initial render.
 */
export default function useStorageItem<T>(
  key: string,
  initialValue?: T,
  options?: StorageSetterOptions,
): StorageState<T> {
  const [value, setValue] = useState<T | undefined>(() => {
    const storageValue = storageService.getSync<T>(key);

    if (storageValue) {
      return storageValue;
    }

    if (initialValue) {
      storageService.setSync(key, initialValue, options);
      return initialValue;
    }

    return undefined;
  });

  const set = useCallback(
    async (nextValue: T) => {
      await storageService.set(key, nextValue, options);
      setValue(nextValue);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );

  const remove = useCallback(async () => {
    await storageService.remove(key);
    setValue(undefined);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return [value, set, remove];
}
