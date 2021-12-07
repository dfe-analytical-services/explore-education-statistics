import styles from '@admin/components/comments/Comment.module.scss';
import CommentEditForm from '@admin/components/comments/CommentEditForm';
import { useCommentsContext } from '@admin/contexts/comments/CommentsContext';
import useEditingActions from '@admin/contexts/editing/useEditingActions';
import { Comment as CommentType } from '@admin/services/types/content';
import FormattedDate from '@common/components/FormattedDate';
import { useAuthContext } from '@admin/contexts/AuthContext';
import classNames from 'classnames';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect, useRef } from 'react';

interface Props {
  blockId: string;
  comment: CommentType;
}
const Comment = ({ blockId, comment }: Props) => {
  const {
    content,
    created,
    createdBy,
    id,
    resolved,
    resolvedBy,
    updated,
  } = comment;
  const {
    selectedComment,
    onDeleteComment,
    onResolveComment,
    onUnresolveComment,
    setSelectedComment,
  } = useCommentsContext();
  const editingActions = useEditingActions();
  const [isEditingComment, toggleIsEditingComment] = useToggle(false);
  const ref = useRef<HTMLDivElement>(null);
  const { user } = useAuthContext();

  useEffect(() => {
    if (selectedComment.commentId === id) {
      ref.current?.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
        inline: 'start',
      });
    }
  }, [id, selectedComment]);

  const handleCommentSelection = () => {
    if (!comment.resolved) {
      setSelectedComment({ commentId: comment.id });
    }
  };

  return (
    <li className={styles.container}>
      {isEditingComment ? (
        <div className={styles.form}>
          <CommentEditForm
            comment={comment}
            id={id}
            onCancel={toggleIsEditingComment.off}
            onSubmit={() => {
              toggleIsEditingComment.off();
            }}
          />
        </div>
      ) : (
        <>
          <div
            aria-label="Comment"
            className={classNames(styles.comment, {
              [styles.active]: selectedComment.commentId === id,
            })}
            ref={ref}
            role={!comment.resolved ? 'button' : undefined}
            tabIndex={!comment.resolved ? 0 : undefined}
            onKeyDown={e => {
              if (e.key === 'Enter') {
                handleCommentSelection();
              }
            }}
            onClick={() => {
              handleCommentSelection();
            }}
          >
            <p className="govuk-!-margin-bottom-0 govuk-body-s">
              <strong>{`${createdBy.firstName} ${createdBy.lastName} `}</strong>
              <span className="govuk-visually-hidden"> commented on </span>
              <br />
              <FormattedDate format="d MMM yyyy, HH:mm">
                {created}
              </FormattedDate>
              {updated && (
                <>
                  <br />
                  (Updated{' '}
                  <FormattedDate format="d MMM yyyy, HH:mm">
                    {updated}
                  </FormattedDate>
                  )
                </>
              )}
            </p>

            <div
              className="govuk-!-margin-bottom-3 govuk-!-margin-top-2"
              data-testid="comment-content"
            >
              {content}
            </div>

            {resolved && (
              <p className="govuk-!-margin-bottom-0 govuk-body-s">
                Resolved by {resolvedBy?.firstName} {resolvedBy?.lastName} on{' '}
                <FormattedDate format="d MMM yyyy, HH:mm">
                  {resolved}
                </FormattedDate>
              </p>
            )}
          </div>

          <div className={styles.controls}>
            {resolved ? (
              <ButtonText
                onClick={async () => {
                  await onUnresolveComment?.(comment.id, true);
                  editingActions.updateUnresolvedComments(
                    blockId.replace('block-', ''),
                    comment.id,
                  );
                }}
              >
                Unresolve
              </ButtonText>
            ) : (
              <ButtonGroup className="govuk-!-margin-bottom-0">
                <Button
                  onClick={async () => {
                    editingActions.updateUnresolvedComments(
                      blockId.replace('block-', ''),
                      comment.id,
                    );
                    await onResolveComment?.(comment.id, true);
                  }}
                >
                  Resolve
                </Button>
                {user?.id === createdBy.id && (
                  <>
                    <ButtonText onClick={toggleIsEditingComment.on}>
                      Edit
                    </ButtonText>

                    <ButtonText
                      onClick={async () => {
                        onDeleteComment?.(comment.id);
                        editingActions.updateUnsavedCommentDeletions(
                          blockId.replace('block-', ''),
                          comment.id,
                        );
                      }}
                    >
                      Delete
                    </ButtonText>
                  </>
                )}
              </ButtonGroup>
            )}
          </div>
        </>
      )}
    </li>
  );
};

export default Comment;
