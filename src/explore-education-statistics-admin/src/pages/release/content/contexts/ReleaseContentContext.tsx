import { ReleaseDispatchAction } from '@admin/pages/release/content/contexts/ReleaseContentContextActionTypes';
import { EditableRelease } from '@admin/services/releaseContentService';
import { EditableBlock } from '@admin/services/types/content';
import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import { ContentSection } from '@common/services/publicationService';
import { BaseBlock, DataBlock } from '@common/services/types/blocks';
import remove from 'lodash/remove';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer } from 'use-immer';

export type ReleaseContentContextDispatch = (
  action: ReleaseDispatchAction,
) => void;

export interface ReleaseContentContextState {
  release: EditableRelease;
  canUpdateRelease: boolean;
  availableDataBlocks: DataBlock[];
}

const ReleaseContentStateContext = createContext<
  ReleaseContentContextState | undefined
>(undefined);

const ReleaseContentDispatchContext = createContext<
  ReleaseContentContextDispatch | undefined
>(undefined);

export const releaseReducer: Reducer<
  ReleaseContentContextState,
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
    default: {
      return draft;
    }
  }
};

interface ReleaseProviderProps {
  children: ReactNode;
  value: ReleaseContentContextState;
}

export function ReleaseContentProvider({
  children,
  value,
}: ReleaseProviderProps) {
  const [state, dispatch] = useLoggedImmerReducer(
    'Release',
    releaseReducer,
    value,
  );

  return (
    <ReleaseContentStateContext.Provider value={state}>
      <ReleaseContentDispatchContext.Provider value={dispatch}>
        {children}
      </ReleaseContentDispatchContext.Provider>
    </ReleaseContentStateContext.Provider>
  );
}

export function useReleaseContentState(): ReleaseContentContextState {
  const context = useContext(ReleaseContentStateContext);
  if (context === undefined) {
    throw new Error('useReleaseState must be used within a ReleaseProvider');
  }
  return context;
}

export function useReleaseContentDispatch(): ReleaseContentContextDispatch {
  const context = useContext(ReleaseContentDispatchContext);
  if (context === undefined) {
    throw new Error('useReleaseDispatch must be used within a ReleaseProvider');
  }
  return context;
}
