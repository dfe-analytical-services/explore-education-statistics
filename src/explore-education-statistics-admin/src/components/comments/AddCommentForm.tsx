import styles from '@admin/components/comments/AddCommentForm.module.scss';
import releaseContentCommentService, {
  AddComment,
} from '@admin/services/releaseContentCommentService';
import { Comment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
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
  releaseId?: string;
  sectionId?: string;
  onCancel: () => void;
  onSave: (comment: Comment) => void;
}

const AddCommentForm = ({
  blockId,
  containerRef,
  releaseId,
  sectionId,
  onCancel,
  onSave,
}: Props) => {
  const [isSubmitting, toggleSubmitting] = useToggle(false);
  const [fixPosition, toggleFixPosition] = useToggle(false);
  const [focus, toggleFocus] = useToggle(false);
  const ref = useRef<HTMLDivElement>(null);

  useMounted(() => {
    const setPosition = () => {
      const formEl = ref.current?.getBoundingClientRect();
      const containerEl = containerRef?.current?.getBoundingClientRect();

      if (!formEl || !containerEl) {
        return;
      }

      if (containerEl.top >= 0 || containerEl.bottom <= formEl?.height) {
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

  const addComment = async (content: string) => {
    const additionalComment: AddComment = {
      content,
    };
    toggleSubmitting.on();

    if (releaseId && sectionId) {
      const theBlockId = blockId?.replace('block-', '');
      const newComment = await releaseContentCommentService.addContentSectionComment(
        releaseId,
        sectionId,
        theBlockId,
        additionalComment,
      );

      return onSave(newComment);
    }
    return toggleSubmitting.off();
  };

  return (
    <div className={classNames(styles.container, { fixPosition })} ref={ref}>
      <Formik<FormValues>
        initialValues={{
          content: '',
        }}
        validationSchema={Yup.object({
          content: Yup.string().required('Enter a comment'),
        })}
        onSubmit={async values => {
          await addComment(values.content);
        }}
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

export default AddCommentForm;
