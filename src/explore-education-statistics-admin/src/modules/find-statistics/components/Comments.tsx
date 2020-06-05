import { useAuthContext } from '@admin/contexts/AuthContext';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import releaseContentCommentService, {
  AddExtendedComment,
  UpdateExtendedComment,
} from '@admin/services/releaseContentCommentService';
import { ExtendedComment } from '@admin/services/types/content';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import classNames from 'classnames';
import React, { useRef, useState } from 'react';
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
  comments = [],
  onChange,
  canResolve = false,
  canComment = false,
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);

  const [newComment, setNewComment] = useState<string>('');
  const [editableComment, setEditableComment] = useState<string>('');
  const [editableCommentText, setEditableCommentText] = useState<string>('');

  const { user } = useAuthContext();
  const { releaseId } = useManageReleaseContext();

  const addComment = (content: string) => {
    const additionalComment: AddExtendedComment = {
      content,
    };

    if (releaseId && sectionId) {
      releaseContentCommentService
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

    releaseContentCommentService
      .deleteContentSectionComment(commentId)
      .then(() => {
        const newComments = [...comments];
        newComments.splice(index, 1);

        onChange(blockId, newComments);
      });
  };

  const updateComment = (index: number, content: string) => {
    const editedComment: UpdateExtendedComment = {
      ...comments[index],
      content,
    };

    releaseContentCommentService
      .updateContentSectionComment(editedComment)
      .then(savedComment => {
        const newComments = [...comments];
        newComments[index] = savedComment;

        onChange(blockId, newComments);

        setEditableComment('');
        setEditableCommentText('');
      });
  };

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
            {comments.map(
              (
                { id, content, created, createdById, createdByName, updated },
                index,
              ) => (
                <div key={id}>
                  <h2 className="govuk-body-xs govuk-!-margin-0">
                    <strong>
                      {createdByName} <FormattedDate>{created}</FormattedDate>
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
                      {content}
                    </p>
                  )}
                  {canComment && (
                    <ButtonText
                      className="govuk-body-xs govuk-!-margin-right-3"
                      onClick={() => removeComment(index)}
                    >
                      Remove
                    </ButtonText>
                  )}
                  {canComment && user?.id === createdById && (
                    <>
                      {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events */}
                      <ButtonText
                        className="govuk-body-xs govuk-!-margin-right-3"
                        onClick={() => {
                          setEditableCommentText(content);
                          return editableComment
                            ? setEditableComment('')
                            : setEditableComment(id);
                        }}
                      >
                        {editableComment && editableComment === id
                          ? 'Cancel'
                          : 'Edit'}
                      </ButtonText>
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
