import { DependencyList, useCallback, useState } from 'react';

export interface AsyncState<T> {
  isLoading: boolean;
  value?: T;
  error?: Error;
}

export type AsyncCallback<Args extends unknown[]> = (...args: Args) => void;

/**
 * Hook that encapsulates state around an asynchronous
 * callback such as loading, error and successful values.
 */
export default function useAsyncCallback<Value, Args extends unknown[] = []>(
  callback: (...args: Args) => Promise<Value>,
  deps: DependencyList = [],
  initialState: AsyncState<Value> = {
    isLoading: true,
  },
): [AsyncState<Value>, AsyncCallback<Args>] {
  const [state, setState] = useState<AsyncState<Value>>(initialState);

  const run = useCallback(async (...args: Args) => {
    setState({
      isLoading: true,
    });

    try {
      const result = await callback(...args);

      setState({
        isLoading: false,
        value: result,
      });

      return result;
    } catch (error) {
      setState({
        isLoading: false,
        error,
      });

      return error;
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  return [state, run];
}
