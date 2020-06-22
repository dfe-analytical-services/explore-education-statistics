import { useEffect, useRef } from 'react';

/**
 * Hook that returns the previous render's
 * version of a given {@param value}.
 */
export default function usePrevious<T>(value: T): T | undefined {
  const ref = useRef<T>();

  useEffect(() => {
    ref.current = value;
  });

  return ref.current;
}
