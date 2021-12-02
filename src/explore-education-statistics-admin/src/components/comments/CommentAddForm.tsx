import styles from '@admin/components/comments/CommentAddForm.module.scss';
import { useCommentsContext } from '@admin/contexts/comments/CommentsContext';
import { AddComment } from '@admin/services/releaseContentCommentService';
import { Comment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';

import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import useFormSubmit from '@common/hooks/useFormSubmit';
import Yup from '@common/validation/yup';
import useMounted from '@common/hooks/useMounted';
import { Formik } from 'formik';
import React, { RefObject, useRef } from 'react';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';

interface FormValues {
  content: string;
}

interface Props {
  blockId: string;
  containerRef?: RefObject<HTMLDivElement>;
  onCancel: () => void;
  onSave: (comment: Comment) => void;
}

const CommentAddForm = ({ blockId, containerRef, onCancel, onSave }: Props) => {
  const { onAddComment } = useCommentsContext();
  const [isSubmitting, toggleSubmitting] = useToggle(false);
  const [fixPosition, toggleFixPosition] = useToggle(false);
  const [focus, toggleFocus] = useToggle(false);
  const ref = useRef<HTMLDivElement>(null);

  useMounted(() => {
    const setPosition = () => {
      const formRect = ref.current?.getBoundingClientRect();
      const containerRect = containerRef?.current?.getBoundingClientRect();

      if (!formRect || !containerRect) {
        return;
      }

      if (containerRect.top >= 0 || containerRect.bottom <= formRect.height) {
        toggleFixPosition.off();
        return;
      }
      toggleFixPosition.on();
    };

    setPosition();
    toggleFocus.on();

    window.addEventListener('scroll', setPosition);

    return () => window.removeEventListener('scroll', setPosition);
  });

  const handleSubmit = useFormSubmit(async (values: FormValues) => {
    const additionalComment: AddComment = {
      content: values.content,
    };
    toggleSubmitting.on();
    const newComment = await onAddComment?.(additionalComment);
    if (newComment) {
      return onSave(newComment);
    }
    return toggleSubmitting.off();
  });

  return (
    <div
      className={classNames(styles.container, {
        [styles.fixPosition]: fixPosition,
      })}
      ref={ref}
    >
      <Formik<FormValues>
        initialValues={{
          content: '',
        }}
        validationSchema={Yup.object({
          content: Yup.string().required('Enter a comment'),
        })}
        onSubmit={handleSubmit}
      >
        <Form id={`${blockId}-addCommentForm`} showErrorSummary={false}>
          <FormFieldTextArea<FormValues>
            focus={focus}
            label="Add comment"
            hideLabel
            name="content"
            data-testid="comment-textarea"
            rows={3}
          />
          <ButtonGroup className="govuk-!-margin-bottom-2">
            <Button type="submit" disabled={isSubmitting}>
              Add comment
            </Button>
            <ButtonText onClick={onCancel}>Cancel</ButtonText>
          </ButtonGroup>
        </Form>
      </Formik>
    </div>
  );
};

export default CommentAddForm;
