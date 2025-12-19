import { useCallback, useMemo, useState } from 'react';

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

  const toggleFunc = useCallback((nextValue?: unknown) => {
    if (typeof nextValue === 'boolean') {
      setValue(nextValue);
    } else {
      setValue(currentValue => !currentValue);
    }
  }, []);

  const on = useCallback(() => setValue(true), []);
  const off = useCallback(() => setValue(false), []);

  const toggle = useMemo(() => {
    const func = toggleFunc as Toggle;
    func.on = on;
    func.off = off;
    return func;
  }, [toggleFunc, on, off]);

  return [value, toggle];
}
