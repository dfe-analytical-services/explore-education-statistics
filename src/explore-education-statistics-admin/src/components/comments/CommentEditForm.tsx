import { useCommentsContext } from '@admin/contexts/CommentsContext';
import { Comment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import useMounted from '@common/hooks/useMounted';
import Yup from '@common/validation/yup';
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

  const handleSubmit = async (values: FormValues) => {
    await updateComment({
      ...comment,
      content: values.content,
    });
    onSubmit();
  };

  return (
    <FormProvider
      initialValues={{
        content,
      }}
      validationSchema={Yup.object({
        content: Yup.string().required('Enter a comment'),
      })}
    >
      {({ formState }) => {
        return (
          <RHFForm
            id={`${id}-editCommentForm`}
            showErrorSummary={false}
            onSubmit={handleSubmit}
          >
            <RHFFormFieldTextArea<FormValues>
              hideLabel
              label="Comment"
              name="content"
              rows={3}
              inputRef={textAreaRef}
            />
            <ButtonGroup className="govuk-!-margin-bottom-0">
              <Button type="submit" disabled={formState.isSubmitting}>
                Update
              </Button>
              <ButtonText onClick={onCancel}>Cancel</ButtonText>
            </ButtonGroup>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
};

export default CommentEditForm;
