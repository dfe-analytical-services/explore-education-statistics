import React, { createContext, ReactNode, useReducer, useContext } from 'react';
import { EditableContentBlock } from '@admin/services/publicationService';
import { AbstractRelease } from '@common/services/publicationService';

type Action = { type: 'increment' } | { type: 'decrement' };
type Dispatch = (action: Action) => void;
type State = { release: AbstractRelease<EditableContentBlock> | undefined };
type ReleaseProviderProps = { children: React.ReactNode };

const ReleaseStateContext = createContext<State | undefined>(undefined);
const ReleaseDispatchContext = createContext<Dispatch | undefined>(undefined);

function releaseReducer(state: State, action: Action) {
  return { release: undefined };
}

function ReleaseProvider({ children }: ReleaseProviderProps) {
  const [state, dispatch] = useReducer(releaseReducer, {
    release: undefined,
  });
  return (
    <ReleaseStateContext.Provider value={state}>
      <ReleaseDispatchContext.Provider value={dispatch}>
        {children}
      </ReleaseDispatchContext.Provider>
    </ReleaseStateContext.Provider>
  );
}

function useReleaseState() {
  const context = useContext(ReleaseStateContext);
  if (context === undefined) {
    throw new Error('useReleaseState must be used within a ReleaseProvider');
  }
  return context;
}

function useReleaseDispatch() {
  const context = useContext(ReleaseDispatchContext);
  if (context === undefined) {
    throw new Error('useReleaseDispatch must be used within a ReleaseProvider');
  }
  return context;
}

export { ReleaseProvider, useReleaseState, useReleaseDispatch };
