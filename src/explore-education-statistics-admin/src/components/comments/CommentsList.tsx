import Comment, {
  toggleResolveCommentHandler,
} from '@admin/components/comments/Comment';
import { SelectedComment } from '@admin/components/editable/EditableContentForm';
import { Comment as CommentType } from '@admin/services/types/content';
import Details from '@common/components/Details';
import React, { useEffect, useState } from 'react';

interface Props {
  comments: CommentType[];
  commentsPendingDeletion: string[];
  markersOrder: string[];
  selectedComment: SelectedComment;
  onCommentRemoved: (id: string) => void;
  onCommentResolved: ({
    comment,
    resolve,
    updateMarker,
  }: toggleResolveCommentHandler) => void;
  onCommentSelect: (id: string) => void;
  onCommentUpdated: (comment: CommentType) => void;
}

const CommentsList = ({
  comments,
  commentsPendingDeletion,
  markersOrder,
  selectedComment,
  onCommentRemoved,
  onCommentResolved,
  onCommentSelect,
  onCommentUpdated,
}: Props) => {
  const [unresolvedComments, setUnresolvedComments] = useState<CommentType[]>(
    [],
  );
  const [resolvedComments, setResolvedComments] = useState<CommentType[]>([]);

  useEffect(() => {
    setResolvedComments(comments.filter(comment => comment.resolved));
    const unresolved = comments.filter(comment => !comment.resolved);
    // Order comments by marker order.
    unresolved.sort((a, b) => {
      if (markersOrder.indexOf(a.id) > markersOrder.indexOf(b.id)) {
        return 1;
      }
      if (markersOrder.indexOf(a.id) < markersOrder.indexOf(b.id)) {
        return -1;
      }
      return 0;
    });
    setUnresolvedComments(unresolved);
  }, [comments, markersOrder]);

  return (
    <>
      {unresolvedComments.map(comment => (
        <Comment
          key={comment.id}
          active={selectedComment.commentId === comment.id}
          comment={comment}
          isPendingDeletion={commentsPendingDeletion.includes(comment.id)}
          onCommentRemoved={onCommentRemoved}
          onCommentResolved={onCommentResolved}
          onCommentSelect={onCommentSelect}
          onCommentUpdated={onCommentUpdated}
        />
      ))}
      {resolvedComments.length > 0 && (
        <Details summary={`Resolved comments (${resolvedComments.length})`}>
          {resolvedComments.map(comment => (
            <Comment
              key={comment.id}
              active={false}
              comment={comment}
              isPendingDeletion={commentsPendingDeletion.includes(comment.id)}
              onCommentRemoved={onCommentRemoved}
              onCommentResolved={onCommentResolved}
              onCommentSelect={onCommentSelect}
              onCommentUpdated={onCommentUpdated}
            />
          ))}
        </Details>
      )}
    </>
  );
};

export default CommentsList;
