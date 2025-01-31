import { useEffect, useRef } from 'react';
import isEqual from 'lodash/isEqual';

interface Props<T> {
  value: T;
  onMount?: (value: T) => void;
  onChange?: (value: T, previousValue: T) => void;
}

export default function Effect<T>({ onChange, onMount, value }: Props<T>) {
  const rendered = useRef<boolean>(false);
  const previousValue = useRef(value);

  useEffect(() => {
    if (!rendered.current) {
      rendered.current = true;

      if (onMount) {
        onMount(value);
      }

      return;
    }

    // Prevent potential infinite re-rendering
    if (isEqual(previousValue.current, value)) {
      return;
    }

    if (onChange) {
      onChange(value, previousValue.current);
      previousValue.current = value;
    }
  }, [onChange, onMount, value]);

  return null;
}
