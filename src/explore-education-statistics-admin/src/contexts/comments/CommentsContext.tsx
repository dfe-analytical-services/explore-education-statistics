import { Comment } from '@admin/services/types/content';
import {
  AddComment,
  UpdateComment,
} from '@admin/services/releaseContentCommentService';
import React, {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
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
  onAddComment: (comment: AddComment) => Promise<Comment | null>;
  onDeleteComment: (commentId: string) => void;
  onDeletePendingComments: () => void;
  onResolveComment: (commentId: string, updateMarker?: boolean) => void;
  onUndeleteComment: (commentId: string) => void;
  onUnresolveComment: (commentId: string, updateMarker?: boolean) => void;
  onUpdateComment: (comment: UpdateComment) => void;
  setCurrentInteraction: (currentInteraction: CurrentInteraction) => void;
  setMarkersOrder: (order: string[]) => void;
  setSelectedComment: (selectedComment: SelectedComment) => void;
};

export type CommentsContextInitialValues = {
  comments: Comment[];
  onDeletePendingComment: (commentId: string) => Promise<void>;
  onSaveComment: (comment: AddComment) => Promise<Comment>;
  onSaveUpdatedComment: (comment: UpdateComment) => Promise<Comment>;
};

export const CommentsStateContext = createContext<CommentsContextState>({
  comments: [],
  currentInteraction: undefined,
  markersOrder: [],
  pendingDeletions: [],
  selectedComment: { commentId: '' },
  onAddComment: () => Promise.resolve(null),
  onDeleteComment: noop,
  onDeletePendingComments: noop,
  onResolveComment: noop,
  onUndeleteComment: noop,
  onUnresolveComment: noop,
  onUpdateComment: noop,
  setCurrentInteraction: noop,
  setMarkersOrder: noop,
  setSelectedComment: noop,
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
  const [markersOrder, setMarkersOrder] = useState<string[]>([]);
  const [pendingDeletions, setPendingDeletions] = useState<Comment[]>([]);
  const [selectedComment, setSelectedComment] = useState<SelectedComment>({
    commentId: '',
    fromEditor: false,
  });

  // Store and update comments and pending deletions ref as they are stale in any function
  // called by undo/redo in the editor.
  const commentsRef = useRef(comments);
  const pendingDeletionsRef = useRef(pendingDeletions);
  useEffect(() => {
    commentsRef.current = comments;
    pendingDeletionsRef.current = pendingDeletions;
  }, [comments, pendingDeletions]);

  const state = useMemo<CommentsContextState>(() => {
    const onAddComment = async (comment: AddComment) => {
      const newComment = await value.onSaveComment(comment);
      if (newComment) {
        setComments(currentComments => [...currentComments, newComment]);
        setCurrentInteraction({ type: 'adding', id: newComment.id });
        return newComment;
      }
      return null;
    };

    const onDeleteComment = (commentId: string) => {
      const commentToDelete = commentsRef.current.find(
        comment => comment.id === commentId,
      );
      if (commentToDelete) {
        setComments([
          ...commentsRef.current.filter(comment => comment.id !== commentId),
        ]);

        setPendingDeletions([...pendingDeletionsRef.current, commentToDelete]);

        setCurrentInteraction({
          type: 'removing',
          id: commentId,
        });
      }
    };

    const onUndeleteComment = (commentId: string) => {
      const commentToUndelete = pendingDeletionsRef.current.find(
        comment => comment.id === commentId,
      );
      if (commentToUndelete) {
        setPendingDeletions([
          ...pendingDeletionsRef.current.filter(
            deletion => deletion.id !== commentId,
          ),
        ]);
        setComments([...commentsRef.current, commentToUndelete]);
      }
    };

    const onDeletePendingComments = async () => {
      const promises: Promise<void>[] = [];
      pendingDeletions.forEach(deletion => {
        promises.push(value.onDeletePendingComment(deletion.id));
      });
      await Promise.all(promises);
      setPendingDeletions([]);
    };

    const onUpdateComment = async (comment: UpdateComment) => {
      const updatedComment = await value.onSaveUpdatedComment(comment);
      if (updatedComment) {
        const index = comments.findIndex(c => c.id === updatedComment.id);
        const updatedComments = [...comments];
        updatedComments.splice(index, index, updatedComment);
        setComments(updatedComments);
      }
    };

    const onResolveComment = async (
      commentId: string,
      updateMarker?: boolean,
    ) => {
      const comment = commentsRef.current.find(c => c.id === commentId);
      if (!comment) {
        return;
      }
      const resolvedComment = {
        ...comment,
        setResolved: true,
      };
      const updatedComment = await value.onSaveUpdatedComment(resolvedComment);
      if (updatedComment) {
        const index = commentsRef.current.findIndex(
          c => c.id === updatedComment.id,
        );
        const updatedComments = [...commentsRef.current];
        updatedComments[index] = updatedComment;
        setComments(updatedComments);
      }
      if (updateMarker) {
        setCurrentInteraction({
          type: 'resolving',
          id: comment.id,
        });
      }
    };

    const onUnresolveComment = async (
      commentId: string,
      updateMarker?: boolean,
    ) => {
      const comment = commentsRef.current.find(c => c.id === commentId);
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
        const index = commentsRef.current.findIndex(
          c => c.id === updatedComment.id,
        );
        const updatedComments = [...commentsRef.current];
        updatedComments[index] = updatedComment;
        setComments(updatedComments);
      }

      if (updateMarker) {
        setCurrentInteraction({
          type: 'unresolving',
          id: comment.id,
        });
      }
    };

    return {
      comments,
      currentInteraction,
      markersOrder,
      pendingDeletions,
      selectedComment,
      onAddComment,
      onDeleteComment,
      onResolveComment,
      onUnresolveComment,
      onUpdateComment,
      onUndeleteComment,
      onDeletePendingComments,
      setCurrentInteraction,
      setMarkersOrder,
      setSelectedComment,
    };
  }, [
    comments,
    currentInteraction,
    markersOrder,
    pendingDeletions,
    selectedComment,
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
