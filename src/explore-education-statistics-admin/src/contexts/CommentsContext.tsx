import { Comment } from '@admin/services/types/content';
import {
  AddComment,
  UpdateComment,
} from '@admin/services/releaseContentCommentService';
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

export type CurrentInteraction =
  | { type: 'adding' | 'removing' | 'resolving' | 'unresolving'; id: string }
  | undefined;
export interface SelectedComment {
  commentId: string;
  fromEditor?: boolean;
}

export type CommentsContextState = {
  comments: Comment[];
  currentInteraction: CurrentInteraction;
  markersOrder: string[];
  pendingDeletions: Comment[];
  selectedComment: SelectedComment;
  addComment: (comment: AddComment) => Promise<Comment | null>;
  removeComment: MutableRefObject<(commentId: string) => void>;
  deletePendingComments: () => void;
  resolveComment: MutableRefObject<
    (commentId: string, updateMarker?: boolean) => void
  >;
  reAddComment: MutableRefObject<(commentId: string) => void>;
  unresolveComment: MutableRefObject<
    (commentId: string, updateMarker?: boolean) => void
  >;
  updateComment: (comment: UpdateComment) => void;
  setCurrentInteraction: (currentInteraction: CurrentInteraction) => void;
  setMarkersOrder: (order: string[]) => void;
  setSelectedComment: (selectedComment: SelectedComment) => void;
};

export type CommentsContextInitialValues = {
  comments: Comment[];
  markersOrder?: string[];
  pendingDeletions?: Comment[];
  onDeletePendingComment: (commentId: string) => Promise<void>;
  onSaveComment: (comment: AddComment) => Promise<Comment>;
  onSaveUpdatedComment: (comment: UpdateComment) => Promise<Comment>;
};

export const CommentsStateContext = createContext<CommentsContextState>({
  addComment: () => Promise.resolve(null),
  comments: [],
  currentInteraction: undefined,
  deletePendingComments: noop,
  markersOrder: [],
  pendingDeletions: [],
  reAddComment: { current: noop },
  removeComment: { current: noop },
  resolveComment: { current: noop },
  selectedComment: { commentId: '' },
  setCurrentInteraction: noop,
  setMarkersOrder: noop,
  setSelectedComment: noop,
  unresolveComment: { current: noop },
  updateComment: noop,
});

export interface CommentsContextProviderProps {
  children: ReactNode;
  value: CommentsContextInitialValues;
}

export const CommentsProvider = ({
  children,
  value,
}: CommentsContextProviderProps) => {
  const [comments, setComments] = useState<Comment[]>(value.comments);
  const [currentInteraction, setCurrentInteraction] = useState<
    CurrentInteraction
  >();
  const [markersOrder, setMarkersOrder] = useState<string[]>(
    value.markersOrder ?? [],
  );
  const [pendingDeletions, setPendingDeletions] = useState<Comment[]>(
    value.pendingDeletions ?? [],
  );
  const [selectedComment, setSelectedComment] = useState<SelectedComment>({
    commentId: '',
    fromEditor: false,
  });

  // Use useCallbackRef for these to avoid the problem of stale references when they're called from the editor undo/redo
  const removeComment = useCallbackRef(
    (commentId: string) => {
      const commentToDelete = comments.find(
        comment => comment.id === commentId,
      );
      if (commentToDelete) {
        setComments([...comments.filter(comment => comment.id !== commentId)]);

        setPendingDeletions([...pendingDeletions, commentToDelete]);

        setCurrentInteraction({
          type: 'removing',
          id: commentId,
        });
      }
    },
    [comments, pendingDeletions],
  );

  const resolveComment = useCallbackRef(
    async (commentId: string, updateMarker?: boolean) => {
      const comment = comments.find(c => c.id === commentId);
      if (!comment) {
        return;
      }
      const resolvedComment = {
        ...comment,
        setResolved: true,
      };
      const updatedComment = await value.onSaveUpdatedComment(resolvedComment);
      if (updatedComment) {
        const index = comments.findIndex(c => c.id === updatedComment.id);
        const updatedComments = [...comments];
        updatedComments[index] = updatedComment;
        setComments(updatedComments);
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

  const reAddComment = useCallbackRef(
    (commentId: string) => {
      const commentToUndelete = pendingDeletions.find(
        comment => comment.id === commentId,
      );
      if (commentToUndelete) {
        setPendingDeletions([
          ...pendingDeletions.filter(deletion => deletion.id !== commentId),
        ]);
        setComments([...comments, commentToUndelete]);
      }
    },
    [comments, pendingDeletions],
  );

  const unresolveComment = useCallbackRef(
    async (commentId: string, updateMarker?: boolean) => {
      const comment = comments.find(c => c.id === commentId);
      if (!comment) {
        return;
      }
      const unresolvedComment = {
        ...comment,
        setResolved: false,
      };

      const updatedComment = await value.onSaveUpdatedComment(
        unresolvedComment,
      );
      if (updatedComment) {
        const index = comments.findIndex(c => c.id === updatedComment.id);
        const updatedComments = [...comments];
        updatedComments[index] = updatedComment;
        setComments(updatedComments);
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
    const addComment = async (comment: AddComment) => {
      const newComment = await value.onSaveComment(comment);
      if (newComment) {
        setComments(currentComments => [...currentComments, newComment]);
        setCurrentInteraction({ type: 'adding', id: newComment.id });
        return newComment;
      }
      return null;
    };

    const deletePendingComments = async () => {
      const promises: Promise<void>[] = [];
      pendingDeletions.forEach(deletion => {
        promises.push(value.onDeletePendingComment(deletion.id));
      });
      await Promise.all(promises);
      setPendingDeletions([]);
    };

    const updateComment = async (comment: UpdateComment) => {
      const updatedComment = await value.onSaveUpdatedComment(comment);
      if (updatedComment) {
        const index = comments.findIndex(c => c.id === updatedComment.id);
        const updatedComments = [...comments];
        updatedComments.splice(index, index, updatedComment);
        setComments(updatedComments);
      }
    };

    return {
      addComment,
      comments,
      currentInteraction,
      deletePendingComments,
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
    value,
  ]);

  return (
    <CommentsStateContext.Provider value={state}>
      {children}
    </CommentsStateContext.Provider>
  );
};

export function useCommentsContext() {
  return useContext(CommentsStateContext);
}
