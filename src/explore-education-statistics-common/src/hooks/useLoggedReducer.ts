import logger from '@common/services/logger';
import produce, { Immutable } from 'immer';
import {
  Dispatch,
  Reducer,
  ReducerWithoutAction,
  useCallback,
  useReducer,
} from 'react';
import { Reducer as ImmerReducer } from 'use-immer';

export default function useLoggedReducer<S, A>(
  name: string,
  reducer: Reducer<S, A>,
  initialState: S,
  initializer?: ReducerWithoutAction<S>,
): [S, Dispatch<A>] {
  const cachedReducer = useCallback(
    (state: S, action: A) => {
      logger.debugGroup(`${name} reducer:`);
      logger.debug(
        '%cPrevious State:',
        'color: #9E9E9E; font-weight: 700;',
        state,
      );

      logger.debug('%cAction:', 'color: #00A7F7; font-weight: 700;', action);

      const nextState = reducer(state, action);

      logger.debug(
        '%cNext State:',
        'color: #47B04B; font-weight: 700;',
        nextState,
      );

      logger.debugGroupEnd();

      return nextState;
    },
    [name, reducer],
  );

  return useReducer(
    cachedReducer,
    initialState,
    initializer as ReducerWithoutAction<S>,
  );
}

export function useLoggedImmerReducer<S, A>(
  name: string,
  reducer: ImmerReducer<S, A>,
  initialState: S,
  initializer?: ReducerWithoutAction<S>,
): [S & Immutable<S>, Dispatch<A>] {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const cachedReducer = useCallback(produce(reducer), [reducer]);

  return useLoggedReducer(
    name,
    cachedReducer as never,
    initialState,
    initializer,
  ) as [S & Immutable<S>, Dispatch<A>];
}
