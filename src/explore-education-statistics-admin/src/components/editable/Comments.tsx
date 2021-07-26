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

  const addComment = (content: string) => {
    const additionalComment: AddComment = {
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
        .then(newComment => {
          onChange(blockId, [newComment, ...comments]);
        });
    }
  };

  const removeComment = (commentId: string) => {
    const index = comments.findIndex(comment => comment.id === commentId);
    releaseContentCommentService
      .deleteContentSectionComment(commentId)
      .then(() => {
        const newComments = [...comments];
        newComments.splice(index, 1);
        onChange(blockId, newComments);
      });
  };

  const updateComment = (commentId: string, content: string) => {
    const index = comments.findIndex(comment => comment.id === commentId);
    const editedComment: UpdateComment = {
      ...comments[index],
      content,
    };

    releaseContentCommentService
      .updateContentSectionComment(editedComment)
      .then(savedComment => {
        const newComments = [...comments];
        newComments[index] = savedComment;

        onChange(blockId, newComments);

        setEditingComment(undefined);
      });
  };

  const resolveComment = (commentId: string, resolved: boolean) => {
    const index = comments.findIndex(comment => comment.id === commentId);
    releaseContentCommentService
      .updateContentSectionComment(comments[index], resolved)
      .then(savedComment => {
        const newComments = [...comments];
        newComments[index] = savedComment;
        onChange(blockId, newComments);
      });
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
            onSubmit={(values, { resetForm }) => {
              addComment(values.content);
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

            return comment.resolved ? (
              <li className="govuk-body-s">
                <p className="govuk-!-margin-0">
                  <strong>Comment resolved</strong>
                </p>
                <p className="govuk-!-margin-0">
                  <FormattedDate format="dd/MM/yy HH:mm">
                    {comment.resolved}
                  </FormattedDate>
                </p>
                <p className="govuk-!-margin-0">
                  {`by ${resolvedBy?.firstName} ${resolvedBy?.lastName}`}
                </p>
                <Details className="govuk-!-margin-0" summary="See comment">
                  <p className="govuk-!-margin-0">
                    <strong>
                      {`${createdBy.firstName} ${createdBy.lastName}`}
                    </strong>
                  </p>
                  <p className="govuk-!-margin-0">
                    {'Created: '}
                    <FormattedDate format="dd/MM/yy HH:mm">
                      {comment.created}
                    </FormattedDate>
                  </p>
                  {comment.updated && (
                    <p className="govuk-!-margin-0">
                      {'Updated: '}
                      <FormattedDate format="dd/MM/yy HH:mm">
                        {comment.updated}
                      </FormattedDate>
                    </p>
                  )}
                  <p className="govuk-!-margin-top-3">{comment.content}</p>
                </Details>
                <ButtonText
                  onClick={() => {
                    resolveComment(comment.id, false);
                  }}
                >
                  Unresolve
                </ButtonText>
                <hr />
              </li>
            ) : (
              <li key={comment.id} className="govuk-body-s">
                <p className="govuk-!-margin-0">
                  <strong>
                    {`${createdBy.firstName} ${createdBy.lastName}`}
                  </strong>
                </p>
                <p className="govuk-!-margin-0">
                  {'Created: '}
                  <FormattedDate format="dd/MM/yy HH:mm">
                    {comment.created}
                  </FormattedDate>
                </p>
                {comment.updated && (
                  <p className="govuk-!-margin-0">
                    {'Updated: '}
                    <FormattedDate format="dd/MM/yy HH:mm">
                      {comment.updated}
                    </FormattedDate>
                  </p>
                )}

                {editingComment?.id === comment.id ? (
                  <Formik<FormValues>
                    initialValues={{
                      content: editingComment.content,
                    }}
                    validationSchema={Yup.object({
                      content: Yup.string().required('Enter a comment'),
                    })}
                    onSubmit={values => {
                      updateComment(comment.id, values.content);
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
                    <p className="govuk-!-margin-top-3">{comment.content}</p>
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
                              onClick={() => {
                                removeComment(comment.id);
                              }}
                            >
                              Delete
                            </ButtonText>
                          </>
                        )}

                        <ButtonText
                          onClick={() => {
                            resolveComment(comment.id, true);
                          }}
                        >
                          Resolve
                        </ButtonText>
                      </ButtonGroup>
                    )}
                  </>
                )}

                <hr />
              </li>
            );
          })}
        </ul>
      </Details>
    </div>
  );
};

export default Comments;
