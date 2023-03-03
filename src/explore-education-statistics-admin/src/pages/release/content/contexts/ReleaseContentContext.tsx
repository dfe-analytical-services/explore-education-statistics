import {
  BlockMeta,
  ContentSectionKeys,
  ReleaseDispatchAction,
} from '@admin/pages/release/content/contexts/ReleaseContentContextActionTypes';
import { EditableRelease } from '@admin/services/releaseContentService';
import { EditableBlock } from '@admin/services/types/content';
import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import { ContentSection } from '@common/services/publicationService';
import { DataBlock } from '@common/services/types/blocks';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer } from 'use-immer';

export type ReleaseContentContextDispatch = (
  action: ReleaseDispatchAction,
) => void;

export interface ReleaseContentContextState {
  release: EditableRelease;
  canUpdateRelease: boolean;
  unattachedDataBlocks: DataBlock[];
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
  function getSection({
    sectionKey,
    sectionId,
  }: {
    sectionKey: ContentSectionKeys;
    sectionId: string;
  }): ContentSection<EditableBlock> | undefined {
    const matchingSection = draft.release[sectionKey] as
      | ContentSection<EditableBlock>
      | ContentSection<EditableBlock>[];

    if (!matchingSection) {
      throw new Error(
        `${action.type}: Section "${sectionKey}" could not be found.`,
      );
    }

    if (Array.isArray(matchingSection)) {
      return matchingSection.find(section => section.id === sectionId);
    }

    return matchingSection;
  }

  function getBlock({
    sectionKey,
    sectionId,
    blockId,
  }: BlockMeta): EditableBlock | undefined {
    const section = getSection({ sectionKey, sectionId });

    return section?.content.find(block => block.id === blockId);
  }

  switch (action.type) {
    case 'SET_UNATTACHED_DATABLOCKS': {
      draft.unattachedDataBlocks = action.payload;
      return draft;
    }
    case 'REMOVE_SECTION_BLOCK': {
      const { meta } = action.payload;
      const { blockId } = meta;

      const matchingSection = getSection(meta);

      if (matchingSection) {
        matchingSection.content = matchingSection.content.filter(
          block => block.id !== blockId,
        );
      }

      return draft;
    }
    case 'UPDATE_SECTION_BLOCK': {
      const { block, meta } = action.payload;
      const { blockId } = meta;

      const matchingSection = getSection(meta);

      if (matchingSection) {
        const blockIndex = matchingSection.content.findIndex(
          contentBlock => contentBlock.id === blockId,
        );

        if (blockIndex !== -1) {
          matchingSection.content[blockIndex] = block;
        }
      }

      return draft;
    }
    case 'ADD_SECTION_BLOCK': {
      const { block, meta } = action.payload;

      const matchingSection = getSection(meta);

      matchingSection?.content.push({
        ...block,
        comments: block.comments ?? [],
      });

      return draft;
    }
    case 'ADD_BLOCK_COMMENT': {
      const { comment, meta } = action.payload;

      const block = getBlock(meta);

      if (block) {
        block.comments.push(comment);
      }

      return draft;
    }
    case 'UPDATE_BLOCK_COMMENT': {
      const { comment, meta } = action.payload;

      const block = getBlock(meta);

      if (block) {
        const index = block.comments.findIndex(c => c.id === comment.id);

        if (index !== -1) {
          block.comments[index] = comment;
        }
      }

      return draft;
    }
    case 'REMOVE_BLOCK_COMMENT': {
      const { commentId, meta } = action.payload;

      const block = getBlock(meta);

      if (block) {
        const index = block.comments.findIndex(
          comment => comment.id === commentId,
        );

        if (index !== -1) {
          block.comments.splice(index, 1);
        }
      }

      return draft;
    }
    case 'UPDATE_SECTION_CONTENT': {
      const { sectionContent, meta } = action.payload;

      const matchingSection = getSection(meta);

      if (matchingSection) {
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
    case 'ADD_KEY_STATISTIC': {
      const { keyStatistic } = action.payload;
      draft.release.keyStatistics.push(keyStatistic);
      return draft;
    }
    case 'UPDATE_KEY_STATISTIC': {
      const { keyStatistic } = action.payload;

      const keyStatisticIndex = draft.release.keyStatistics.findIndex(
        ks => ks.id === keyStatistic.id,
      );

      if (keyStatisticIndex !== -1) {
        draft.release.keyStatistics[keyStatisticIndex] = keyStatistic;
      }

      return draft;
    }
    case 'REMOVE_KEY_STATISTIC': {
      const { keyStatisticId } = action.payload;

      draft.release.keyStatistics = draft.release.keyStatistics.filter(
        ks => ks.id !== keyStatisticId,
      );

      return draft;
    }
    case 'SET_KEY_STATISTICS': {
      const { keyStatistics } = action.payload;
      draft.release.keyStatistics = keyStatistics;
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
