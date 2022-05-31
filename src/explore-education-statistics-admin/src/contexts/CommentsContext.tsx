import { Comment } from '@admin/services/types/content';
import { CommentCreate } from '@admin/services/releaseContentCommentService';
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
  addComment: (comment: CommentCreate) => Promise<Comment | null>;
  comments: Comment[];
  currentInteraction: CurrentCommentInteraction;
  clearPendingDeletions: () => void;
  markersOrder: string[];
  pendingDeletions: Comment[];
  reAddComment: MutableRefObject<(commentId: string) => void>;
  removeComment: MutableRefObject<(commentId: string) => void>;
  resolveComment: MutableRefObject<
    (commentId: string, updateMarker?: boolean) => void
  >;
  selectedComment: SelectedComment;
  setCurrentInteraction: (
    currentInteraction: CurrentCommentInteraction,
  ) => void;
  setMarkersOrder: (order: string[]) => void;
  setSelectedComment: (selectedComment: SelectedComment) => void;
  unresolveComment: MutableRefObject<
    (commentId: string, updateMarker?: boolean) => void
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
  onCreate: (comment: CommentCreate) => Promise<Comment>;
  onDelete: (commentId: string) => void;
  onPendingDelete: (commentId: string) => void;
  onPendingDeleteUndo: (commentId: string) => void;
  onUpdate: (comment: Comment) => void;
}

export const CommentsContextProvider = ({
  children,
  comments: commentsProp,
  markersOrder: initialMarkersOrder = [],
  pendingDeletions: initialPendingDeletions = [],
  onCreate,
  onDelete,
  onPendingDelete,
  onPendingDeleteUndo,
  onUpdate,
}: CommentsContextProviderProps) => {
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

  const comments = useMemo(
    () =>
      commentsProp.filter(
        comment =>
          !pendingDeletions.some(deletion => deletion.id === comment.id),
      ),
    [commentsProp, pendingDeletions],
  );

  // Use `useCallbackRef` for these callbacks to avoid the problem of stale
  // references when they're called from the editor undo/redo

  const removeComment: CommentsContextState['removeComment'] = useCallbackRef(
    commentId => {
      const commentToDelete = comments.find(
        comment => comment.id === commentId,
      );

      if (commentToDelete) {
        setPendingDeletions([...pendingDeletions, commentToDelete]);

        setCurrentInteraction({
          type: 'removing',
          id: commentId,
        });

        if (commentId !== 'commentPlaceholder') {
          onPendingDelete(commentId);
        }
      }
    },
    [comments, pendingDeletions],
  );

  const resolveComment: CommentsContextState['resolveComment'] = useCallbackRef(
    async (commentId, updateMarker) => {
      const comment = comments.find(c => c.id === commentId);

      if (!comment) {
        return;
      }

      await onUpdate({
        ...comment,
        setResolved: true,
      });

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
    commentId => {
      const commentToUndelete = pendingDeletions.find(
        deletion => deletion.id === commentId,
      );

      if (commentToUndelete) {
        setPendingDeletions(
          pendingDeletions.filter(deletion => deletion.id !== commentId),
        );

        onPendingDeleteUndo(commentId);
      }
    },
    [comments, pendingDeletions],
  );

  const unresolveComment: CommentsContextState['unresolveComment'] = useCallbackRef(
    async (commentId, updateMarker) => {
      const comment = comments.find(c => c.id === commentId);

      if (!comment) {
        return;
      }

      await onUpdate({
        ...comment,
        setResolved: false,
      });

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
    const addComment: CommentsContextState['addComment'] = async comment => {
      const newComment = await onCreate(comment);

      setCurrentInteraction({ type: 'adding', id: newComment.id });

      return newComment;
    };

    const clearPendingDeletions = async () => {
      await Promise.all(
        pendingDeletions.map(deletion => onDelete(deletion.id)),
      );
      setPendingDeletions([]);
    };

    const updateComment: CommentsContextState['updateComment'] = async comment => {
      await onUpdate(comment);
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
    onDelete,
    onCreate,
    onUpdate,
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
