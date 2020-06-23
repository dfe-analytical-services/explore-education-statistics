import { useErrorControl } from '@common/contexts/ErrorControlContext';
import { AsyncState } from '@common/hooks/useAsyncCallback';
import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { OmitStrict } from '@common/types';
import omit from 'lodash/omit';
import pick from 'lodash/pick';
import { DependencyList, useEffect, useMemo } from 'react';

export type AsyncHandledStateSetter<T> = OmitStrict<AsyncState<T>, 'error'>;

export type AsyncHandledRetryState<T> = OmitStrict<
  AsyncRetryState<T>,
  'error' | 'setState'
> & {
  setState: (state: AsyncHandledStateSetter<T>) => void;
};

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
  initialState?: AsyncHandledStateSetter<T>,
): AsyncHandledRetryState<T> {
  const { handleApiErrors } = useErrorControl();

  const asyncRetry = useAsyncRetry(task, deps, initialState);

  useEffect(() => {
    if (asyncRetry.error) {
      handleApiErrors(asyncRetry.error);
    }
  }, [asyncRetry.error, handleApiErrors]);

  return useMemo(() => {
    return {
      ...omit(asyncRetry, ['error', 'setState']),
      setState: state => {
        asyncRetry.setState(pick(state, ['value', 'isLoading']));
      },
    };
  }, [asyncRetry]);
}
