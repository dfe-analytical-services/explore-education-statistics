import { useCallback, useState } from 'react';

export type Toggle = ((nextValue?: boolean | unknown) => void) & {
  off: () => void;
  on: () => void;
};

/**
 * Hook to add toggle functionality with the initial
 * state being set from {@param initialValue}.
 */
export default function useToggle(initialValue: boolean): [boolean, Toggle] {
  const [value, setValue] = useState<boolean>(initialValue);

  const toggleFunc = useCallback(
    (nextValue?: unknown) => {
      if (typeof nextValue === 'boolean') {
        setValue(nextValue);
      } else {
        setValue(currentValue => !currentValue);
      }
    },
    [setValue],
  );

  const toggle = toggleFunc as Toggle;

  toggle.off = useCallback(() => setValue(false), [setValue]);
  toggle.on = useCallback(() => setValue(true), [setValue]);

  return [value, toggle];
}
