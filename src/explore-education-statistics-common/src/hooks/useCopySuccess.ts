import { useCallback, useEffect, useRef, useState } from 'react';

type ShowCopySuccess = () => void;

/**
 * Track whether something has just been copied, resetting
 * back to false after {@param resetDelay} (in milliseconds).
 *
 * Showing the success again before the reset restarts the delay.
 */
export default function useCopySuccess(
  resetDelay = 5000,
): [boolean, ShowCopySuccess] {
  const [copySuccess, setCopySuccess] = useState(false);
  const timeout = useRef<ReturnType<typeof setTimeout>>(null);

  const clearResetTimeout = useCallback(() => {
    if (timeout.current) {
      clearTimeout(timeout.current);
    }
  }, []);

  useEffect(() => clearResetTimeout, [clearResetTimeout]);

  const showCopySuccess = useCallback(() => {
    clearResetTimeout();
    setCopySuccess(true);
    timeout.current = setTimeout(() => setCopySuccess(false), resetDelay);
  }, [clearResetTimeout, resetDelay]);

  return [copySuccess, showCopySuccess];
}
