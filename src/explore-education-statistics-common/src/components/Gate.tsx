import useAsyncCallback, {
  AsyncStateSetterParam,
} from '@common/hooks/useAsyncCallback';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import React, { ReactNode, useEffect } from 'react';

interface GateProps {
  condition: boolean | (() => Promise<boolean>);
  children: ReactNode;
  fallback?: ReactNode | ((error?: unknown) => ReactNode);
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
  const [{ isLoading, value, error }, checkCondition] = useAsyncCallback(
    async () => (typeof condition === 'boolean' ? condition : condition()),
    [condition],
    () => {
      let initialState: AsyncStateSetterParam<boolean> = {
        isLoading: true,
      };

      if (typeof condition === 'boolean') {
        initialState = {
          isLoading: false,
          value: condition,
        };
      }

      return initialState;
    },
  );

  useEffect(() => {
    if (isLoading && !value && !error) {
      checkCondition();
    }
  }, [isLoading, value, error, checkCondition]);

  if (isLoading) {
    return loading;
  }

  if (value) {
    return children;
  }

  return typeof fallback === 'function' ? fallback(error) : fallback;
};

export default Gate;
