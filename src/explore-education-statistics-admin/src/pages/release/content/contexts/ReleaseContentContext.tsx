import { ReleaseDispatchAction } from '@admin/pages/release/content/contexts/ReleaseContentContextActionTypes';
import { EditableRelease } from '@admin/services/releaseContentService';
import { Comment, EditableBlock } from '@admin/services/types/content';
import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import { ContentSection } from '@common/services/publicationService';
import { BaseBlock, DataBlock } from '@common/services/types/blocks';
import getUnresolvedComments from '@admin/pages/release/content/utils/getUnresolvedComments';
import remove from 'lodash/remove';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer } from 'use-immer';

export type ReleaseContextDispatch = (action: ReleaseDispatchAction) => void;
export type CommentsPendingDeletion = { [key: string]: string[] };

export type ReleaseContextState = {
  release: EditableRelease;
  canUpdateRelease: boolean;
  availableDataBlocks: DataBlock[];
  unresolvedComments: Comment[];
  commentsPendingDeletion?: CommentsPendingDeletion;
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
      const { block, meta, isSaving } = action.payload;
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

          matchingContentSection.content[blockIndex] = block ?? {
            ...matchingContentSection.content[blockIndex],
            isSaving,
          };
        }
      } else {
        const blockIndex = matchingSection.content.findIndex(
          contentBlock => contentBlock.id === blockId,
        );

        if (blockIndex !== -1) {
          matchingSection.content[blockIndex] = block ?? {
            ...matchingSection.content[blockIndex],
            isSaving,
          };
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
    case 'SET_COMMENTS_PENDING_DELETION': {
      const { commentId, meta } = action.payload;
      if (!draft.commentsPendingDeletion) {
        return draft;
      }
      if (!commentId) {
        draft.commentsPendingDeletion[meta.blockId] = [];
        return draft;
      }
      if (draft.commentsPendingDeletion[meta.blockId]) {
        if (draft.commentsPendingDeletion[meta.blockId].includes(commentId)) {
          draft.commentsPendingDeletion[
            meta.blockId
          ] = draft.commentsPendingDeletion[meta.blockId].filter(
            id => id !== commentId,
          );
        } else {
          draft.commentsPendingDeletion[meta.blockId] = [
            ...draft.commentsPendingDeletion[meta.blockId],
            commentId,
          ];
        }
      } else {
        draft.commentsPendingDeletion[meta.blockId] = [commentId];
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

      draft.unresolvedComments = getUnresolvedComments(
        draft.release,
        draft.commentsPendingDeletion,
      );
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

function ReleaseContentProvider({ children, value }: ReleaseProviderProps) {
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

function useReleaseContentState() {
  const context = useContext(ReleaseStateContext);
  if (context === undefined) {
    throw new Error('useReleaseState must be used within a ReleaseProvider');
  }
  return context;
}

function useReleaseContentDispatch() {
  const context = useContext(ReleaseDispatchContext);
  if (context === undefined) {
    throw new Error('useReleaseDispatch must be used within a ReleaseProvider');
  }
  return context;
}

export {
  ReleaseContentProvider,
  useReleaseContentState,
  useReleaseContentDispatch,
};
