import {
  EditableContentBlock,
  EditableRelease,
  ExtendedComment,
} from '@admin/services/publicationService';
import { DataBlock } from '@common/services/dataBlockService';
import { ContentSection } from '@common/services/publicationService';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer, useImmerReducer } from 'use-immer';
import remove from 'lodash/remove';
import ReleaseDispatchAction from './actions';

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
  console.log(action.type);
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
        draft.release[sectionKey] = draft.release[sectionKey].map(section => {
          if (section.id === sectionId) {
            return {
              ...section,
              content: section.content
                ? [
                    ...section.content,
                    { ...block, comments: block.comments || [] },
                  ]
                : [{ ...block, comments: block.comments || [] }],
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
      return draft;
    }
    case 'UPDATE_SECTION_CONTENT': {
      const { sectionContent, meta } = action.payload;
      const { sectionId, sectionKey } = meta;
      if (!draft.release?.[sectionKey]) {
        throw new Error('ADD_BLOCK_TO_SECTION: failed');
      }
      if (sectionKey === 'content') {
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
        draft.release.content = draft.release.content.map(accordionSection => {
          if (accordionSection.id === sectionId) {
            return section;
          }
          return accordionSection;
        });
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
  //eslint-disable-next-line no-undef
  ReleaseContextState,
  //eslint-disable-next-line no-undef
  ReleaseContextDispatch,
  ReleaseProvider,
  useReleaseState,
  useReleaseDispatch,
};
