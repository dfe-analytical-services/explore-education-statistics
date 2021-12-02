import { CommentsDispatchAction } from '@admin/contexts/comments/CommentsContextActionTypes';
import { Comment } from '@admin/services/types/content';
import {
  AddComment,
  UpdateComment,
} from '@admin/services/releaseContentCommentService';
import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import React, { createContext, ReactNode, useContext } from 'react';
import { Reducer } from 'use-immer';

export type CurrentInteraction =
  | { type: 'adding' | 'removing' | 'resolving' | 'unresolving'; id: string }
  | undefined;
export type CommentsContextDispatch = (action: CommentsDispatchAction) => void;
export type CommentsContextState = {
  comments: Comment[];
  currentInteraction?: CurrentInteraction;
  pendingDeletions: Comment[];
  onAddComment?: (comment: AddComment) => Promise<Comment>;
  onDeletePendingComments?: (pendingDeletions: Comment[]) => void;
  onToggleResolveComment?: (comment: UpdateComment) => Promise<Comment>;
  onUpdateComment?: (comment: UpdateComment) => Promise<Comment>;
};

const CommentsDispatchContext = createContext<
  CommentsContextDispatch | undefined
>(undefined);

export const CommentsStateContext = createContext<CommentsContextState>({
  comments: [],
  currentInteraction: undefined,
  pendingDeletions: [],
});

export const commentsReducer: Reducer<
  CommentsContextState,
  CommentsDispatchAction
> = (draft, action) => {
  switch (action.type) {
    case 'ADD_COMMENT': {
      draft.comments.push(action.payload.comment);
      return draft;
    }
    case 'DELETE_COMMENT': {
      const index = draft.comments.findIndex(
        comment => comment.id === action.payload.id,
      );
      if (index !== -1) {
        draft.pendingDeletions.push(draft.comments[index]);
        draft.comments.splice(index, 1);
      }
      return draft;
    }
    case 'RESET_PENDING_DELETION': {
      draft.pendingDeletions = [];
      return draft;
    }
    case 'SET_CURRENT_INTERACTION': {
      draft.currentInteraction = action.payload ?? undefined;
      return draft;
    }
    case 'UNDELETE_COMMENT': {
      const index = draft.pendingDeletions.findIndex(
        comment => comment.id === action.payload.id,
      );
      if (index !== -1) {
        draft.comments.push(draft.pendingDeletions[index]);
        draft.pendingDeletions.splice(index, 1);
      }
      return draft;
    }
    case 'UPDATE_COMMENT': {
      const index = draft.comments.findIndex(
        comment => comment.id === action.payload.comment.id,
      );
      if (index === -1) {
        return draft;
      }
      draft.comments[index] = action.payload.comment;
      return draft;
    }
    default: {
      return draft;
    }
  }
};

export interface CommentsContextProviderProps {
  children: ReactNode | ((state: CommentsContextState) => ReactNode);
  value: CommentsContextState;
}

export const CommentsProvider = ({
  children,
  value,
}: CommentsContextProviderProps) => {
  const [state, dispatch] = useLoggedImmerReducer(
    'Comments',
    commentsReducer,
    value,
  );

  return (
    <CommentsStateContext.Provider value={state}>
      <CommentsDispatchContext.Provider value={dispatch}>
        {children}
      </CommentsDispatchContext.Provider>
    </CommentsStateContext.Provider>
  );
};

export function useCommentsContext() {
  return useContext(CommentsStateContext);
}

export function useCommentsDispatch() {
  const context = useContext(CommentsDispatchContext);
  return context;
}
