import { DependencyList, useEffect, useRef } from 'react';

/**
 * Assign a given {@param callback} to a ref and
 * update it whenever the {@param deps} change.
 *
 * This is useful in situations where it's not possible
 * to ensure that the callback has the most up-to-date
 * dependencies e.g. where it might be called
 * outside of the React lifecycle.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export default function useCallbackRef<T extends (...args: any[]) => unknown>(
  callback: T,
  deps: DependencyList = [],
) {
  const ref = useRef<T>(callback);

  useEffect(() => {
    ref.current = callback;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  return ref;
}
