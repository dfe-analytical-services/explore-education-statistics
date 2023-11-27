import Comment, { CommentType } from '@admin/components/comments/Comment';
import styles from '@admin/components/comments/CommentsList.module.scss';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import Details from '@common/components/Details';
import { useDesktopMedia } from '@common/hooks/useMedia';
import sortBy from 'lodash/sortBy';
import React, { ReactNode } from 'react';

interface Props {
  className?: string;
  type: CommentType;
}

export default function CommentsList({ className, type }: Props) {
  const { comments, markersOrder } = useCommentsContext();
  const resolvedComments = comments.filter(comment => comment.resolved);
  const unresolvedComments = sortBy(
    comments.filter(comment => !comment.resolved),
    comment => markersOrder.indexOf(comment.id),
  );

  return (
    <div className={className}>
      {unresolvedComments.length > 0 && (
        <UnresolvedCommentsWrapper total={unresolvedComments.length}>
          <ol className={styles.list} data-testid="comments-unresolved">
            {unresolvedComments.map(comment => (
              <Comment key={comment.id} comment={comment} type={type} />
            ))}
          </ol>
        </UnresolvedCommentsWrapper>
      )}

      {resolvedComments.length > 0 && (
        <Details summary={`Resolved comments (${resolvedComments.length})`}>
          <ol className={styles.list} data-testid="comments-resolved">
            {resolvedComments.map(comment => (
              <Comment key={comment.id} comment={comment} type={type} />
            ))}
          </ol>
        </Details>
      )}
    </div>
  );
}

function UnresolvedCommentsWrapper({
  children,
  total,
}: {
  children: ReactNode;
  total: number;
}) {
  const { isMedia: isDesktopMedia } = useDesktopMedia();

  if (isDesktopMedia) {
    return <>{children}</>;
  }

  return (
    <Details open summary={`View comments (${total})`}>
      {children}
    </Details>
  );
}
