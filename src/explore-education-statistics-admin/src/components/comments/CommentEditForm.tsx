import { useCommentsContext } from '@admin/contexts/comments/CommentsContext';
import { Comment } from '@admin/services/types/content';
import { UpdateComment } from '@admin/services/releaseContentCommentService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

interface FormValues {
  content: string;
}

interface Props {
  comment: Comment;
  id: string;
  onCancel: () => void;
  onSubmit: () => void;
}

const CommentEditForm = ({ comment, id, onCancel, onSubmit }: Props) => {
  const { onUpdateComment } = useCommentsContext();
  const { content } = comment;
  const [isSubmitting, toggleSubmitting] = useToggle(false);

  const handleSubmit = useFormSubmit(async (values: FormValues) => {
    toggleSubmitting.on();
    const updatedComment: UpdateComment = {
      ...comment,
      content: values.content,
    };
    await onUpdateComment?.(updatedComment);
    onSubmit();
  });

  return (
    <Formik<FormValues>
      initialValues={{
        content,
      }}
      validationSchema={Yup.object({
        content: Yup.string().required('Enter a comment'),
      })}
      onSubmit={handleSubmit}
    >
      <Form id={`${id}-editCommentForm`} showErrorSummary={false}>
        <FormFieldTextArea<FormValues>
          focus
          data-testid="comment-textarea"
          hideLabel
          label="Edit comment"
          name="content"
          rows={3}
        />
        <ButtonGroup className="govuk-!-margin-bottom-0">
          <Button type="submit" disabled={isSubmitting}>
            Update
          </Button>
          <ButtonText onClick={onCancel}>Cancel</ButtonText>
        </ButtonGroup>
      </Form>
    </Formik>
  );
};

export default CommentEditForm;
