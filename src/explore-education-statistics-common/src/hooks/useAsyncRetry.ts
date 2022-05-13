import useAsyncCallback, {
  AsyncCallbackState,
  UseAsyncCallbackOptions,
} from '@common/hooks/useAsyncCallback';
import usePrevious from '@common/hooks/usePrevious';
import isEqual from 'lodash/isEqual';
import { DependencyList, useCallback, useEffect, useMemo } from 'react';

export interface AsyncRetryState<T> extends AsyncCallbackState<T> {
  retry: () => void;
}

export type UseAsyncRetryOptions<T> = UseAsyncCallbackOptions<T>;

/**
 * Runs an asynchronous task on component render
 * and provides a `retry` method if you want
 * to try and re-run the task later.
 */
export default function useAsyncRetry<T>(
  task: () => Promise<T>,
  deps: DependencyList = [],
  options: UseAsyncRetryOptions<T> = {},
): AsyncRetryState<T> {
  const prevDeps = usePrevious<DependencyList>(deps);

  const [state, run] = useAsyncCallback<T, []>(task, deps, {
    ...options,
    initialState: options.initialState ?? {
      isLoading: true,
    },
  });

  const { isLoading } = state;

  useEffect(() => {
    run();
  }, [run]);

  const retry = useCallback(async () => {
    if (!isLoading) {
      await run();
    }
  }, [isLoading, run]);

  return useMemo(() => {
    // The current state is only valid for the previous
    // dependencies, so we have to reset to initial
    // state if the dependencies change.
    // This avoids potential stale state issues due
    // to the task not having ran yet.
    if (prevDeps && !isEqual(prevDeps, deps)) {
      return {
        isLoading: true,
        setState: state.setState,
        retry,
      };
    }

    return {
      ...state,
      retry,
    };
  }, [deps, prevDeps, retry, state]);
}
