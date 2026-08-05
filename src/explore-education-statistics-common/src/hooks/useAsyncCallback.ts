import logger from '@common/services/logger';
import {
  DependencyList,
  SetStateAction,
  useCallback,
  useMemo,
  useRef,
  useState,
} from 'react';

export interface AsyncState<T> {
  isLoading: boolean;
  value?: T;
  error?: unknown;
}

export type AsyncStateSetterParam<T> = SetStateAction<
  | {
      isLoading: boolean;
    }
  | {
      isLoading?: false;
      value: T;
    }
  | {
      isLoading?: false;
      error: unknown;
    }
>;

export interface AsyncCallbackState<T> extends AsyncState<T> {
  setState: (state: AsyncStateSetterParam<T>) => void;
}

export type AsyncCallback<Args extends unknown[]> = (...args: Args) => void;

export interface UseAsyncCallbackOptions<Value> {
  initialState?: AsyncStateSetterParam<Value>;
  /**
   * When set to true, the last value will be
   * kept in state when re-running the callback.
   * By default, the last value is unset from
   * state whilst the callback is running.
   */
  keepStaleValue?: boolean;
}

/**
 * Hook that encapsulates state around an asynchronous
 * callback such as loading, error and successful values.
 */
export default function useAsyncCallback<Value, Args extends unknown[] = []>(
  callback: (...args: Args) => Promise<Value>,
  deps: DependencyList = [],
  options: UseAsyncCallbackOptions<Value> = {},
): [AsyncCallbackState<Value>, AsyncCallback<Args>] {
  const {
    initialState = {
      isLoading: false,
    },
    keepStaleValue,
  } = options;

  const previousCall = useRef(0);

  const [state, setState] = useState<AsyncState<Value>>(
    initialState as AsyncState<Value>,
  );

  const run = useCallback(async (...args: Args) => {
    previousCall.current += 1;

    const currentCall = previousCall.current;

    setState(prev =>
      keepStaleValue && prev.value
        ? {
            isLoading: true,
            value: prev.value,
          }
        : {
            isLoading: true,
          },
    );

    try {
      const nextValue = await callback(...args);

      if (currentCall === previousCall.current) {
        setState({
          isLoading: false,
          value: nextValue,
        });
      }
    } catch (error) {
      if (currentCall === previousCall.current) {
        if (process.env.NODE_ENV !== 'test') {
          logger.error(error);
        }

        setState({
          isLoading: false,
          error,
        });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  const stateSetter = useCallback(
    (setter: AsyncStateSetterParam<Value>) => {
      const nextState = typeof setter === 'function' ? setter(state) : setter;

      if (nextState.isLoading) {
        setState({ isLoading: true });
      } else {
        setState({
          ...nextState,
          isLoading: false,
        });
      }
    },
    [state],
  );

  const stateReturn: AsyncCallbackState<Value> = useMemo(() => {
    return {
      ...state,
      setState: stateSetter,
    };
  }, [state, stateSetter]);

  return [stateReturn, run];
}
