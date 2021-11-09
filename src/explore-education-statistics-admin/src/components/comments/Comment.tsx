import styles from '@admin/components/comments/Comment.module.scss';
import { Comment as CommentType } from '@admin/services/types/content';
import FormattedDate from '@common/components/FormattedDate';
import { useAuthContext } from '@admin/contexts/AuthContext';
import classNames from 'classnames';
import releaseContentCommentService, {
  UpdateComment,
} from '@admin/services/releaseContentCommentService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import Yup from '@common/validation/yup';
import useToggle from '@common/hooks/useToggle';
import { Formik } from 'formik';
import React, { useEffect, useRef } from 'react';

interface FormValues {
  content: string;
}

export interface toggleResolveCommentHandler {
  comment: CommentType;
  resolve?: boolean;
  updateMarker?: boolean;
}

interface Props {
  active: boolean;
  comment: CommentType;
  isPendingDeletion: boolean;
  onCommentRemoved: (id: string) => void;
  onCommentResolved: ({
    comment,
    resolve,
    updateMarker,
  }: toggleResolveCommentHandler) => void;
  onCommentSelect: (id: string) => void;
  onCommentUpdated: (comment: CommentType) => void;
}
const Comment = ({
  active,
  comment,
  isPendingDeletion,
  onCommentRemoved,
  onCommentResolved,
  onCommentSelect,
  onCommentUpdated,
}: Props) => {
  const {
    content,
    created,
    createdBy,
    id,
    resolved,
    resolvedBy,
    updated,
  } = comment;
  const [isEditingComment, toggleIsEditingComment] = useToggle(false);
  const [isSubmitting, toggleSubmitting] = useToggle(false);
  const ref = useRef<HTMLDivElement>(null);
  const { user } = useAuthContext();

  useEffect(() => {
    if (active) {
      ref.current?.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
        inline: 'start',
      });
    }
  }, [active]);

  const updateComment = async (updatedContent: string) => {
    toggleSubmitting.on();
    const editedComment: UpdateComment = {
      ...comment,
      content: updatedContent,
    };
    const updatedComment = await releaseContentCommentService.updateContentSectionComment(
      editedComment,
    );
    onCommentUpdated(updatedComment);
    toggleSubmitting.off();
    toggleIsEditingComment.off();
  };

  const handleCommentSelection = () => {
    if (!comment.resolved) {
      onCommentSelect(comment.id);
    }
  };

  if (isPendingDeletion) {
    return null;
  }

  return (
    <div
      aria-label="Comment"
      className={classNames(styles.comment, {
        [styles.active]: active,
      })}
      ref={ref}
      role="button"
      tabIndex={0}
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
        <br />
        <FormattedDate format="dd/MM/yy HH:mm">{created}</FormattedDate>
        {updated && (
          <>
            {' '}
            (Updated{' '}
            <FormattedDate format="dd/MM/yy HH:mm">{updated}</FormattedDate>)
          </>
        )}
      </p>

      {isEditingComment ? (
        <Formik<FormValues>
          initialValues={{
            content,
          }}
          validationSchema={Yup.object({
            content: Yup.string().required('Enter a comment'),
          })}
          onSubmit={async values => {
            await updateComment(values.content);
          }}
        >
          <Form id={`${id}-editCommentForm`} showErrorSummary={false}>
            <FormFieldTextArea<FormValues>
              label="Edit comment"
              hideLabel
              name="content"
              data-testid="comment-textarea"
              rows={3}
            />
            <ButtonGroup>
              <Button type="submit" disabled={isSubmitting}>
                Update
              </Button>
              <ButtonText onClick={toggleIsEditingComment.off}>
                Cancel
              </ButtonText>
            </ButtonGroup>
          </Form>
        </Formik>
      ) : (
        <>
          <div
            className="govuk-!-margin-bottom-3 govuk-!-margin-top-2"
            data-testid="comment-content"
          >
            {content}
          </div>

          {resolved ? (
            <>
              <p className="govuk-!-margin-bottom-0 govuk-body-s">
                Resolved by {resolvedBy?.firstName} {resolvedBy?.lastName} on{' '}
                <FormattedDate format="dd/MM/yy HH:mm">
                  {resolved}
                </FormattedDate>
              </p>
              <ButtonText
                onClick={async () => {
                  await onCommentResolved({ comment });
                }}
              >
                Unresolve
              </ButtonText>
            </>
          ) : (
            <ButtonGroup className="govuk-!-margin-bottom-0">
              <Button
                onClick={async () => {
                  await onCommentResolved({ comment });
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
                      onCommentRemoved(id);
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
  );
};

export default Comment;
