import styles from '@admin/components/comments/CommentAddForm.module.scss';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import usePinElementToContainer from '@common/hooks/usePinElementToContainer';
import Yup from '@common/validation/yup';
import React, { RefObject, useEffect, useRef } from 'react';
import classNames from 'classnames';

interface FormValues {
  content: string;
}

interface Props {
  baseId: string;
  containerRef: RefObject<HTMLDivElement | null>;
  onCancel: () => void;
  onSave: () => void;
}

export default function CommentAddForm({
  baseId,
  containerRef,
  onCancel,
  onSave,
}: Props) {
  const { addComment, setCurrentInteraction } = useCommentsContext();

  const ref = useRef<HTMLDivElement>(null);
  const textAreaRef = useRef<HTMLTextAreaElement>(null);

  const { focus, positionStyle } = usePinElementToContainer(ref, containerRef);

  useEffect(() => {
    if (focus) {
      textAreaRef.current?.focus();
    }
  }, [focus]);

  const handleSubmit = async (values: FormValues) => {
    const newComment = await addComment({
      content: values.content,
    });
    if (newComment) {
      onSave();
    }
  };

  return (
    <div
      className={classNames(styles.container, positionStyle)}
      data-testid="comment-add-form"
      ref={ref}
    >
      <FormProvider
        initialValues={{
          content: '',
        }}
        validationSchema={Yup.object({
          content: Yup.string().required('Enter a comment'),
        })}
      >
        {({ formState }) => {
          return (
            <Form
              id={`${baseId}-commentAddForm`}
              showErrorSummary={false}
              onSubmit={handleSubmit}
            >
              <FormFieldTextArea<FormValues>
                label="Comment"
                hideLabel
                name="content"
                rows={3}
                inputRef={textAreaRef}
              />
              <ButtonGroup className="govuk-!-margin-bottom-2">
                <Button type="submit" disabled={formState.isSubmitting}>
                  Add comment
                </Button>

                <ButtonText
                  onClick={() => {
                    setCurrentInteraction?.({
                      type: 'removing',
                      id: 'commentplaceholder',
                    });
                    onCancel();
                  }}
                >
                  Cancel
                </ButtonText>
              </ButtonGroup>
            </Form>
          );
        }}
      </FormProvider>
    </div>
  );
}
