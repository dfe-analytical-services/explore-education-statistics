import { useEffect, useRef } from 'react';

interface Props<T> {
  value: T;
  onChange: (value: T) => void;
}

const Effect = <T extends unknown>({ onChange, value }: Props<T>) => {
  const rendered = useRef<boolean>(false);

  const valueDependency = JSON.stringify(value);

  useEffect(() => {
    if (!rendered.current) {
      rendered.current = true;
    } else {
      onChange(value);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [onChange, valueDependency]);

  return null;
};

export default Effect;
