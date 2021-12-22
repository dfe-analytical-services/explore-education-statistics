import { Comment } from '@admin/services/types/content';
import { AddComment } from '@admin/services/releaseContentCommentService';
import useCallbackRef from '@common/hooks/useCallbackRef';
import React, {
  createContext,
  ReactNode,
  useContext,
  useMemo,
  useState,
  MutableRefObject,
} from 'react';
import noop from 'lodash/noop';

export type CurrentCommentInteraction =
  | { type: 'adding' | 'removing' | 'resolving' | 'unresolving'; id: string }
  | undefined;
export interface SelectedComment {
  id: string;
  fromEditor?: boolean;
}

export interface CommentsContextState {
  addComment: (blockId: string, comment: AddComment) => Promise<Comment | null>;
  comments: Comment[];
  currentInteraction: CurrentCommentInteraction;
  clearPendingDeletions: () => void;
  markersOrder: string[];
  pendingDeletions: Comment[];
  reAddComment: MutableRefObject<(blockId: string, commentId: string) => void>;
  removeComment: MutableRefObject<(blockId: string, commentId: string) => void>;
  resolveComment: MutableRefObject<
    (blockId: string, commentId: string, updateMarker?: boolean) => void
  >;
  selectedComment: SelectedComment;
  setCurrentInteraction: (
    currentInteraction: CurrentCommentInteraction,
  ) => void;
  setMarkersOrder: (order: string[]) => void;
  setSelectedComment: (selectedComment: SelectedComment) => void;
  unresolveComment: MutableRefObject<
    (blockId: string, commentId: string, updateMarker?: boolean) => void
  >;
  updateComment: (comment: Comment) => void;
}

export const CommentsContext = createContext<CommentsContextState>({
  addComment: () => Promise.resolve(null),
  comments: [],
  currentInteraction: undefined,
  clearPendingDeletions: noop,
  markersOrder: [],
  pendingDeletions: [],
  reAddComment: { current: noop },
  removeComment: { current: noop },
  resolveComment: { current: noop },
  selectedComment: { id: '' },
  setCurrentInteraction: noop,
  setMarkersOrder: noop,
  setSelectedComment: noop,
  unresolveComment: { current: noop },
  updateComment: noop,
});

export interface CommentsContextProviderProps {
  children: ReactNode;
  comments: Comment[];
  markersOrder?: string[];
  pendingDeletions?: Comment[];
  onDeleteComment: (commentId: string) => Promise<void>;
  onSaveComment: (comment: AddComment) => Promise<Comment>;
  onSaveUpdatedComment: (comment: Comment) => Promise<Comment>;
  onUpdateUnresolvedComments: MutableRefObject<
    (blockId: string, commentId: string) => void
  >;
  onUpdateUnsavedCommentDeletions: MutableRefObject<
    (blockId: string, commentId: string) => void
  >;
}

export const CommentsContextProvider = ({
  children,
  comments: initialComments,
  markersOrder: initialMarkersOrder = [],
  pendingDeletions: initialPendingDeletions = [],
  onDeleteComment,
  onSaveComment,
  onSaveUpdatedComment,
  onUpdateUnresolvedComments,
  onUpdateUnsavedCommentDeletions,
}: CommentsContextProviderProps) => {
  const [comments, setComments] = useState<Comment[]>(initialComments);
  const [currentInteraction, setCurrentInteraction] = useState<
    CurrentCommentInteraction
  >();
  const [markersOrder, setMarkersOrder] = useState<string[]>(
    initialMarkersOrder,
  );
  const [pendingDeletions, setPendingDeletions] = useState<Comment[]>(
    initialPendingDeletions,
  );
  const [selectedComment, setSelectedComment] = useState<SelectedComment>({
    id: '',
    fromEditor: false,
  });

  // Use useCallbackRef for these to avoid the problem of stale references when they're called from the editor undo/redo
  const removeComment: CommentsContextState['removeComment'] = useCallbackRef(
    (blockId, commentId) => {
      const commentToDelete = comments.find(
        comment => comment.id === commentId,
      );
      if (commentToDelete) {
        setComments(comments.filter(comment => comment.id !== commentId));

        setPendingDeletions([...pendingDeletions, commentToDelete]);

        setCurrentInteraction({
          type: 'removing',
          id: commentId,
        });

        onUpdateUnsavedCommentDeletions.current(blockId, commentId);
      }
    },
    [comments, pendingDeletions],
  );

  const resolveComment: CommentsContextState['resolveComment'] = useCallbackRef(
    async (blockId, commentId, updateMarker) => {
      const comment = comments.find(c => c.id === commentId);
      if (!comment) {
        return;
      }

      const updatedComment = await onSaveUpdatedComment({
        ...comment,
        setResolved: true,
      });
      const index = comments.findIndex(c => c.id === updatedComment.id);
      if (index !== -1) {
        const updatedComments = [...comments];
        updatedComments[index] = updatedComment;
        setComments(updatedComments);
        onUpdateUnresolvedComments.current(blockId, comment.id);
      }

      if (updateMarker) {
        setCurrentInteraction({
          type: 'resolving',
          id: comment.id,
        });
      }
    },
    [comments],
  );

  const reAddComment: CommentsContextState['reAddComment'] = useCallbackRef(
    (blockId, commentId) => {
      const commentToUndelete = pendingDeletions.find(
        comment => comment.id === commentId,
      );
      if (commentToUndelete) {
        setPendingDeletions(
          pendingDeletions.filter(deletion => deletion.id !== commentId),
        );
        setComments([...comments, commentToUndelete]);
        onUpdateUnsavedCommentDeletions.current(blockId, commentId);
      }
    },
    [comments, pendingDeletions],
  );

  const unresolveComment: CommentsContextState['unresolveComment'] = useCallbackRef(
    async (blockId, commentId, updateMarker) => {
      const comment = comments.find(c => c.id === commentId);
      if (!comment) {
        return;
      }

      const updatedComment = await onSaveUpdatedComment({
        ...comment,
        setResolved: false,
      });
      const index = comments.findIndex(c => c.id === updatedComment.id);
      if (index !== -1) {
        const updatedComments = [...comments];
        updatedComments[index] = updatedComment;
        setComments(updatedComments);
        onUpdateUnresolvedComments.current(blockId, comment.id);
      }

      if (updateMarker) {
        setCurrentInteraction({
          type: 'unresolving',
          id: comment.id,
        });
      }
    },
    [comments],
  );

  const state = useMemo<CommentsContextState>(() => {
    const addComment: CommentsContextState['addComment'] = async (
      blockId,
      comment,
    ) => {
      const newComment = await onSaveComment(comment);
      setComments(currentComments => [...currentComments, newComment]);
      setCurrentInteraction({ type: 'adding', id: newComment.id });
      onUpdateUnresolvedComments.current(blockId, newComment.id);
      return newComment;
    };

    const clearPendingDeletions = async () => {
      await Promise.all(
        pendingDeletions.map(deletion => onDeleteComment(deletion.id)),
      );
      setPendingDeletions([]);
    };

    const updateComment: CommentsContextState['updateComment'] = async comment => {
      const updatedComment = await onSaveUpdatedComment(comment);

      setComments(currentComments => {
        const index = currentComments.findIndex(
          c => c.id === updatedComment.id,
        );
        if (index === -1) {
          return currentComments;
        }
        const updatedComments = [...comments];
        updatedComments.splice(index, 1, updatedComment);
        return updatedComments;
      });
    };

    return {
      addComment,
      comments,
      currentInteraction,
      clearPendingDeletions,
      markersOrder,
      pendingDeletions,
      reAddComment,
      removeComment,
      resolveComment,
      selectedComment,
      setCurrentInteraction,
      setMarkersOrder,
      setSelectedComment,
      unresolveComment,
      updateComment,
    };
  }, [
    comments,
    currentInteraction,
    markersOrder,
    pendingDeletions,
    reAddComment,
    removeComment,
    resolveComment,
    selectedComment,
    unresolveComment,
    onDeleteComment,
    onSaveComment,
    onSaveUpdatedComment,
    onUpdateUnresolvedComments,
  ]);

  return (
    <CommentsContext.Provider value={state}>
      {children}
    </CommentsContext.Provider>
  );
};

export function useCommentsContext() {
  return useContext(CommentsContext);
}
