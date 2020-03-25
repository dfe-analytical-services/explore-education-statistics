import { MethodologyContent } from '@admin/services/methodology/types';
import remove from 'lodash/remove';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer, useImmerReducer } from 'use-immer';
import { MethodologyDispatchAction } from './MethodologyContextActionTypes';

export type MethodologyContextDispatch = (
  action: MethodologyDispatchAction,
) => void;

export type MethodologyContextState = {
  methodology: MethodologyContent | undefined;
  canUpdateMethodology: boolean;
};
type MethodologyProviderProps = {
  methodology: MethodologyContent;
  children: ReactNode;
};

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
        throw new Error(
          `REMOVE_BLOCK_FROM_SECTION: Error - Section "${sectionKey}" could not be found.`,
        );
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
        throw new Error(
          `UPDATE_BLOCK_FROM_SECTION: Error - Section "${sectionKey}" could not be found.`,
        );
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
        throw new Error(
          `ADD_BLOCK_TO_SECTION: Error - Section "${sectionKey}" could not be found.`,
        );
      }
      const matchingSection = draft.methodology[sectionKey].find(
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
      const { sectionId, sectionKey } = meta;
      if (!draft.methodology?.[sectionKey]) {
        throw new Error(
          `UPDATE_SECTION_CONTENT: Error - Section "${sectionKey}" could not be found.`,
        );
      }

      const matchingSection = draft.methodology[sectionKey].find(
        section => section.id === sectionId,
      );
      if (matchingSection) matchingSection.content = sectionContent;

      return draft;
    }
    case 'ADD_CONTENT_SECTION': {
      const { section, sectionKey } = action.payload;
      if (draft.methodology) draft.methodology[sectionKey].push(section);
      return draft;
    }
    case 'SET_CONTENT': {
      const { content, sectionKey } = action.payload;
      if (draft.methodology) {
        draft.methodology[sectionKey] = content;
      }
      return draft;
    }
    case 'UPDATE_CONTENT_SECTION': {
      const { section, meta } = action.payload;
      const { sectionId, sectionKey } = meta;

      if (draft.methodology) {
        const sectionIndex = draft.methodology[sectionKey].findIndex(
          accordionSection => accordionSection.id === sectionId,
        );
        if (sectionIndex !== -1)
          draft.methodology[sectionKey][sectionIndex] = section;
      }
      return draft;
    }
    default: {
      return draft;
    }
  }
};

function MethodologyProvider({
  methodology,
  children,
}: MethodologyProviderProps) {
  const [state, dispatch] = useImmerReducer(methodologyReducer, {
    methodology,
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

export { MethodologyProvider, useMethodologyState, useMethodologyDispatch };
