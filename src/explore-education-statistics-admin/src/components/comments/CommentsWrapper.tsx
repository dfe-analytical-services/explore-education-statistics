import styles from '@admin/components/comments/CommentsWrapper.module.scss';
import { CommentType } from '@admin/components/comments/Comment';
import CommentAddForm from '@admin/components/comments/CommentAddForm';
import CommentsList from '@admin/components/comments/CommentsList';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import React, { ReactNode, useRef } from 'react';
import classNames from 'classnames';
import Button from '@common/components/Button';
import Tooltip from '@common/components/Tooltip';

interface Props {
  allowComments?: boolean;
  blockLockedMessage?: string;
  children: ReactNode;
  commentType: CommentType;
  id: string;
  showCommentAddForm?: boolean;
  showCommentsList?: boolean;
  testId?: string;
  onAdd?: () => void;
  onAddCancel?: () => void;
  onAddSave?: () => void;
  onViewComments?: () => void;
}

export default function CommentsWrapper({
  allowComments = true,
  blockLockedMessage,
  children,
  id,
  commentType,
  showCommentAddForm,
  showCommentsList = true,
  testId,
  onAdd,
  onAddCancel,
  onAddSave,
  onViewComments,
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const { comments } = useCommentsContext();
  const isInline = commentType === 'inline';

  return (
    <div className={styles.container} ref={containerRef} data-testid={testId}>
      {allowComments && (
        <div className={styles.sidebar} data-testid="comments-sidebar">
          {showCommentAddForm && onAddCancel && onAddSave && (
            <CommentAddForm
              baseId={id}
              containerRef={containerRef}
              onCancel={onAddCancel}
              onSave={onAddSave}
            />
          )}
          {!showCommentAddForm && !isInline && (
            <Button
              className={`${styles.button} govuk-!-margin-bottom-2`}
              testId="comment-add-button"
              variant="secondary"
              onClick={onAdd}
            >
              Add comment
            </Button>
          )}

          {!showCommentsList && comments.length > 0 && (
            <Tooltip text={blockLockedMessage} enabled={!!blockLockedMessage}>
              {({ ref }) => (
                <Button
                  ariaDisabled={!!blockLockedMessage}
                  className={styles.button}
                  ref={ref}
                  testId="view-comments"
                  variant="secondary"
                  onClick={onViewComments}
                >
                  View comments
                  <br />
                  <span className="govuk-!-margin-top-1 govuk-body-s">
                    {`(${
                      comments.filter(comment => !comment.resolved).length
                    } unresolved)`}
                  </span>
                </Button>
              )}
            </Tooltip>
          )}

          {showCommentsList && comments.length > 0 && (
            <CommentsList
              className={classNames(styles.list, {
                [styles.inline]: isInline,
                [styles.formOpen]: showCommentAddForm,
              })}
              type={commentType}
            />
          )}
        </div>
      )}
      <div className={styles.block}>{children}</div>
    </div>
  );
}
