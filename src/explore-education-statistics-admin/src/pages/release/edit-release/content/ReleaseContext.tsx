import produce from 'immer';
import {
  EditableContentBlock,
  ExtendedComment,
  EditableRelease,
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
  release: EditableRelease | undefined;
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
    case 'REMOVE_BLOCK_FROM_SECTION':
      return produce<State>(state, draft => {
        const { sectionId, blockId, sectionKey } = action.payload.meta;
        if (
          draft.release === undefined ||
          typeof draft.release[sectionKey] !== 'object'
        ) {
          throw new Error('REMOVE_BLOCK_FROM_SECTION: failed');
        } else if (sectionKey === 'content') {
          draft.release[sectionKey] = draft.release[sectionKey].map(section => {
            if (section.id === sectionId) {
              return {
                content: section.content?.filter(
                  contentBlock => (contentBlock.id as string) !== blockId,
                ),
              } as ContentSection<EditableContentBlock>;
            }
            return section;
          });
        } else {
          (draft.release[sectionKey] as ContentSection<
            EditableContentBlock
          >).content = (draft.release[sectionKey] as ContentSection<
            EditableContentBlock
          >).content?.filter(
            contentBlock => (contentBlock.id as string) !== blockId,
          );
        }
      });
    case 'UPDATE_BLOCK_FROM_SECTION': {
      return produce<State>(state, draft => {
        const { block, meta } = action.payload;
        const { sectionId, blockId, sectionKey } = meta;
        if (
          draft.release === undefined ||
          typeof draft.release[sectionKey] !== 'object'
        ) {
          throw new Error('UPDATE_BLOCK_FROM_SECTION: failed');
        } else if (sectionKey === 'content') {
          draft.release[sectionKey] = draft.release[sectionKey].map(section => {
            if (section.id === sectionId) {
              return {
                content: section.content?.map(contentBlock => {
                  if ((contentBlock.id as string) === blockId) {
                    return block;
                  }
                  return contentBlock;
                }),
              } as ContentSection<EditableContentBlock>;
            }
            return section;
          });
        } else {
          (draft.release[sectionKey] as ContentSection<
            EditableContentBlock
          >).content = (draft.release[sectionKey] as ContentSection<
            EditableContentBlock
          >).content?.map(contentBlock => {
            if ((contentBlock.id as string) === blockId) {
              return block;
            }
            return contentBlock;
          });
        }
      });
    }
    case 'ADD_BLOCK_TO_SECTION': {
      return produce<State>(state, draft => {
        const { block, meta } = action.payload;
        const { sectionId, sectionKey } = meta;
        if (
          draft.release === undefined ||
          typeof draft.release[sectionKey] !== 'object'
        ) {
          throw new Error('ADD_BLOCK_TO_SECTION: failed');
        } else if (sectionKey === 'content') {
          draft.release[sectionKey] = draft.release[sectionKey].map(section => {
            if (section.id === sectionId) {
              return {
                content: section.content?.push(block),
              } as ContentSection<EditableContentBlock>;
            }
            return section;
          });
        } else if (
          (draft.release[sectionKey] as ContentSection<EditableContentBlock>)
            .content
        ) {
          ((draft.release[sectionKey] as ContentSection<EditableContentBlock>)
            .content as EditableContentBlock[]).push(block);
        } else {
          (draft.release[sectionKey] as ContentSection<
            EditableContentBlock
          >).content = [block];
        }
      });
    }
    case 'UPDATE_SECTION_CONTENT': {
      return produce<State>(state, draft => {
        const { sectionContent, meta } = action.payload;
        const { sectionId, sectionKey } = meta;
        if (
          draft.release === undefined ||
          typeof draft.release[sectionKey] !== 'object'
        ) {
          throw new Error('ADD_BLOCK_TO_SECTION: failed');
        } else if (sectionKey === 'content') {
          draft.release[sectionKey] = draft.release[sectionKey].map(section => {
            if (section.id === sectionId) {
              return { ...section, content: sectionContent };
            }
            return section;
          });
        } else {
          (draft.release[sectionKey] as ContentSection<
            EditableContentBlock
          >).content = sectionContent;
        }
      });
    }
    case 'ADD_CONTENT_SECTION': {
      return produce<State>(state, draft => {
        const { section } = action.payload;
        if (draft.release) draft.release.content.push(section);
      });
    }
    case 'SET_CONTENT': {
      return produce<State>(state, draft => {
        const { content } = action.payload;
        if (draft.release) draft.release.content = content;
      });
    }
    case 'UPDATE_CONTENT_SECTION': {
      return produce<State>(state, draft => {
        const { section, meta } = action.payload;
        const { sectionId } = meta;
        if (draft.release)
          draft.release.content = draft.release.content.map(
            accordionSection => {
              if (accordionSection.id === sectionId) {
                return section;
              }
              return accordionSection;
            },
          );
      });
    }
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
