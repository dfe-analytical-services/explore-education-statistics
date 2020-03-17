import { DependencyList, useCallback, useMemo, useRef, useState } from 'react';

export interface AsyncState<T> {
  isLoading: boolean;
  value?: T;
  error?: Error;
}

export interface AsyncStateReturn<T> extends AsyncState<T> {
  setLoading: (loading: boolean) => void;
  setValue: (value: T) => void;
  setError: (error: Error) => void;
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
): [AsyncStateReturn<Value>, AsyncCallback<Args>] {
  const previousCall = useRef(0);
  const [state, setState] = useState<AsyncState<Value>>(initialState);

  const run = useCallback(async (...args: Args) => {
    previousCall.current += 1;

    const currentCall = previousCall.current;

    setState({
      isLoading: true,
    });

    try {
      const result = await callback(...args);

      if (currentCall === previousCall.current) {
        setState({
          isLoading: false,
          value: result,
        });
      }

      return result;
    } catch (error) {
      if (currentCall === previousCall.current) {
        setState({
          isLoading: false,
          error,
        });
      }

      return error;
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  const stateReturn: AsyncStateReturn<Value> = useMemo(() => {
    return {
      ...state,
      setLoading: isLoading =>
        setState({
          ...state,
          isLoading,
        }),
      setValue: value =>
        setState({
          ...state,
          value,
        }),
      setError: error =>
        setState({
          ...state,
          error,
        }),
    };
  }, [state]);

  return [stateReturn, run];
}
