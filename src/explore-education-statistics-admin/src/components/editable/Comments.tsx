import { useAuthContext } from '@admin/contexts/AuthContext';
import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
import releaseContentCommentService, {
  AddComment,
  UpdateComment,
} from '@admin/services/releaseContentCommentService';
import { Comment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import { Form } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormattedDate from '@common/components/FormattedDate';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';
import orderBy from 'lodash/orderBy';
import styles from './Comments.module.scss';

interface FormValues {
  content: string;
}

export type CommentsChangeHandler = (
  blockId: string,
  comments: Comment[],
) => void;

interface Props {
  blockId: string;
  sectionId: string;
  comments: Comment[];
  canComment?: boolean;
  onChange: CommentsChangeHandler;
  onToggle?: (opened: boolean) => void;
}

const Comments = ({
  blockId,
  sectionId,
  comments = [],
  onChange,
  onToggle,
  canComment = true,
}: Props) => {
  const [editingComment, setEditingComment] = useState<Comment>();
  const { user } = useAuthContext();
  const { releaseId } = useReleaseContext();

  const addComment = async (content: string) => {
    const additionalComment: AddComment = {
      content,
    };

    if (releaseId && sectionId) {
      const newComment = await releaseContentCommentService.addContentSectionComment(
        releaseId,
        sectionId,
        blockId,
        additionalComment,
      );
      onChange(blockId, [newComment, ...comments]);
    }
  };

  const removeComment = async (commentId: string) => {
    const index = comments.findIndex(comment => comment.id === commentId);
    await releaseContentCommentService.deleteContentSectionComment(commentId);

    const newComments = [...comments];
    newComments.splice(index, 1);
    onChange(blockId, newComments);
  };

  const updateComment = async (commentId: string, content: string) => {
    const index = comments.findIndex(comment => comment.id === commentId);
    const editedComment: UpdateComment = {
      ...comments[index],
      content,
    };

    const savedComment = await releaseContentCommentService.updateContentSectionComment(
      editedComment,
    );

    const newComments = [...comments];
    newComments[index] = savedComment;
    onChange(blockId, newComments);
    setEditingComment(undefined);
  };

  const setResolved = async (commentId: string, resolved: boolean) => {
    const index = comments.findIndex(comment => comment.id === commentId);
    const newComment = { ...comments[index], setResolved: resolved };
    const savedComment = await releaseContentCommentService.updateContentSectionComment(
      newComment,
    );

    const newComments = [...comments];
    newComments[index] = savedComment;
    onChange(blockId, newComments);
  };

  return (
    <div className={styles.container}>
      <Details
        onToggle={isOpen => onToggle && onToggle(isOpen)}
        className="govuk-!-margin-bottom-1 govuk-body-s"
        summary={`${canComment ? `Add / ` : ''}View comments (${
          comments.filter(comment => !comment.resolved).length
        } unresolved)`}
      >
        {canComment && (
          <Formik<FormValues>
            initialValues={{
              content: '',
            }}
            validationSchema={Yup.object({
              content: Yup.string().required('Enter a comment'),
            })}
            onSubmit={async (values, { resetForm }) => {
              await addComment(values.content);
              resetForm();
            }}
          >
            <Form id={`${blockId}-addCommentForm`} showErrorSummary={false}>
              <FormFieldTextArea<FormValues>
                label="Comment"
                name="content"
                rows={3}
              />

              <Button type="submit">Add comment</Button>
            </Form>
          </Formik>
        )}

        <ul className={styles.commentsContainer}>
          {orderBy(comments, ['created'], ['desc']).map(comment => {
            const { createdBy, resolvedBy } = comment;

            const commentDetail = (
              <>
                <p className="govuk-!-font-weight-bold govuk-!-margin-0">
                  {`${createdBy.firstName} ${createdBy.lastName}`}
                </p>

                <dl>
                  <div>
                    <dt>{'Created: '}</dt>
                    <dd>
                      <FormattedDate format="dd/MM/yy HH:mm">
                        {comment.created}
                      </FormattedDate>
                    </dd>
                  </div>
                  {comment.updated && (
                    <div>
                      <dt>{'Updated: '}</dt>
                      <dd>
                        <FormattedDate format="dd/MM/yy HH:mm">
                          {comment.updated}
                        </FormattedDate>
                      </dd>
                    </div>
                  )}
                </dl>
              </>
            );

            if (comment.resolved) {
              return (
                <li key={comment.id}>
                  <p className="govuk-!-font-weight-bold govuk-!-margin-bottom-2">
                    Comment resolved <span aria-hidden>✓</span>
                  </p>

                  <dl>
                    <div>
                      <dt>{'On: '}</dt>
                      <dd>
                        <FormattedDate format="dd/MM/yy HH:mm">
                          {comment.resolved}
                        </FormattedDate>
                      </dd>
                    </div>
                    <div>
                      <dt>{'By: '}</dt>
                      <dd>{`${resolvedBy?.firstName} ${resolvedBy?.lastName}`}</dd>
                    </div>
                  </dl>

                  <Details
                    summary="See comment"
                    className="govuk-!-margin-bottom-2"
                  >
                    {commentDetail}

                    <p>{comment.content}</p>
                  </Details>

                  {canComment && (
                    <ButtonText
                      onClick={async () => {
                        await setResolved(comment.id, false);
                      }}
                    >
                      Unresolve
                    </ButtonText>
                  )}
                </li>
              );
            }

            return (
              <li key={comment.id}>
                {commentDetail}

                <p>{comment.content}</p>

                {editingComment?.id === comment.id ? (
                  <Formik<FormValues>
                    initialValues={{
                      content: editingComment.content,
                    }}
                    validationSchema={Yup.object({
                      content: Yup.string().required('Enter a comment'),
                    })}
                    onSubmit={async values => {
                      await updateComment(comment.id, values.content);
                    }}
                  >
                    <Form
                      id={`${blockId}-editCommentForm`}
                      showErrorSummary={false}
                    >
                      <FormFieldTextArea<FormValues>
                        label="Comment"
                        name="content"
                        id={`${blockId}-editCommentForm-editComment`}
                        rows={3}
                      />

                      <ButtonGroup>
                        <Button type="submit">Update</Button>
                        <ButtonText
                          onClick={() => {
                            setEditingComment(undefined);
                          }}
                        >
                          Cancel
                        </ButtonText>
                      </ButtonGroup>
                    </Form>
                  </Formik>
                ) : (
                  <>
                    {canComment && (
                      <ButtonGroup>
                        {user?.id === createdBy.id && (
                          <>
                            <ButtonText
                              onClick={() => {
                                setEditingComment(comment);
                              }}
                            >
                              Edit
                            </ButtonText>
                            <ButtonText
                              onClick={async () => {
                                await removeComment(comment.id);
                              }}
                            >
                              Delete
                            </ButtonText>
                          </>
                        )}

                        <ButtonText
                          onClick={async () => {
                            await setResolved(comment.id, true);
                          }}
                        >
                          Resolve
                        </ButtonText>
                      </ButtonGroup>
                    )}
                  </>
                )}
              </li>
            );
          })}
        </ul>
      </Details>
    </div>
  );
};

export default Comments;
