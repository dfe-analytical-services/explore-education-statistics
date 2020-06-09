import { EditableRelease } from '@admin/services/releaseContentService';
import { EditableBlock, Comment } from '@admin/services/types/content';
import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import { ContentSection } from '@common/services/publicationService';
import { BaseBlock, DataBlock } from '@common/services/types/blocks';
import remove from 'lodash/remove';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer } from 'use-immer';
import { ReleaseDispatchAction } from './ReleaseContextActionTypes';

export type ReleaseContextDispatch = (action: ReleaseDispatchAction) => void;

export type ReleaseContextState = {
  release: EditableRelease;
  canUpdateRelease: boolean;
  availableDataBlocks: DataBlock[];
  unresolvedComments: Comment[];
};
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
    case 'SET_AVAILABLE_DATABLOCKS': {
      draft.availableDataBlocks = action.payload;
      return draft;
    }
    case 'REMOVE_BLOCK_FROM_SECTION': {
      const { sectionId, blockId, sectionKey } = action.payload.meta;
      const matchingSection = draft.release[sectionKey] as
        | ContentSection<BaseBlock>
        | ContentSection<BaseBlock>[];

      if (!matchingSection) {
        throw new Error(
          `${action.type}: Section "${sectionKey}" could not be found.`,
        );
      }

      if (Array.isArray(matchingSection)) {
        const matchingContentSection = matchingSection.find(
          section => section.id === sectionId,
        );

        if (matchingContentSection) {
          remove(
            matchingContentSection.content,
            content => content.id === blockId,
          );
        }
      } else {
        remove(matchingSection.content, content => content.id === blockId);
      }

      return draft;
    }
    case 'UPDATE_BLOCK_FROM_SECTION': {
      const { block, meta } = action.payload;
      const { sectionId, blockId, sectionKey } = meta;

      const matchingSection = draft.release[sectionKey] as
        | ContentSection<BaseBlock>
        | ContentSection<BaseBlock>[];

      if (!matchingSection) {
        throw new Error(
          `${action.type}: Section "${sectionKey}" could not be found.`,
        );
      }

      if (Array.isArray(matchingSection)) {
        const matchingContentSection = matchingSection.find(
          section => section.id === sectionId,
        );

        if (matchingContentSection) {
          const blockIndex = matchingContentSection.content.findIndex(
            contentBlock => contentBlock.id === blockId,
          );

          matchingContentSection.content[blockIndex] = block;
        }
      } else {
        const blockIndex = matchingSection.content.findIndex(
          contentBlock => contentBlock.id === blockId,
        );

        if (blockIndex !== -1) {
          matchingSection.content[blockIndex] = block;
        }
      }

      return draft;
    }
    case 'ADD_BLOCK_TO_SECTION': {
      const { block, meta } = action.payload;
      const { sectionId, sectionKey } = meta;

      const matchingSection = draft.release[sectionKey] as
        | ContentSection<BaseBlock>
        | ContentSection<BaseBlock>[];

      if (!matchingSection) {
        throw new Error(
          `${action.type}: Section "${sectionKey}" could not be found.`,
        );
      }

      // comments needs initialising to array as will be undefined if empty
      const newBlock: EditableBlock = {
        ...block,
        comments: block.comments || [],
      };

      if (Array.isArray(matchingSection)) {
        const matchingContentSection = matchingSection.find(
          section => section.id === sectionId,
        );

        if (matchingContentSection) {
          matchingContentSection.content.push(newBlock);
        }
      } else {
        matchingSection.content.push(newBlock);
      }

      return draft;
    }
    case 'UPDATE_SECTION_CONTENT': {
      const { sectionContent, meta } = action.payload;
      const { sectionId, sectionKey } = meta;

      const matchingSection = draft.release[sectionKey] as
        | ContentSection<BaseBlock>
        | ContentSection<BaseBlock>[];

      if (!matchingSection) {
        throw new Error(
          `${action.type}: Section "${sectionKey}" could not be found.`,
        );
      }

      if (Array.isArray(matchingSection)) {
        const matchingContentSection = matchingSection.find(
          section => section.id === sectionId,
        );

        if (matchingContentSection) {
          matchingContentSection.content = sectionContent;
        }
      } else {
        matchingSection.content = sectionContent;
      }

      return draft;
    }
    case 'ADD_CONTENT_SECTION': {
      const { section } = action.payload;

      if (draft.release) {
        draft.release.content.push({
          ...section,
          content: [],
        });
      }

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

        if (sectionIndex !== -1) {
          draft.release.content[sectionIndex] = section;
        }
      }
      return draft;
    }
    case 'UPDATE_BLOCK_COMMENTS': {
      const { comments, meta } = action.payload;
      const { sectionId, sectionKey, blockId } = meta;

      const matchingSection = draft.release[sectionKey] as
        | ContentSection<EditableBlock>
        | ContentSection<EditableBlock>[];

      if (!matchingSection) {
        throw new Error(
          `${action.type}: Section "${sectionKey}" could not be found.`,
        );
      }

      let matchingBlock;
      if (Array.isArray(matchingSection)) {
        const matchingContentSection = matchingSection.find(
          section => section.id === sectionId,
        );

        if (matchingContentSection) {
          matchingBlock = matchingContentSection.content.find(
            block => block.id === blockId,
          );
        }
      } else {
        matchingBlock = matchingSection.content.find(
          block => block.id === blockId,
        );
      }

      if (!matchingBlock) {
        throw new Error(
          `${action.type}: Block "${blockId}" could not be found with sectionKey "${sectionKey}" and sectionId "${sectionId}".`,
        );
      } else {
        matchingBlock.comments = comments;
      }
      return draft;
    }
    default: {
      return draft;
    }
  }
};

interface ReleaseProviderProps {
  children: ReactNode;
  value: ReleaseContextState;
}

function ReleaseProvider({ children, value }: ReleaseProviderProps) {
  const [state, dispatch] = useLoggedImmerReducer(
    'Release',
    releaseReducer,
    value,
  );

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
