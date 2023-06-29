import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { OmitStrict } from '@common/types';
import pick from 'lodash/pick';
import { DependencyList, useEffect, useMemo } from 'react';

export type AsyncHandledRetryStateSetterParam<T> =
  | {
      isLoading: true;
    }
  | {
      isLoading?: false;
      value: T;
    };

export type AsyncHandledRetryState<T> = OmitStrict<
  AsyncRetryState<T>,
  'error' | 'setState'
> & {
  setState: (state: AsyncHandledRetryStateSetterParam<T>) => void;
};

export interface UseAsyncHandledRetryOptions<T> {
  initialState?: AsyncHandledRetryStateSetterParam<T>;
}

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
  options?: UseAsyncHandledRetryOptions<T>,
): AsyncHandledRetryState<T> {
  const { handleError } = useErrorControl();

  const { error, setState, ...state } = useAsyncRetry(task, deps, options);

  useEffect(() => {
    if (error) {
      handleError(error);
    }
  }, [error, handleError]);

  return useMemo(() => {
    return {
      ...state,
      setState: nextState => {
        setState(
          pick(nextState, [
            'value',
            'isLoading',
          ]) as AsyncHandledRetryStateSetterParam<T>,
        );
      },
    };
  }, [setState, state]);
}
