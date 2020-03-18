import useAsyncCallback from '@common/hooks/useAsyncCallback';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import React, { ReactNode, useEffect } from 'react';

interface GateProps {
  condition: boolean | (() => Promise<boolean>);
  children: ReactNode;
  fallback?: ReactNode | ((error?: Error) => ReactNode);
  loading?: ReactNode;
}

/**
 * Conditionally render {@param children} based on a
 * {@param condition} that must be passed. This may be
 * an asynchronous task or just any boolean.
 *
 * @param loading state can be rendered if it is asynchronous.
 * @param fallback will be rendered if the condition
 * fails or there is an error.
 */
const Gate = ({
  children,
  condition,
  fallback = null,
  loading = null,
}: GateProps) => {
  const [
    { isLoading, value: isSuccess, error },
    checkCondition,
  ] = useAsyncCallback(
    async () => (typeof condition === 'boolean' ? condition : condition()),
    [condition],
    {
      isLoading: typeof condition !== 'boolean',
      value: typeof condition === 'boolean' ? condition : undefined,
    },
  );

  useEffect(() => {
    if (isLoading && !isSuccess && !error) {
      checkCondition();
    }
  }, [isLoading, isSuccess, error, checkCondition]);

  if (isLoading) {
    return loading;
  }

  if (isSuccess) {
    return children;
  }

  return typeof fallback === 'function' ? fallback(error) : fallback;
};

export default Gate;
