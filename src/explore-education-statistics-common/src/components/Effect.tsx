import { useEffect, useRef } from 'react';
import isEqual from 'lodash/isEqual';

interface Props<T> {
  value: T;
  onChange: (value: T) => void;
}

const Effect = <T extends unknown>({ onChange, value }: Props<T>) => {
  const rendered = useRef<boolean>(false);
  const previousValue = useRef(value);

  useEffect(() => {
    if (!rendered.current) {
      rendered.current = true;
      return;
    }

    // Prevent potential infinite re-rendering
    if (isEqual(previousValue.current, value)) {
      return;
    }

    onChange(value);
    previousValue.current = value;
  }, [onChange, value]);

  return null;
};

export default Effect;
