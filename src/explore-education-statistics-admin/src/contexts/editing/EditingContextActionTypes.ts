import { EditingMode } from '@admin/contexts/editing/EditingContext';

export type AddUnsavedBlock = {
  type: 'ADD_UNSAVED_BLOCK';
  payload: {
    blockId: string;
  };
};
export type RemoveUnsavedDeletionsForBlock = {
  type: 'REMOVE_UNSAVED_DELETIONS_FOR_BLOCK';
  payload: {
    blockId: string;
  };
};
export type RemoveUnsavedBlock = {
  type: 'REMOVE_UNSAVED_BLOCK';
  payload: {
    blockId: string;
  };
};
export type SetEditingMode = {
  type: 'SET_EDITING_MODE';
  payload: {
    editingMode: EditingMode;
  };
};
export type UpdateUnresolvedComments = {
  type: 'UPDATE_UNRESOLVED_COMMENTS';
  payload: {
    blockId: string;
    commentId: string;
  };
};
export type UpdateUnsavedCommentDeletions = {
  type: 'UPDATE_UNSAVED_COMMENT_DELETIONS';
  payload: {
    blockId: string;
    commentId: string;
  };
};

export type EditingDispatchAction =
  | AddUnsavedBlock
  | RemoveUnsavedDeletionsForBlock
  | RemoveUnsavedBlock
  | SetEditingMode
  | UpdateUnresolvedComments
  | UpdateUnsavedCommentDeletions;
