import { CurrentInteraction } from '@admin/contexts/comments/CommentsContext';
import { Comment } from '@admin/services/types/content';

export type AddComment = {
  type: 'ADD_COMMENT';
  payload: {
    comment: Comment;
  };
};
export type DeleteComment = {
  type: 'DELETE_COMMENT';
  payload: {
    id: string;
  };
};
export type ResetPendingDeletion = {
  type: 'RESET_PENDING_DELETION';
};
export type SetCurrentInteraction = {
  type: 'SET_CURRENT_INTERACTION';
  payload: CurrentInteraction;
};
export type UndeleteComment = {
  type: 'UNDELETE_COMMENT';
  payload: {
    id: string;
  };
};
export type UpdateComment = {
  type: 'UPDATE_COMMENT';
  payload: {
    comment: Comment;
  };
};
export type CommentsDispatchAction =
  | AddComment
  | DeleteComment
  | ResetPendingDeletion
  | SetCurrentInteraction
  | UndeleteComment
  | UpdateComment;
