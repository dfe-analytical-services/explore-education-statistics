import { useErrorControl } from '@common/contexts/ErrorControlContext';
import { AsyncState } from '@common/hooks/useAsyncCallback';
import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { OmitStrict } from '@common/types';
import omit from 'lodash/omit';
import { DependencyList, useEffect } from 'react';

export type AsyncHandledRetryState<T> = OmitStrict<
  AsyncRetryState<T>,
  'error' | 'setError'
>;

/**
 * Wrapper around {@see useAsyncRetry} that automatically handles
 * API errors using the global {@see ErrorControlContext}.
 *
 * If you require more precise error handling,
 * you should use {@see useAsyncRetry} instead.
 */
export default function useAsyncHandledRetry<T>(
  task: () => Promise<T>,
  deps: DependencyList = [],
  initialState?: AsyncState<T>,
): AsyncHandledRetryState<T> {
  const { handleApiErrors } = useErrorControl();

  const asyncRetry = useAsyncRetry(task, deps, initialState);

  useEffect(() => {
    if (asyncRetry.error) {
      handleApiErrors(asyncRetry.error);
    }
  }, [asyncRetry.error, handleApiErrors]);

  return omit(asyncRetry, ['error', 'setError']);
}
