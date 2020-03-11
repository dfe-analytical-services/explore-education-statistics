import {
  EditableRelease,
  ExtendedComment,
} from '@admin/services/publicationService';
import { DataBlock } from '@common/services/dataBlockService';
import remove from 'lodash/remove';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer, useImmerReducer } from 'use-immer';
import ReleaseDispatchAction from './ReleaseContextActionTypes';

type ReleaseContextDispatch = (action: ReleaseDispatchAction) => void;
type ReleaseContextState = {
  release: EditableRelease | undefined;
  canUpdateRelease: boolean;
  availableDataBlocks: DataBlock[];
  unresolvedComments: ExtendedComment[];
};
type ReleaseProviderProps = { children: ReactNode };

const ReleaseStateContext = createContext<ReleaseContextState | undefined>(
  undefined,
);
const ReleaseDispatchContext = createContext<
  ReleaseContextDispatch | undefined
>(undefined);

export const releaseReducer: Reducer<
  ReleaseContextState,
  ReleaseDispatchAction
> = (draft, action) => {
  switch (action.type) {
    case 'CLEAR_STATE': {
      return {
        release: undefined,
        canUpdateRelease: false,
        availableDataBlocks: [],
        unresolvedComments: [],
      };
    }
    case 'SET_STATE':
    case 'SET_AVAILABLE_DATABLOCKS': {
      return { ...draft, ...action.payload };
    }
    case 'REMOVE_BLOCK_FROM_SECTION': {
      const { sectionId, blockId, sectionKey } = action.payload.meta;
      if (!draft.release?.[sectionKey]) {
        throw new Error('REMOVE_BLOCK_FROM_SECTION: failed');
      }

      if (sectionKey === 'content') {
        const matchingSection = draft.release[sectionKey].find(
          section => section.id === sectionId,
        );
        if (matchingSection?.content) {
          remove(matchingSection.content, content => content.id === blockId);
        }
      } else if (draft.release?.[sectionKey]?.content) {
        remove(
          draft.release?.[sectionKey]?.content,
          content => content.id === blockId,
        );
      }

      return draft;
    }
    case 'UPDATE_BLOCK_FROM_SECTION': {
      const { block, meta } = action.payload;
      const { sectionId, blockId, sectionKey } = meta;
      if (!draft.release?.[sectionKey]) {
        throw new Error('UPDATE_BLOCK_FROM_SECTION: failed');
      }
      if (sectionKey === 'content') {
        const matchingSection = draft.release[sectionKey].find(
          section => section.id === sectionId,
        );
        if (matchingSection?.content) {
          const blockIndex = matchingSection.content.findIndex(
            contentBlock => contentBlock.id === blockId,
          );
          matchingSection.content[blockIndex] = block;
        }
      } else if (draft.release?.[sectionKey]?.content) {
        const matchingSectionContent = draft.release?.[sectionKey].content;
        const blockIndex = matchingSectionContent.findIndex(
          contentBlock => contentBlock.id === blockId,
        );
        if (blockIndex !== -1) {
          matchingSectionContent[blockIndex] = block;
        }
      }
      return draft;
    }
    case 'ADD_BLOCK_TO_SECTION': {
      const { block, meta } = action.payload;
      const { sectionId, sectionKey } = meta;
      if (!draft.release?.[sectionKey]) {
        throw new Error('ADD_BLOCK_TO_SECTION: failed');
      }

      if (sectionKey === 'content') {
        // .comments needs initialising to array as will be undefined if empty
        const newBlock = { ...block, comments: block.comments || [] };
        const matchingSection = draft.release[sectionKey].find(
          section => section.id === sectionId,
        );
        if (!matchingSection) return draft;
        if (Array.isArray(matchingSection.content)) {
          matchingSection.content.push(newBlock);
        } else {
          matchingSection.content = [newBlock];
        }
      } else if (draft.release?.[sectionKey]) {
        const matchingSection = draft.release[sectionKey];
        if (matchingSection) {
          if (Array.isArray(matchingSection.content)) {
            matchingSection.content.push(block);
          } else {
            matchingSection.content = [block];
          }
        }
      }
      return draft;
    }
    case 'UPDATE_SECTION_CONTENT': {
      const { sectionContent, meta } = action.payload;
      const { sectionId, sectionKey } = meta;
      if (!draft.release?.[sectionKey]) {
        throw new Error('ADD_BLOCK_TO_SECTION: failed');
      }

      if (sectionKey === 'content') {
        const matchingSection = draft.release[sectionKey].find(
          section => section.id === sectionId,
        );
        if (matchingSection) matchingSection.content = sectionContent;
      } else if (draft.release?.[sectionKey]) {
        const matchingSection = draft.release[sectionKey];
        if (matchingSection) matchingSection.content = sectionContent;
      }
      return draft;
    }
    case 'ADD_CONTENT_SECTION': {
      const { section } = action.payload;
      if (draft.release) draft.release.content.push(section);
      return draft;
    }
    case 'SET_CONTENT': {
      const { content } = action.payload;
      if (draft.release) {
        draft.release.content = content;
      }
      return draft;
    }
    case 'UPDATE_CONTENT_SECTION': {
      const { section, meta } = action.payload;
      const { sectionId } = meta;

      if (draft.release) {
        const sectionIndex = draft.release.content.findIndex(
          accordionSection => accordionSection.id === sectionId,
        );
        if (sectionIndex !== -1) draft.release.content[sectionIndex] = section;
      }
      return draft;
    }
    default: {
      return draft;
    }
  }
};

function ReleaseProvider({ children }: ReleaseProviderProps) {
  const [state, dispatch] = useImmerReducer(releaseReducer, {
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
  ReleaseProvider,
  useReleaseState,
  useReleaseDispatch,
  // eslint-disable-next-line no-undef
  ReleaseContextState,
  // eslint-disable-next-line no-undef
  ReleaseContextDispatch,
};
