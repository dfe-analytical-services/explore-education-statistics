import Comment from '@admin/components/comments/Comment';
import styles from '@admin/components/comments/CommentsList.module.scss';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import Details from '@common/components/Details';
import sortBy from 'lodash/sortBy';
import React from 'react';

interface Props {
  blockId: string;
}

const CommentsList = ({ blockId }: Props) => {
  const { comments, markersOrder } = useCommentsContext();

  const resolvedComments = comments.filter(comment => comment.resolved);
  const unresolvedComments = sortBy(
    comments.filter(comment => !comment.resolved),
    comment => markersOrder.indexOf(comment.id),
  );

  return (
    <>
      <ol className={styles.list} data-testid="unresolvedComments">
        {unresolvedComments.map(comment => (
          <Comment key={comment.id} blockId={blockId} comment={comment} />
        ))}
      </ol>
      {resolvedComments.length > 0 && (
        <Details summary={`Resolved comments (${resolvedComments.length})`}>
          <ol className={styles.list} data-testid="resolvedComments">
            {resolvedComments.map(comment => (
              <Comment key={comment.id} blockId={blockId} comment={comment} />
            ))}
          </ol>
        </Details>
      )}
    </>
  );
};

export default CommentsList;
