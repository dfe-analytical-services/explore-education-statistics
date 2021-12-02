import Comment, {
  ResolveCommentEvent,
} from '@admin/components/comments/Comment';
import styles from '@admin/components/comments/CommentsList.module.scss';
import { SelectedComment } from '@admin/components/editable/EditableContentForm';
import { Comment as CommentType } from '@admin/services/types/content';
import Details from '@common/components/Details';
import sortBy from 'lodash/sortBy';
import React, { useEffect, useState } from 'react';

interface Props {
  comments: CommentType[];
  markersOrder: string[];
  selectedComment: SelectedComment;
  onRemove: (commentId: string) => void;
  onResolve: (event: ResolveCommentEvent) => void;
  onSelect: (id: string) => void;
  onUpdate: (comment: CommentType) => void;
}

const CommentsList = ({
  comments,
  markersOrder,
  selectedComment,
  onRemove,
  onResolve,
  onSelect,
  onUpdate,
}: Props) => {
  const [unresolvedComments, setUnresolvedComments] = useState<CommentType[]>(
    [],
  );
  const [resolvedComments, setResolvedComments] = useState<CommentType[]>([]);

  useEffect(() => {
    setResolvedComments(comments.filter(comment => comment.resolved));
    const unresolved = comments.filter(comment => !comment.resolved);
    setUnresolvedComments(
      sortBy(unresolved, comment => markersOrder.indexOf(comment.id)),
    );
  }, [comments, markersOrder]);

  return (
    <>
      <ol className={styles.list} data-testid="unresolvedComments">
        {unresolvedComments.map(comment => (
          <Comment
            key={comment.id}
            active={selectedComment.commentId === comment.id}
            comment={comment}
            onRemove={onRemove}
            onResolve={onResolve}
            onSelect={onSelect}
            onUpdate={onUpdate}
          />
        ))}
      </ol>
      {resolvedComments.length > 0 && (
        <Details summary={`Resolved comments (${resolvedComments.length})`}>
          <ol className={styles.list} data-testid="resolvedComments">
            {resolvedComments.map(comment => (
              <Comment
                key={comment.id}
                active={false}
                comment={comment}
                onRemove={onRemove}
                onResolve={onResolve}
                onSelect={onSelect}
                onUpdate={onUpdate}
              />
            ))}
          </ol>
        </Details>
      )}
    </>
  );
};

export default CommentsList;
