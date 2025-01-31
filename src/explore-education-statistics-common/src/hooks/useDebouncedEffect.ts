import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { DependencyList, useEffect } from 'react';

/**
 * Runs an {@param effect} after a specified {@param timeout}.
 * Provide {@param deps} like a normal effect.
 * If the deps change before the timeout, the timeout will
 * be reset and the new effect will be ran instead.
 */
export default function useDebouncedEffect(
  effect: () => void,
  timeout = 0,
  deps: DependencyList = [],
): void {
  const [run] = useDebouncedCallback(effect, timeout);

  useEffect(() => {
    run();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);
}
