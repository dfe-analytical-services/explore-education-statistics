import styles from '@admin/components/comments/CommentAddForm.module.scss';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import useFormSubmit from '@common/hooks/useFormSubmit';
import usePinElementToContainer from '@common/hooks/usePinElementToContainer';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
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

  const handleSubmit = useFormSubmit(async (values: FormValues) => {
    const newComment = await addComment({
      content: values.content,
    });
    if (newComment) {
      onSave();
    }
  });

  return (
    <div className={classNames(styles.container, positionStyle)} ref={ref}>
      <Formik<FormValues>
        initialValues={{
          content: '',
        }}
        validationSchema={Yup.object({
          content: Yup.string().required('Enter a comment'),
        })}
        onSubmit={handleSubmit}
      >
        {form => (
          <Form id={`${baseId}-commentAddForm`} showErrorSummary={false}>
            <FormFieldTextArea<FormValues>
              label="Comment"
              hideLabel
              name="content"
              data-testid="comment-textarea"
              rows={3}
              textAreaRef={textAreaRef}
            />
            <ButtonGroup className="govuk-!-margin-bottom-2">
              <Button type="submit" disabled={form.isSubmitting}>
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
        )}
      </Formik>
    </div>
  );
};

export default CommentAddForm;
