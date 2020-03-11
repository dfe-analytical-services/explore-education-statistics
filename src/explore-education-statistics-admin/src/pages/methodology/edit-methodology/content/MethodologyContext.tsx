import { ExtendedComment } from '@admin/services/publicationService';
import { DataBlock } from '@common/services/dataBlockService';
import remove from 'lodash/remove';
import React, { createContext, ReactNode, useContext } from 'react';
import { MethodologyContent } from 'src/services/methodology/types';
import { Reducer, useImmerReducer } from 'use-immer';
import MethodologyDispatchAction from './MethodologyContextActionTypes';

type MethodologyContextDispatch = (action: MethodologyDispatchAction) => void;
type MethodologyContextState = {
  methodology: MethodologyContent | undefined;
  canUpdateMethodology: boolean;
};
type MethodologyProviderProps = { children: ReactNode };

const MethodologyStateContext = createContext<
  MethodologyContextState | undefined
>(undefined);
const MethodologyDispatchContext = createContext<
  MethodologyContextDispatch | undefined
>(undefined);

export const methodologyReducer: Reducer<
  MethodologyContextState,
  MethodologyDispatchAction
> = (draft, action) => {
  switch (action.type) {
    case 'CLEAR_STATE': {
      return {
        methodology: undefined,
        canUpdateMethodology: false,
      };
    }
    case 'SET_STATE': {
      return { ...draft, ...action.payload };
    }
    case 'REMOVE_BLOCK_FROM_SECTION': {
      const { sectionId, blockId, sectionKey } = action.payload.meta;
      if (!draft.methodology?.[sectionKey]) {
        throw new Error('REMOVE_BLOCK_FROM_SECTION: failed');
      }

      const matchingSection = draft.methodology[sectionKey].find(
        section => section.id === sectionId,
      );
      if (matchingSection?.content) {
        remove(matchingSection.content, content => content.id === blockId);
      }

      return draft;
    }
    case 'UPDATE_BLOCK_FROM_SECTION': {
      const { block, meta } = action.payload;
      const { sectionId, blockId, sectionKey } = meta;
      if (!draft.methodology?.[sectionKey]) {
        throw new Error('UPDATE_BLOCK_FROM_SECTION: failed');
      }
      const matchingSection = draft.methodology[sectionKey].find(
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
      const { sectionId, sectionKey } = meta;
      if (!draft.methodology?.[sectionKey]) {
        throw new Error('ADD_BLOCK_TO_SECTION: failed');
      }

      // .comments needs initialising to array as will be undefined if empty
      const newBlock = { ...block, comments: block.comments || [] };
      const matchingSection = draft.methodology[sectionKey].find(
        section => section.id === sectionId,
      );
      if (!matchingSection) return draft;
      if (Array.isArray(matchingSection.content)) {
        matchingSection.content.push(newBlock);
      } else {
        matchingSection.content = [newBlock];
      }

      return draft;
    }
    case 'UPDATE_SECTION_CONTENT': {
      const { sectionContent, meta } = action.payload;
      const { sectionId, sectionKey } = meta;
      if (!draft.methodology?.[sectionKey]) {
        throw new Error('ADD_BLOCK_TO_SECTION: failed');
      }

      const matchingSection = draft.methodology[sectionKey].find(
        section => section.id === sectionId,
      );
      if (matchingSection) matchingSection.content = sectionContent;

      return draft;
    }
    case 'ADD_CONTENT_SECTION': {
      const { section } = action.payload;
      if (draft.methodology) draft.methodology.content.push(section);
      return draft;
    }
    case 'SET_CONTENT': {
      const { content } = action.payload;
      if (draft.methodology) {
        draft.methodology.content = content;
      }
      return draft;
    }
    case 'UPDATE_CONTENT_SECTION': {
      const { section, meta } = action.payload;
      const { sectionId } = meta;

      if (draft.methodology) {
        const sectionIndex = draft.methodology.content.findIndex(
          accordionSection => accordionSection.id === sectionId,
        );
        if (sectionIndex !== -1)
          draft.methodology.content[sectionIndex] = section;
      }
      return draft;
    }
    default: {
      return draft;
    }
  }
};

function MethodologyProvider({ children }: MethodologyProviderProps) {
  const [state, dispatch] = useImmerReducer(methodologyReducer, {
    methodology: undefined,
    canUpdateMethodology: false,
  });
  return (
    <MethodologyStateContext.Provider value={state}>
      <MethodologyDispatchContext.Provider value={dispatch}>
        {children}
      </MethodologyDispatchContext.Provider>
    </MethodologyStateContext.Provider>
  );
}

function useMethodologyState() {
  const context = useContext(MethodologyStateContext);
  if (context === undefined) {
    throw new Error(
      'useMethodologyState must be used within a MethodologyProvider',
    );
  }
  return context;
}

function useMethodologyDispatch() {
  const context = useContext(MethodologyDispatchContext);
  if (context === undefined) {
    throw new Error(
      'useMethodologyDispatch must be used within a MethodologyProvider',
    );
  }
  return context;
}

export {
  MethodologyProvider,
  useMethodologyState,
  useMethodologyDispatch,
  // eslint-disable-next-line no-undef
  MethodologyContextState,
  // eslint-disable-next-line no-undef
  MethodologyContextDispatch,
};
