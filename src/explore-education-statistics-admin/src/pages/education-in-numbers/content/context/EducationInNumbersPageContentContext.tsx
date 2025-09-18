import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import remove from 'lodash/remove';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer } from 'use-immer';
import { EinContent } from '@admin/services/educationInNumbersContentService';
import { EinSummary } from '@admin/services/educationInNumbersService';
import { EducationInNumbersPageDispatchAction } from './EducationInNumbersPageContentContextActionTypes';

export type EducationInNumbersPageContextDispatch = (
  action: EducationInNumbersPageDispatchAction,
) => void;

export type EducationInNumbersPageContextState = {
  pageContent: EinContent;
  pageVersion: EinSummary;
};

const EducationInNumbersPageStateContext = createContext<
  EducationInNumbersPageContextState | undefined
>(undefined);
const EducationInNumbersPageDispatchContext = createContext<
  EducationInNumbersPageContextDispatch | undefined
>(undefined);

export const educationInNumbersPageReducer: Reducer<
  EducationInNumbersPageContextState,
  EducationInNumbersPageDispatchAction
> = (draft, action) => {
  switch (action.type) {
    case 'REMOVE_BLOCK_FROM_SECTION': {
      const { sectionId, blockId } = action.payload.meta;
      if (!draft.pageContent.content) {
        throw new Error(
          `${action.type}: Error - Section "content" could not be found.`,
        );
      }

      const matchingSection = draft.pageContent.content.find(
        section => section.id === sectionId,
      );
      if (matchingSection?.content) {
        remove(matchingSection.content, content => content.id === blockId);
      }

      return draft;
    }
    case 'UPDATE_BLOCK_FROM_SECTION': {
      const { block, meta } = action.payload;
      const { sectionId, blockId } = meta;
      if (!draft.pageContent.content) {
        throw new Error(
          `${action.type}: Error - Section "content" could not be found.`,
        );
      }
      const matchingSection = draft.pageContent.content.find(
        section => section.id === sectionId,
      );
      if (matchingSection?.content) {
        const blockIndex = matchingSection.content.findIndex(
          contentBlock => contentBlock.id === blockId,
        );
        matchingSection.content[blockIndex] = block;
      }

      return draft;
    }
    case 'ADD_BLOCK_TO_SECTION': {
      const { block, meta } = action.payload;
      const { sectionId } = meta;
      if (!draft.pageContent.content) {
        throw new Error(
          `${action.type}: Error - Section "content" could not be found.`,
        );
      }
      const matchingSection = draft.pageContent.content.find(
        section => section.id === sectionId,
      );
      if (!matchingSection) return draft;
      if (Array.isArray(matchingSection.content)) {
        matchingSection.content.push(block);
      } else {
        matchingSection.content = [block];
      }

      return draft;
    }
    case 'UPDATE_SECTION_CONTENT': {
      const { sectionContent, meta } = action.payload;
      const { sectionId } = meta;
      if (!draft.pageContent.content) {
        throw new Error(
          `${action.type}: Error - Section "content" could not be found.`,
        );
      }

      const matchingSection = draft.pageContent.content.find(
        section => section.id === sectionId,
      );
      if (matchingSection) matchingSection.content = sectionContent;

      return draft;
    }
    case 'ADD_CONTENT_SECTION': {
      const { section } = action.payload;
      if (draft.pageContent) draft.pageContent.content.push(section);
      return draft;
    }
    case 'SET_CONTENT': {
      const { content } = action.payload;
      if (draft.pageContent) {
        draft.pageContent.content = content;
      }
      return draft;
    }
    case 'UPDATE_CONTENT_SECTION': {
      const { section, meta } = action.payload;
      const { sectionId } = meta;

      if (draft.pageContent) {
        const sectionIndex = draft.pageContent.content.findIndex(
          accordionSection => accordionSection.id === sectionId,
        );
        if (sectionIndex !== -1)
          draft.pageContent.content[sectionIndex] = section;
      }
      return draft;
    }
    case 'ADD_FREE_TEXT_STAT_TILE_TO_BLOCK': {
      const { tile, meta } = action.payload;
      const { blockId, sectionId } = meta;
      if (!draft.pageContent.content) {
        throw new Error(
          `${action.type}: Error - Section "content" could not be found.`,
        );
      }
      const matchingSection = draft.pageContent.content.find(
        section => section.id === sectionId,
      );
      if (!matchingSection) return draft;
      const matchingBlock = matchingSection.content.find(
        block => block.id === blockId,
      );
      if (!matchingBlock || matchingBlock.type !== 'TileGroupBlock') {
        return draft;
      }
      if (Array.isArray(matchingBlock.tiles)) {
        matchingBlock.tiles.push(tile);
      } else {
        matchingBlock.tiles = [tile];
      }
      return draft;
    }
    case 'UPDATE_FREE_TEXT_STAT_TILE_IN_BLOCK': {
      const { tile: newTile, meta } = action.payload;
      const { blockId, sectionId, tileId } = meta;
      if (!draft.pageContent.content) {
        throw new Error(
          `${action.type}: Error - Section "content" could not be found.`,
        );
      }
      const matchingSection = draft.pageContent.content.find(
        section => section.id === sectionId,
      );
      if (!matchingSection) return draft;
      const matchingBlock = matchingSection.content.find(
        block => block.id === blockId,
      );
      if (matchingBlock?.type !== 'TileGroupBlock' || !matchingBlock.tiles) {
        return draft;
      }
      matchingBlock.tiles = matchingBlock.tiles.map(tileItem =>
        tileItem.id === tileId ? newTile : tileItem,
      );
      return draft;
    }
    case 'DELETE_FREE_TEXT_STAT_TILE_FROM_BLOCK': {
      const { meta } = action.payload;
      const { blockId, sectionId, tileId } = meta;
      if (!draft.pageContent.content) {
        throw new Error(
          `${action.type}: Error - Section "content" could not be found.`,
        );
      }
      const matchingSection = draft.pageContent.content.find(
        section => section.id === sectionId,
      );
      if (!matchingSection) return draft;
      const matchingBlock = matchingSection.content.find(
        block => block.id === blockId,
      );
      if (matchingBlock?.type !== 'TileGroupBlock' || !matchingBlock.tiles) {
        return draft;
      }
      matchingBlock.tiles = matchingBlock.tiles.filter(
        tileItem => tileItem.id !== tileId,
      );
      return draft;
    }
    default: {
      return draft;
    }
  }
};

interface EducationInNumbersPageContentProviderProps {
  children: ReactNode;
  value: EducationInNumbersPageContextState;
}

function EducationInNumbersPageContentProvider({
  children,
  value,
}: EducationInNumbersPageContentProviderProps) {
  const [state, dispatch] = useLoggedImmerReducer(
    'EducationInNumbersPage',
    educationInNumbersPageReducer,
    value,
  );

  return (
    <EducationInNumbersPageStateContext.Provider value={state}>
      <EducationInNumbersPageDispatchContext.Provider value={dispatch}>
        {children}
      </EducationInNumbersPageDispatchContext.Provider>
    </EducationInNumbersPageStateContext.Provider>
  );
}

function useEducationInNumbersPageContentState() {
  const context = useContext(EducationInNumbersPageStateContext);
  if (context === undefined) {
    throw new Error(
      'useEducationInNumbersPageContentState must be used within a EducationInNumbersPageContentProvider',
    );
  }
  return context;
}

function useEducationInNumbersPageContentDispatch() {
  const context = useContext(EducationInNumbersPageDispatchContext);
  if (context === undefined) {
    throw new Error(
      'useEducationInNumbersPageContentDispatch must be used within a EducationInNumbersPageContentProvider',
    );
  }
  return context;
}

export {
  EducationInNumbersPageContentProvider,
  useEducationInNumbersPageContentState,
  useEducationInNumbersPageContentDispatch,
};
