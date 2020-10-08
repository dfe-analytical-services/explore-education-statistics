import { useCallback, useEffect, useRef } from 'react';

type CancelInterval = () => void;

/**
 * Call a {@param callback} at a regular
 * {@param delay} interval (in milliseconds).
 */
export default function useInterval(
  callback: () => void,
  delay: number,
): [CancelInterval] {
  const savedCallback = useRef<() => void>(callback);
  savedCallback.current = callback;

  const interval = useRef<NodeJS.Timeout>();

  const cancelInterval = useCallback(() => {
    if (interval.current) {
      clearInterval(interval.current);
    }
  }, []);

  useEffect(() => {
    interval.current = setInterval(() => savedCallback.current(), delay);
    return cancelInterval;
  }, [cancelInterval, delay]);

  return [cancelInterval];
}
