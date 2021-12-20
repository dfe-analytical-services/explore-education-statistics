import { useCommentsContext } from '@admin/contexts/CommentsContext';
import { Comment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useMounted from '@common/hooks/useMounted';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useRef } from 'react';

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
  const { content } = comment;

  const { updateComment } = useCommentsContext();

  const textAreaRef = useRef<HTMLTextAreaElement>(null);

  useMounted(() => {
    textAreaRef?.current?.focus();
  });

  const handleSubmit = useFormSubmit(async (values: FormValues) => {
    await updateComment({
      ...comment,
      content: values.content,
    });
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
      {form => (
        <Form id={`${id}-editCommentForm`} showErrorSummary={false}>
          <FormFieldTextArea<FormValues>
            data-testid="comment-textarea"
            hideLabel
            label="Comment"
            name="content"
            rows={3}
            textAreaRef={textAreaRef}
          />
          <ButtonGroup className="govuk-!-margin-bottom-0">
            <Button type="submit" disabled={form.isSubmitting}>
              Update
            </Button>
            <ButtonText onClick={onCancel}>Cancel</ButtonText>
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
};

export default CommentEditForm;
