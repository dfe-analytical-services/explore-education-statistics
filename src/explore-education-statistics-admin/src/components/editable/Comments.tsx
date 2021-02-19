import { useAuthContext } from '@admin/contexts/AuthContext';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
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
  const { releaseId } = useManageReleaseContext();

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

  return (
    <div className={styles.container}>
      <Details
        onToggle={isOpen => onToggle && onToggle(isOpen)}
        className="govuk-!-margin-bottom-1 govuk-body-s"
        summary={`${canComment ? `Add / ` : ''}View comments (${
          comments.length
        })`}
      >
        {canComment && (
          <Formik<FormValues>
            initialValues={{
              content: '',
            }}
            validationSchema={Yup.object({
              content: Yup.string().required('Enter a comment'),
            })}
            onSubmit={values => {
              addComment(values.content);
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
          {comments.map((comment, index) => {
            const { createdBy } = comment;

            return (
              <li key={comment.id} className="govuk-body-s">
                <p className="govuk-!-margin-0">
                  <strong>
                    {`${createdBy.firstName} ${createdBy.lastName}`}
                  </strong>
                </p>
                <p>
                  {comment.updated ? 'Updated: ' : ''}
                  <FormattedDate format="d MMMM yyyy HH:mm">
                    {comment.updated || comment.created}
                  </FormattedDate>
                </p>

                {editingComment?.id === comment.id ? (
                  <Formik<FormValues>
                    initialValues={{
                      content: editingComment.content,
                    }}
                    validationSchema={Yup.object({
                      content: Yup.string().required('Enter a comment'),
                    })}
                    onSubmit={values => {
                      updateComment(index, values.content);
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
                    <p>{comment.content}</p>

                    {canComment && user?.id === createdBy.id && (
                      <ButtonGroup>
                        <ButtonText
                          onClick={() => {
                            setEditingComment(comment);
                          }}
                        >
                          Edit
                        </ButtonText>

                        <ButtonText
                          onClick={() => {
                            removeComment(index);
                          }}
                        >
                          Delete
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
