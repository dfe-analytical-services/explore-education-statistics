import { useCommentsContext } from '@admin/contexts/CommentsContext';
import { Comment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
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

export default function CommentEditForm({
  comment,
  id,
  onCancel,
  onSubmit,
}: Props) {
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
          <Form
            id={`${id}-editCommentForm`}
            showErrorSummary={false}
            onSubmit={handleSubmit}
          >
            <FormFieldTextArea<FormValues>
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
          </Form>
        );
      }}
    </FormProvider>
  );
}
