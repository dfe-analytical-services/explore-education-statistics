import styles from '@admin/components/comments/Comment.module.scss';
import CommentEditForm from '@admin/components/comments/CommentEditForm';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
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
  comment: CommentType;
}

const Comment = ({ comment }: Props) => {
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
    removeComment,
    resolveComment,
    unresolveComment,
    setSelectedComment,
  } = useCommentsContext();
  const [isEditingComment, toggleIsEditingComment] = useToggle(false);
  const [isFocused, toggleIsFocused] = useToggle(false);
  const ref = useRef<HTMLDivElement>(null);
  const { user } = useAuthContext();

  useEffect(() => {
    if (selectedComment.id === id) {
      ref.current?.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
        inline: 'start',
      });
    }
  }, [id, selectedComment]);

  const handleCommentSelection = () => {
    if (!resolved) {
      setSelectedComment({ id: comment.id });
    }
  };

  return (
    <li className={styles.container} data-testid="comment">
      <>
        {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
        <div
          className={classNames(styles.comment, {
            [styles.active]: selectedComment.id === id,
            [styles.focused]: isFocused,
          })}
          ref={ref}
          onClick={handleCommentSelection}
        >
          <p className="govuk-!-margin-bottom-0 govuk-body-s">
            <strong data-testid="comment-author">
              {`${createdBy.firstName} ${createdBy.lastName} `}
            </strong>
            <span className="govuk-visually-hidden"> commented on </span>
            <br />
            <FormattedDate format="d MMM yyyy, HH:mm">{created}</FormattedDate>
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
          {isEditingComment ? (
            <div className={styles.form}>
              <CommentEditForm
                comment={comment}
                id={id}
                onCancel={toggleIsEditingComment.off}
                onSubmit={toggleIsEditingComment.off}
              />
            </div>
          ) : (
            <div
              className="govuk-!-margin-bottom-3 govuk-!-margin-top-2"
              data-testid="comment-content"
            >
              {content}
            </div>
          )}

          {resolved && (
            <p className="govuk-!-margin-bottom-0 govuk-body-s">
              Resolved by {resolvedBy?.firstName} {resolvedBy?.lastName} on{' '}
              <FormattedDate format="d MMM yyyy, HH:mm">
                {resolved}
              </FormattedDate>
            </p>
          )}

          {!isEditingComment && (
            <>
              <button
                aria-hidden
                className="govuk-visually-hidden"
                type="button"
                onBlur={toggleIsFocused.off}
                onFocus={toggleIsFocused.on}
                onKeyDown={e => {
                  if (e.key === 'Enter') {
                    handleCommentSelection();
                  }
                }}
              >
                Highlight comment in content
              </button>
              {resolved ? (
                <ButtonText
                  onClick={async () => {
                    await unresolveComment.current(comment.id, true);
                  }}
                >
                  Unresolve
                </ButtonText>
              ) : (
                <ButtonGroup className="govuk-!-margin-bottom-0">
                  <Button
                    onClick={async () => {
                      await resolveComment.current(comment.id, true);
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
                          removeComment.current(comment.id);
                        }}
                      >
                        Delete
                      </ButtonText>
                    </>
                  )}
                </ButtonGroup>
              )}
            </>
          )}
        </div>
      </>
    </li>
  );
};

export default Comment;
