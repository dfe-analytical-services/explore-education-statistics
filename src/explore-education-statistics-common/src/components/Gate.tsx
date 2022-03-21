import usePrevious from '@common/hooks/usePrevious';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import React, { ReactNode, useEffect, useState } from 'react';

interface GateProps {
  condition: boolean | (() => Promise<boolean>) | (() => boolean);
  children: ReactNode;
  /**
   * Fallback to render if the {@property condition}
   * has not passed yet.
   */
  fallback?: ReactNode | ((error?: unknown) => ReactNode);
  /**
   * Loading state to render if the {@property condition}
   * is dependent on an asynchronous task.
   */
  loading?: ReactNode;
  /**
   * If the {@property condition} passes once, then we
   * consider the gate 'closed' and will not try to
   * re-run the condition and will not unmount the
   * children if the condition changes back to false.
   */
  passOnce?: boolean;
}

/**
 * Conditionally render its children based on a condition
 * that must be passed. This may be an asynchronous task
 * or just any boolean.
 */
const Gate = ({
  children,
  condition,
  fallback = null,
  loading = null,
  passOnce = true,
}: GateProps) => {
  const previousCondition = usePrevious(condition);

  const [passed, setPassed] = useState(false);
  const [isLoading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>();

  useEffect(() => {
    const checkCondition = async () => {
      if (previousCondition === condition) {
        return;
      }

      if (passed && passOnce) {
        return;
      }

      if (isLoading) {
        return;
      }

      if (typeof condition === 'boolean') {
        setPassed(condition);
        return;
      }

      const task = condition();

      if (task instanceof Promise) {
        setLoading(true);

        try {
          setPassed(await task);
        } catch (err) {
          setError(err);
        } finally {
          setLoading(false);
        }
      } else {
        setPassed(task);
      }
    };

    checkCondition();
  }, [condition, isLoading, passOnce, passed, previousCondition]);

  if (isLoading) {
    return loading;
  }

  if (passed) {
    return children;
  }

  return typeof fallback === 'function' ? fallback(error) : fallback;
};

export default Gate;
