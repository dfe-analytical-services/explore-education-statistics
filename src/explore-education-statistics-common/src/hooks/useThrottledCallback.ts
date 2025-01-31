import useMountedRef from '@common/hooks/useMountedRef';
import { useCallback, useEffect, useRef } from 'react';

export type UseThrottledCallbackReturn<Args extends unknown[]> = [
  (...args: Args) => void,
  () => void,
];

/**
 * Throttles a {@param callback} so that it can only run *once*
 * until a {@param timeout} has passed (in milliseconds).
 *
 * If the throttled callback is run again before the timeout
 * has elapsed, the callback cannot be triggered until it is over.
 */
export default function useThrottledCallback<Args extends unknown[] = []>(
  callback: (...args: Args) => void,
  timeout = 0,
): UseThrottledCallbackReturn<Args> {
  const timeoutRef = useRef<ReturnType<typeof setTimeout>>();
  const callbackRef = useRef(callback);
  const mountedRef = useMountedRef();

  callbackRef.current = callback;

  const run = useCallback(
    (...args: Args) => {
      if (timeoutRef.current) {
        return;
      }

      timeoutRef.current = setTimeout(() => {
        if (mountedRef.current) {
          callbackRef.current(...args);
          timeoutRef.current = undefined;
        }
      }, timeout);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [timeout],
  );

  const cancel = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
      timeoutRef.current = undefined;
    }
  }, []);

  useEffect(() => {
    return cancel;
  }, [cancel, run]);

  return [run, cancel];
}
