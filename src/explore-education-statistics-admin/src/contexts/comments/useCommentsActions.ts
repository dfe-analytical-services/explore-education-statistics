import { useCommentsDispatch } from '@admin/contexts/comments/CommentsContext';
import { Comment } from '@admin/services/types/content';
import { useCallback, useMemo } from 'react';

export default function useCommentsActions() {
  const dispatch = useCommentsDispatch();

  const addComment = useCallback(
    async (comment: Comment) => {
      dispatch?.({
        type: 'ADD_COMMENT',
        payload: { comment },
      });

      dispatch?.({
        type: 'SET_CURRENT_INTERACTION',
        payload: { type: 'adding', id: comment.id },
      });
    },
    [dispatch],
  );

  const deleteComment = useCallback(
    async id => {
      dispatch?.({
        type: 'DELETE_COMMENT',
        payload: { id },
      });
    },
    [dispatch],
  );

  const resetPendingDeletion = useCallback(async () => {
    dispatch?.({
      type: 'RESET_PENDING_DELETION',
    });
  }, [dispatch]);

  const setCurrentInteraction = useCallback(
    async values => {
      dispatch?.({
        type: 'SET_CURRENT_INTERACTION',
        payload: values ? { type: values.type, id: values.id } : undefined,
      });
    },
    [dispatch],
  );

  const undeleteComment = useCallback(
    async id => {
      dispatch?.({
        type: 'UNDELETE_COMMENT',
        payload: { id },
      });
    },
    [dispatch],
  );

  const updateComment = useCallback(
    async (comment: Comment) => {
      dispatch?.({
        type: 'UPDATE_COMMENT',
        payload: { comment },
      });
    },
    [dispatch],
  );

  return useMemo(
    () => ({
      addComment,
      deleteComment,
      resetPendingDeletion,
      setCurrentInteraction,
      undeleteComment,
      updateComment,
    }),
    [
      addComment,
      deleteComment,
      resetPendingDeletion,
      setCurrentInteraction,
      undeleteComment,
      updateComment,
    ],
  );
}
