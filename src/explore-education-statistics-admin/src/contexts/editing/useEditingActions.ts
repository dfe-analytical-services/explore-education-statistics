import {
  EditingMode,
  useEditingDispatch,
} from '@admin/contexts/editing/EditingContext';
import { useCallback, useMemo } from 'react';

export default function useCommentsActions() {
  const dispatch = useEditingDispatch();

  const addUnsavedBlock = useCallback(
    async (blockId: string) => {
      dispatch?.({
        type: 'ADD_UNSAVED_BLOCK',
        payload: {
          blockId,
        },
      });
    },
    [dispatch],
  );

  const removeUnsavedBlock = useCallback(
    async (blockId: string) => {
      dispatch?.({
        type: 'REMOVE_UNSAVED_BLOCK',
        payload: {
          blockId,
        },
      });
    },
    [dispatch],
  );

  const setEditingMode = useCallback(
    async (editingMode: EditingMode) => {
      dispatch?.({
        type: 'SET_EDITING_MODE',
        payload: { editingMode },
      });
    },
    [dispatch],
  );

  const updateUnresolvedComments = useCallback(
    async (blockId: string, commentId: string) => {
      dispatch?.({
        type: 'UPDATE_UNRESOLVED_COMMENTS',
        payload: {
          blockId,
          commentId,
        },
      });
    },
    [dispatch],
  );

  const updateUnsavedCommentDeletions = useCallback(
    async (blockId: string, commentId: string) => {
      dispatch?.({
        type: 'UPDATE_UNSAVED_COMMENT_DELETIONS',
        payload: {
          blockId,
          commentId,
        },
      });
    },
    [dispatch],
  );

  return useMemo(
    () => ({
      addUnsavedBlock,
      removeUnsavedBlock,
      setEditingMode,
      updateUnresolvedComments,
      updateUnsavedCommentDeletions,
    }),
    [
      addUnsavedBlock,
      removeUnsavedBlock,
      setEditingMode,
      updateUnresolvedComments,
      updateUnsavedCommentDeletions,
    ],
  );
}
