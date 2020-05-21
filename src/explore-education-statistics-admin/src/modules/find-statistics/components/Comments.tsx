import { useAuthContext } from '@admin/contexts/AuthContext';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import { ExtendedComment } from '@admin/services/publicationService';
import { releaseContentService as service } from '@admin/services/release/edit-release/content/service';
import { Authentication } from '@admin/services/sign-in/types';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import classNames from 'classnames';
import React, { createRef, useState } from 'react';
import styles from './Comments.module.scss';

export type CommentsChangeHandler = (
  blockId: string,
  comments: ExtendedComment[],
) => void;

interface Props {
  blockId: string;
  sectionId: string;
  comments: ExtendedComment[];
  canResolve?: boolean;
  canComment?: boolean;
  onChange: CommentsChangeHandler;
}

const Comments = ({
  blockId,
  sectionId,
  comments,
  onChange,
  canResolve = false,
  canComment = false,
}: Props) => {
  const [newComment, setNewComment] = React.useState<string>('');
  const [editableComment, setEditableComment] = useState<string>('');
  const [editableCommentText, setEditableCommentText] = useState<string>('');

  const { releaseId } = useManageReleaseContext();
  const { user }: Authentication = useAuthContext();
  if (!user) {
    throw new Error('User not returned from useAuthContext');
  }

  const addComment = (comment: string) => {
    const additionalComment: ExtendedComment = {
      id: '0',
      userId: user.id,
      name: user.name,
      time: new Date(),
      commentText: comment,
      state: 'open',
    };

    if (releaseId && sectionId) {
      service
        .addContentSectionComment(
          releaseId,
          sectionId,
          blockId,
          additionalComment,
        )
        .then(populatedComment => {
          let newComments = [populatedComment];

          if (comments) {
            newComments = [...newComments, ...comments];
          }

          onChange(blockId, newComments);
          setNewComment('');
        });
    }
  };

  const removeComment = (index: number) => {
    const commentId = comments[index].id;

    if (releaseId && sectionId) {
      service
        .deleteContentSectionComment(releaseId, sectionId, blockId, commentId)
        .then(() => {
          const newComments = [...comments];
          newComments.splice(index, 1);

          onChange(blockId, newComments);
        });
    }
  };

  const resolveComment = (index: number) => {
    const resolvedComment: ExtendedComment = {
      ...comments[index],
      state: 'resolved',
      resolvedOn: new Date(),
      resolvedBy: user.name,
    };

    if (releaseId && sectionId) {
      service
        .updateContentSectionComment(
          releaseId,
          sectionId,
          blockId,
          resolvedComment,
        )
        .then(() => {
          const newComments = [...comments];
          newComments[index] = resolvedComment;

          onChange(blockId, newComments);
        });
    }
  };

  const updateComment = (index: number, newContent: string) => {
    const editedComment = { ...comments[index] };
    editedComment.commentText = newContent;
    if (releaseId && sectionId) {
      service
        .updateContentSectionComment(
          releaseId,
          sectionId,
          blockId,
          editedComment,
        )
        .then(() => {
          const newComments = [...comments];

          newComments[index] = editedComment;

          onChange(blockId, newComments);
          setEditableComment('');
          setEditableCommentText('');
        });
    }
  };

  const ref = createRef<HTMLDivElement>();

  return (
    <>
      <div
        role="presentation"
        ref={ref}
        className={classNames('dfe-comment-block', [styles.addComment])}
      >
        <Details
          summary={`${canComment ? `Add / ` : ''}View comments (${
            comments.length
          })`}
          className="govuk-!-margin-bottom-1 govuk-body-s"
          onToggle={isOpen => {
            if (ref.current) {
              const section = document.getElementById(
                `content-section-${blockId}`,
              );
              if (isOpen) {
                ref.current.classList.add(styles.top);
                if (section) {
                  section.classList.add(styles.commentWhileCommenting);
                }
              } else {
                ref.current.classList.remove(styles.top);
                if (section) {
                  section.classList.remove(styles.commentWhileCommenting);
                }
              }
            }
          }}
        >
          {canComment && (
            <>
              <form>
                <textarea
                  name="comment"
                  id={`new_comment_${blockId}`}
                  value={newComment}
                  onChange={e => setNewComment(e.target.value)}
                />
                <button
                  type="button"
                  className="govuk-button"
                  disabled={newComment.length === 0}
                  onClick={() => {
                    addComment(newComment);
                  }}
                >
                  Submit
                </button>
              </form>
              <hr />
            </>
          )}
          <div className={styles.commentsContainer}>
            {comments &&
              comments.map(
                (
                  {
                    id,
                    userId,
                    name,
                    time,
                    commentText,
                    state,
                    resolvedOn,
                    resolvedBy,
                  },
                  index,
                ) => (
                  <div key={id}>
                    <h2 className="govuk-body-xs govuk-!-margin-0">
                      <strong>
                        {name} <FormattedDate>{time}</FormattedDate>
                      </strong>
                    </h2>
                    {editableComment && editableComment === id ? (
                      <form>
                        <textarea
                          name="editComment"
                          id={`edit_comment_${id}`}
                          value={editableCommentText}
                          onChange={e => setEditableCommentText(e.target.value)}
                        />
                        <button
                          type="button"
                          className="govuk-button"
                          disabled={editableCommentText.length === 0}
                          onClick={() => {
                            updateComment(index, editableCommentText);
                          }}
                        >
                          Update
                        </button>
                      </form>
                    ) : (
                      <p className="govuk-body-xs govuk-!-margin-bottom-1">
                        {commentText}
                      </p>
                    )}
                    {state === 'open' && canResolve && (
                      <button
                        type="button"
                        className="govuk-body-xs govuk-!-margin-right-3"
                        onClick={() => resolveComment(index)}
                      >
                        Resolve
                      </button>
                    )}
                    {state === 'resolved' && (
                      <p className="govuk-body-xs govuk-!-margin-bottom-1 ">
                        <em>
                          Resolved{' '}
                          {resolvedOn && (
                            <FormattedDate>resolvedOn</FormattedDate>
                          )}{' '}
                          by {resolvedBy}
                        </em>
                      </p>
                    )}
                    {canComment && (
                      <>
                        {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events */}
                        <a
                          className="govuk-body-xs govuk-!-margin-right-3"
                          role="button"
                          tabIndex={0}
                          onClick={() => removeComment(index)}
                          style={{ cursor: 'pointer' }}
                        >
                          Remove
                        </a>
                      </>
                    )}
                    {canComment && user?.id === userId && (
                      <>
                        {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events */}
                        <a
                          className="govuk-body-xs govuk-!-margin-right-3"
                          role="button"
                          tabIndex={0}
                          onClick={() => {
                            setEditableCommentText(commentText);
                            return editableComment
                              ? setEditableComment('')
                              : setEditableComment(id);
                          }}
                          style={{ cursor: 'pointer' }}
                        >
                          {editableComment && editableComment === id
                            ? 'Cancel'
                            : 'Edit'}
                        </a>
                      </>
                    )}
                    <hr />
                  </div>
                ),
              )}
          </div>
        </Details>
      </div>
    </>
  );
};

export default Comments;
