import { EditingDispatchAction } from '@admin/contexts/editing/EditingContextActionTypes';
import { Dictionary } from '@admin/types';
import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer } from 'use-immer';

export type BlockCommentIds = Dictionary<string[]>;
export type EditingMode = 'preview' | 'table-preview' | 'edit';

export type EditingContextDispatch = (action: EditingDispatchAction) => void;
export type EditingContextState = {
  editingMode: EditingMode;
  unresolvedComments: BlockCommentIds;
  unsavedCommentDeletions: BlockCommentIds;
  unsavedBlocks: string[];
};
export const EditingStateContext = createContext<EditingContextState>({
  editingMode: 'preview',
  unresolvedComments: {},
  unsavedCommentDeletions: {},
  unsavedBlocks: [],
});

const EditingDispatchContext = createContext<
  EditingContextDispatch | undefined
>(undefined);

export const editingReducer: Reducer<
  EditingContextState,
  EditingDispatchAction
> = (draft, action) => {
  switch (action.type) {
    case 'ADD_UNSAVED_BLOCK': {
      const { blockId } = action.payload;
      if (!draft.unsavedBlocks.includes(blockId)) {
        draft.unsavedBlocks.push(blockId);
      }
      return draft;
    }
    case 'REMOVE_UNSAVED_BLOCK': {
      const { blockId } = action.payload;
      const index = draft.unsavedBlocks.findIndex(block => block === blockId);
      if (index !== -1) {
        draft.unsavedBlocks.splice(index, 1);
      }
      return draft;
    }
    case 'SET_EDITING_MODE': {
      draft.editingMode = action.payload.editingMode;
      return draft;
    }
    case 'UPDATE_UNRESOLVED_COMMENTS': {
      const { blockId, commentId } = action.payload;
      if (!draft.unresolvedComments[blockId]) {
        draft.unresolvedComments[blockId] = [commentId];
        return draft;
      }
      const index = draft.unresolvedComments[blockId].indexOf(commentId);
      if (index !== -1) {
        draft.unresolvedComments[blockId].splice(index, 1);
      } else {
        draft.unresolvedComments[blockId].push(commentId);
      }
      return draft;
    }
    case 'UPDATE_UNSAVED_COMMENT_DELETIONS': {
      const { blockId, commentId } = action.payload;
      if (!draft.unsavedCommentDeletions[blockId]) {
        draft.unsavedCommentDeletions[blockId] = [commentId];
        return draft;
      }
      const index = draft.unsavedCommentDeletions[blockId].indexOf(commentId);
      if (index !== -1) {
        draft.unsavedCommentDeletions[blockId].splice(index, 1);
      } else {
        draft.unsavedCommentDeletions[blockId].push(commentId);
      }
      return draft;
    }
    default:
      return draft;
  }
};

export interface EditingContextProviderProps {
  children: ReactNode | ((state: EditingContextState) => ReactNode);
  value: EditingContextState;
}

export const EditingProvider = ({
  children,
  value,
}: EditingContextProviderProps) => {
  const [state, dispatch] = useLoggedImmerReducer(
    'Editing',
    editingReducer,
    value,
  );

  return (
    <EditingStateContext.Provider value={state}>
      <EditingDispatchContext.Provider value={dispatch}>
        {typeof children === 'function' ? children(state) : children}
      </EditingDispatchContext.Provider>
    </EditingStateContext.Provider>
  );
};

export function useEditingContext() {
  return useContext(EditingStateContext);
}

export function useEditingDispatch() {
  const context = useContext(EditingDispatchContext);
  return context;
}
