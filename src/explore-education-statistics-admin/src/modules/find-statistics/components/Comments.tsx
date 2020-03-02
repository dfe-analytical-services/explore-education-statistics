import LoginContext from '@admin/components/Login';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import { ExtendedComment } from '@admin/services/publicationService';
import { releaseContentService as service } from '@admin/services/release/edit-release/content/service';
import { User } from '@admin/services/sign-in/types';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import classNames from 'classnames';
import React, { useContext } from 'react';
import styles from './Comments.module.scss';

interface Props {
  contentBlockId: string;
  sectionId: string;
  initialComments: ExtendedComment[];
  onCommentsChange?: (comments: ExtendedComment[]) => Promise<void>;
  canResolve?: boolean;
  canComment?: boolean;
}

const Comments = ({
  contentBlockId,
  sectionId,
  initialComments,
  onCommentsChange = () => Promise.resolve(),
  canResolve = false,
  canComment = false,
}: Props) => {
  const [newComment, setNewComment] = React.useState<string>('');
  const [comments, setComments] = React.useState<ExtendedComment[]>(
    initialComments,
  );

  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;
  const context = React.useContext(LoginContext);

  const addComment = (comment: string) => {
    const user = context.user as User;

    const additionalComment: ExtendedComment = {
      id: '0',
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
          contentBlockId,
          additionalComment,
        )
        .then(populatedComment => {
          let newComments = [populatedComment];

          if (comments) {
            newComments = [...newComments, ...comments];
          }

          onCommentsChange(newComments).then(() => {
            setComments(newComments);
            setNewComment('');
          });
        });
    }
  };

  const removeComment = (index: number) => {
    const commentId = comments[index].id;

    if (releaseId && sectionId) {
      service
        .deleteContentSectionComment(
          releaseId,
          sectionId,
          contentBlockId,
          commentId,
        )
        .then(() => {
          const newComments = [...comments];

          newComments.splice(index, 1);

          onCommentsChange(newComments).then(() => {
            setComments(newComments);
          });
        });
    }
  };

  const resolveComment = (index: number) => {
    const resolvedComment = { ...comments[index] };

    resolvedComment.state = 'resolved';
    resolvedComment.resolvedOn = new Date();
    resolvedComment.resolvedBy = context.user && context.user.name;

    if (releaseId && sectionId) {
      service
        .updateContentSectionComment(
          releaseId,
          sectionId,
          contentBlockId,
          resolvedComment,
        )
        .then(() => {
          const newComments = [...comments];

          newComments[index] = resolvedComment;

          onCommentsChange(newComments).then(() => {
            setComments(newComments);
          });
        });
    }
  };

  const ref = React.createRef<HTMLDivElement>();

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
                `content-section-${contentBlockId}`,
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
                  id={`new_comment_${contentBlockId}`}
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
                    <p className="govuk-body-xs govuk-!-margin-bottom-1">
                      {commentText}
                    </p>
                    {state === 'open' &&
                      (canResolve ? (
                        <button
                          type="button"
                          className="govuk-body-xs govuk-!-margin-right-3"
                          onClick={() => resolveComment(index)}
                        >
                          Resolve
                        </button>
                      ) : (
                        <span className="govuk-body-xs govuk-!-margin-right-3">
                          Open
                        </span>
                      ))}
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
                          className="govuk-body-xs"
                          role="button"
                          tabIndex={0}
                          onClick={() => removeComment(index)}
                          style={{ cursor: 'pointer' }}
                        >
                          Remove
                        </a>
                        <hr />
                      </>
                    )}
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
