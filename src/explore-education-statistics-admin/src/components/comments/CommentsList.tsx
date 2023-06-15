import React from 'react';
import Comment from '@admin/components/comments/Comment';
import styles from '@admin/components/comments/CommentsList.module.scss';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import Details from '@common/components/Details';
import sortBy from 'lodash/sortBy';

interface Props {
  className?: string;
}

const CommentsList = ({ className }: Props) => {
  const { comments, markersOrder } = useCommentsContext();

  const resolvedComments = comments.filter(comment => comment.resolved);
  const unresolvedComments = sortBy(
    comments.filter(comment => !comment.resolved),
    comment => markersOrder.indexOf(comment.id),
  );

  return (
    <div className={className}>
      {unresolvedComments.length > 0 && (
        <ol className={styles.list} data-testid="unresolvedComments">
          {unresolvedComments.map(comment => (
            <Comment key={comment.id} comment={comment} />
          ))}
        </ol>
      )}

      {resolvedComments.length > 0 && (
        <Details summary={`Resolved comments (${resolvedComments.length})`}>
          <ol className={styles.list} data-testid="resolvedComments">
            {resolvedComments.map(comment => (
              <Comment key={comment.id} comment={comment} />
            ))}
          </ol>
        </Details>
      )}
    </div>
  );
};

export default CommentsList;
