import styles from '@admin/components/comments/CommentAddForm.module.scss';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import usePinElementToContainer from '@common/hooks/usePinElementToContainer';
import Yup from '@common/validation/yup';
import React, { RefObject, useEffect, useRef } from 'react';
import classNames from 'classnames';

interface FormValues {
  content: string;
}

interface Props {
  baseId: string;
  containerRef: RefObject<HTMLDivElement>;
  onCancel: () => void;
  onSave: () => void;
}

const CommentAddForm = ({ baseId, containerRef, onCancel, onSave }: Props) => {
  const { addComment, setCurrentInteraction } = useCommentsContext();

  const ref = useRef<HTMLDivElement>(null);
  const textAreaRef = useRef<HTMLTextAreaElement>(null);

  const { focus, positionStyle } = usePinElementToContainer(ref, containerRef);

  useEffect(() => {
    if (focus) {
      textAreaRef?.current?.focus();
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
    <div className={classNames(styles.container, positionStyle)} ref={ref}>
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
            <RHFForm
              id={`${baseId}-commentAddForm`}
              showErrorSummary={false}
              onSubmit={handleSubmit}
            >
              <RHFFormFieldTextArea<FormValues>
                label="Comment"
                hideLabel
                name="content"
                data-testid="comment-textarea"
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
            </RHFForm>
          );
        }}
      </FormProvider>
    </div>
  );
};

export default CommentAddForm;
