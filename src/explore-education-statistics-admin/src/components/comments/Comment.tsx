import styles from '@admin/components/comments/Comment.module.scss';
import CommentEditForm from '@admin/components/comments/CommentEditForm';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import { Comment as CommentData } from '@admin/services/types/content';
import FormattedDate from '@common/components/FormattedDate';
import { useAuthContext } from '@admin/contexts/AuthContext';
import classNames from 'classnames';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode, useEffect, useRef } from 'react';

export type CommentType = 'inline' | 'block';

interface Props {
  comment: CommentData;
  type: CommentType;
}

export default function Comment({ comment, type }: Props) {
  return type === 'inline' ? (
    <InlineComment comment={comment} />
  ) : (
    <BlockComment comment={comment} />
  );
}

function InlineComment({ comment }: { comment: CommentData }) {
  const {
    selectedComment,
    removeComment,
    resolveComment,
    unresolveComment,
    setSelectedComment,
  } = useCommentsContext();
  const [isFocused, toggleIsFocused] = useToggle(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (selectedComment.id === comment.id) {
      ref.current?.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
        inline: 'start',
      });
    }
  }, [comment.id, selectedComment]);

  const handleCommentSelection = () => {
    if (!comment.resolved) {
      setSelectedComment({ id: comment.id });
    }
  };

  return (
    <li className={styles.container} data-testid="comment">
      {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
      <div
        className={classNames(styles.comment, {
          [styles.active]: selectedComment.id === comment.id,
          [styles.focused]: isFocused,
        })}
        ref={ref}
        onClick={handleCommentSelection}
      >
        <BaseComment
          comment={comment}
          highlightButton={
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
          }
          onRemove={() => {
            removeComment.current(comment.id);
          }}
          onResolve={async () => {
            await resolveComment.current(comment.id, true);
          }}
          onUnresolve={async () => {
            await unresolveComment.current(comment.id, true);
          }}
        />
      </div>
    </li>
  );
}

function BlockComment({ comment }: { comment: CommentData }) {
  const { deleteComment, resolveComment, unresolveComment } =
    useCommentsContext();

  return (
    <li className={styles.container} data-testid="comment">
      <div className={styles.comment}>
        <BaseComment
          comment={comment}
          onRemove={async () => {
            await deleteComment(comment.id);
          }}
          onResolve={async () => {
            await resolveComment.current(comment.id, true);
          }}
          onUnresolve={async () => {
            await unresolveComment.current(comment.id, true);
          }}
        />
      </div>
    </li>
  );
}

interface BaseCommentProps {
  comment: CommentData;
  highlightButton?: ReactNode;
  onRemove: () => void;
  onResolve: () => void;
  onUnresolve: () => void;
}

function BaseComment({
  comment,
  highlightButton,
  onRemove,
  onResolve,
  onUnresolve,
}: BaseCommentProps) {
  const { content, created, createdBy, id, resolved, resolvedBy, updated } =
    comment;

  const { user } = useAuthContext();

  const [isEditingComment, toggleIsEditingComment] = useToggle(false);

  return (
    <>
      <p className="govuk-!-margin-bottom-0 govuk-body-s">
        <strong data-testid="comment-author">{createdBy.displayName}</strong>
        <span className="govuk-visually-hidden"> commented on </span>
        <br />
        <FormattedDate format="d MMM yyyy, HH:mm">{created}</FormattedDate>
        {updated && (
          <>
            <br />
            (Updated{' '}
            <FormattedDate format="d MMM yyyy, HH:mm">{updated}</FormattedDate>)
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
          className="govuk-!-margin-bottom-3 govuk-!-margin-top-2 dfe-word-break--break-word"
          data-testid="comment-content"
        >
          {content}
        </div>
      )}
      {resolved && (
        <p className="govuk-!-margin-bottom-0 govuk-body-s">
          Resolved by {resolvedBy?.displayName} on{' '}
          <FormattedDate format="d MMM yyyy, HH:mm">{resolved}</FormattedDate>
        </p>
      )}
      {!isEditingComment && (
        <>
          {highlightButton}
          {resolved ? (
            <ButtonText onClick={onUnresolve}>Unresolve</ButtonText>
          ) : (
            <ButtonGroup className="govuk-!-margin-bottom-0">
              <Button onClick={onResolve}>Resolve</Button>
              {user?.id === createdBy.id && (
                <>
                  <ButtonText onClick={toggleIsEditingComment.on}>
                    Edit
                  </ButtonText>

                  <ButtonText onClick={onRemove}>Delete</ButtonText>
                </>
              )}
            </ButtonGroup>
          )}
        </>
      )}
    </>
  );
}
