import { useCallback, useState } from 'react';

const useToggle = (
  initialValue: boolean,
): [boolean, (nextValue?: unknown) => void] => {
  const [value, setValue] = useState<boolean>(initialValue);

  const toggle = useCallback(
    (nextValue?: unknown) => {
      if (typeof nextValue === 'boolean') {
        setValue(nextValue);
      } else {
        setValue(currentValue => !currentValue);
      }
    },
    [setValue],
  );

  return [value, toggle];
};

export default useToggle;
