import { MethodologyContent } from '@admin/services/methodology/types';
import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import remove from 'lodash/remove';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer } from 'use-immer';
import { MethodologyDispatchAction } from './MethodologyContextActionTypes';

export type MethodologyContextDispatch = (
  action: MethodologyDispatchAction,
) => void;

export type MethodologyContextState = {
  methodology: MethodologyContent;
  canUpdateMethodology: boolean;
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
    case 'REMOVE_BLOCK_FROM_SECTION': {
      const { sectionId, blockId, sectionKey } = action.payload.meta;
      if (!draft.methodology[sectionKey]) {
        throw new Error(
          `${action.type}: Error - Section "${sectionKey}" could not be found.`,
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
      if (!draft.methodology[sectionKey]) {
        throw new Error(
          `${action.type}: Error - Section "${sectionKey}" could not be found.`,
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
      if (!draft.methodology[sectionKey]) {
        throw new Error(
          `${action.type}: Error - Section "${sectionKey}" could not be found.`,
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
      if (!draft.methodology[sectionKey]) {
        throw new Error(
          `${action.type}: Error - Section "${sectionKey}" could not be found.`,
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

interface MethodologyProviderProps {
  children: ReactNode;
  value: MethodologyContextState;
}

function MethodologyProvider({ children, value }: MethodologyProviderProps) {
  const [state, dispatch] = useLoggedImmerReducer(
    'Methodology',
    methodologyReducer,
    value,
  );

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
