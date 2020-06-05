import { useAuthContext } from '@admin/contexts/AuthContext';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import releaseContentCommentService, {
  AddExtendedComment,
  UpdateExtendedComment,
} from '@admin/services/releaseContentCommentService';
import { ExtendedComment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import { Form } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormattedDate from '@common/components/FormattedDate';
import Yup from '@common/validation/yup';
import classNames from 'classnames';
import { Formik } from 'formik';
import React, { useRef, useState } from 'react';
import styles from './Comments.module.scss';

interface FormValues {
  content: string;
}

export type CommentsChangeHandler = (
  blockId: string,
  comments: ExtendedComment[],
) => void;

interface Props {
  blockId: string;
  sectionId: string;
  comments: ExtendedComment[];
  canComment?: boolean;
  onChange: CommentsChangeHandler;
}

const Comments = ({
  blockId,
  sectionId,
  comments = [],
  onChange,
  canComment = false,
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);

  const [editingComment, setEditingComment] = useState<ExtendedComment>();

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

        setEditingComment(undefined);
      });
  };

  return (
    <>
      <div
        role="presentation"
        ref={ref}
        className={classNames('dfe-comment-block', styles.addComment)}
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
                  id={`${blockId}-addCommentForm-content`}
                  rows={3}
                />

                <Button type="submit">Add comment</Button>
              </Form>
            </Formik>
          )}

          <div className={styles.commentsContainer}>
            {comments.map((comment, index) => {
              const { createdBy } = comment;

              return (
                <div key={comment.id}>
                  <p className="govuk-body-s govuk-!-margin-0">
                    <strong>
                      {`${createdBy.firstName} ${createdBy.lastName}`}
                    </strong>
                  </p>
                  <p className="govuk-body-xs">
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
                      <p className="govuk-body-xs govuk-!-margin-bottom-1">
                        {comment.content}
                      </p>

                      {canComment && user?.id === createdBy.id && (
                        <ButtonGroup>
                          <ButtonText
                            className="govuk-body-xs"
                            onClick={() => {
                              setEditingComment(comment);
                            }}
                          >
                            Edit
                          </ButtonText>

                          <ButtonText
                            className="govuk-body-xs"
                            onClick={() => {
                              removeComment(index);
                            }}
                          >
                            Remove
                          </ButtonText>
                        </ButtonGroup>
                      )}
                    </>
                  )}

                  <hr />
                </div>
              );
            })}
          </div>
        </Details>
      </div>
    </>
  );
};

export default Comments;
