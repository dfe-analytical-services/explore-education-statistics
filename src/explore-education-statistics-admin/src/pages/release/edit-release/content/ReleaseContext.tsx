import produce from 'immer';
import {
  EditableContentBlock,
  ExtendedComment,
} from '@admin/services/publicationService';
import { DataBlock } from '@common/services/dataBlockService';
import {
  AbstractRelease,
  ContentSection,
} from '@common/services/publicationService';
import React, { createContext, useContext, useReducer } from 'react';
import ReleaseDispatchAction from './actions';

type Dispatch = (action: ReleaseDispatchAction) => void;
type State = {
  release: AbstractRelease<EditableContentBlock> | undefined;
  canUpdateRelease: boolean;
  availableDataBlocks: DataBlock[];
  unresolvedComments: ExtendedComment[];
  pageError?: string;
};
type ReleaseProviderProps = { children: React.ReactNode };

const ReleaseStateContext = createContext<State | undefined>(undefined);
const ReleaseDispatchContext = createContext<Dispatch | undefined>(undefined);

function releaseReducer(state: State, action: ReleaseDispatchAction) {
  switch (action.type) {
    case 'PAGE_ERROR':
      return produce<State>(state, draft => {
        draft.release = undefined;
        draft.canUpdateRelease = false;
        draft.availableDataBlocks = [];
        draft.unresolvedComments = [];
        draft.pageError = action.payload.pageError;
      });
    case 'CLEAR_STATE':
      return produce<State>(state, draft => {
        return {
          release: undefined,
          canUpdateRelease: false,
          availableDataBlocks: [],
          unresolvedComments: [],
          pageError: undefined,
        };
      });
    case 'SET_STATE':
    case 'SET_AVAILABLE_DATABLOCKS':
      return { ...state, ...action.payload };
    case 'REMOVE_BLOCK_FROM_SECTION': {
      const { release } = state;
      const { sectionId, blockId, sectionKey } = action.payload.meta;
      if (release === undefined || typeof release[sectionKey] === 'object') {
        throw new Error('REMOVE_BLOCK_FROM_SECTION: failed');
      } else if (release[sectionKey] instanceof Array) {
        return {
          ...state,
          release: {
            ...release,
            [sectionKey]: (release[sectionKey] as ContentSection<
              EditableContentBlock
            >[]).map(section => {
              if (section.id === sectionId) {
                return {
                  content: section.content?.filter(
                    contentBlock => (contentBlock.id as string) !== blockId,
                  ),
                };
              }
              return section;
            }),
          },
        };
      } else if (
        (release[sectionKey] as ContentSection<EditableContentBlock>).id ===
        sectionId
      ) {
        return {
          ...state,
          release: {
            ...release,
            [sectionKey]: {
              ...release[sectionKey],
              content: (release[sectionKey] as ContentSection<
                EditableContentBlock
              >).content?.filter(
                contentBlock => (contentBlock.id as string) !== blockId,
              ),
            },
          },
        };
      } else {
        throw new Error(
          'REMOVE_BLOCK_FROM_SECTION: failed to find block for removal, in release.',
        );
      }
    }
    // case 'UPDATE_BLOCK_FROM_SECTION': {}
    default: {
      return { ...state };
    }
  }
}

function ReleaseProvider({ children }: ReleaseProviderProps) {
  const [state, dispatch] = useReducer(releaseReducer, {
    release: undefined,
    canUpdateRelease: false,
    availableDataBlocks: [],
    unresolvedComments: [],
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

export {
  // eslint-disable-next-line no-undef
  State,
  // eslint-disable-next-line no-undef
  Dispatch,
  ReleaseProvider,
  useReleaseState,
  useReleaseDispatch,
};
