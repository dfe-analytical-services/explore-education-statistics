import { useErrorControl } from '@common/contexts/ErrorControlContext';
import { AsyncStateSetterParam } from '@common/hooks/useAsyncCallback';
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
  initialState: AsyncHandledRetryStateSetterParam<T> = {
    isLoading: true,
  },
): AsyncHandledRetryState<T> {
  const { handleError } = useErrorControl();

  const { error, setState, ...state } = useAsyncRetry(
    task,
    deps,
    pick(initialState, ['value', 'isLoading']) as AsyncStateSetterParam<T>,
  );

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
          pick(nextState, ['value', 'isLoading']) as AsyncStateSetterParam<T>,
        );
      },
    };
  }, [setState, state]);
}
