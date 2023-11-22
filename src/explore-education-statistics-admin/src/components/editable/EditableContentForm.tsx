import { useCommentsContext } from '@admin/contexts/CommentsContext';
import CommentsWrapper from '@admin/components/comments/CommentsWrapper';
import styles from '@admin/components/editable/EditableContentForm.module.scss';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { Element, Node } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form } from '@common/components/form';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { Formik, FormikHelpers } from 'formik';
import React, { useCallback, useRef } from 'react';
import { useIdleTimer } from 'react-idle-timer';

interface FormValues {
  content: string;
}

export interface Props {
  actionThrottle?: number;
  allowComments?: boolean;
  content: string;
  hideLabel?: boolean;
  id: string;
  idleTimeout?: number;
  label: string;
  onAction?: () => void;
  onAutoSave?: (content: string) => void;
  onBlur?: (isDirty: boolean) => void;
  onCancel?: () => void;
  onIdle?: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSubmit: (content: string) => void;
}

const EditableContentForm = ({
  actionThrottle = 5_000,
  allowComments = false,
  content,
  hideLabel = false,
  id,
  idleTimeout = 600_000,
  label,
  onAction,
  onAutoSave,
  onBlur,
  onCancel,
  onIdle,
  onImageUpload,
  onImageUploadCancel,
  onSubmit,
}: Props) => {
  const { clearPendingDeletions } = useCommentsContext();

  const containerRef = useRef<HTMLDivElement>(null);
  const [showCommentAddForm, toggleCommentAddForm] = useToggle(false);
  const [showAltTextMessage, toggleAltTextMessage] = useToggle(false);

  useIdleTimer({
    element: containerRef.current ?? document,
    // Disable cross tab in tests as it seems to
    // prevent idle callback from being triggered.
    crossTab: process.env.NODE_ENV !== 'test',
    throttle: actionThrottle,
    timeout: idleTimeout,
    onAction,
    onIdle,
  });

  const validateElements = useCallback(
    (elements: Element[]) => {
      const hasInvalidImage = elements.some(
        element =>
          isInvalidImage(element) ||
          Array.from(element.getChildren()).some(child =>
            isInvalidImage(child),
          ),
      );

      toggleAltTextMessage(hasInvalidImage);

      return hasInvalidImage
        ? 'All images must have alternative text'
        : undefined;
    },
    [toggleAltTextMessage],
  );

  const [{ isLoading: isAutoSaving, error: autoSaveError }, handleAutoSave] =
    useAsyncCallback(
      async (nextContent: string) => {
        await onAutoSave?.(nextContent);
      },
      [onAutoSave],
    );

  const handleSubmit = useCallback(
    async (values: FormValues, helpers: FormikHelpers<FormValues>) => {
      try {
        await clearPendingDeletions?.();
        await onSubmit(values.content);
      } catch (err) {
        logger.error(err);
        helpers.setFieldError('content', 'Could not save content');
      }
    },
    [clearPendingDeletions, onSubmit],
  );

  return (
    <CommentsWrapper
      allowComments={allowComments}
      commentType="inline"
      id={id}
      showCommentAddForm={showCommentAddForm}
      onAddCancel={toggleCommentAddForm.off}
      onAddSave={toggleCommentAddForm.off}
      onAdd={toggleCommentAddForm.on}
    >
      <div className={styles.form}>
        <Formik<FormValues>
          initialValues={{
            content,
          }}
          validationSchema={Yup.object({
            content: Yup.string().required('Enter content'),
          })}
          onSubmit={handleSubmit}
        >
          {form => {
            const isSaving = form.isSubmitting || isAutoSaving;

            return (
              <Form id={`${id}-form`} visuallyHiddenErrorSummary>
                {showAltTextMessage && <AltTextWarningMessage />}
                <FormFieldEditor<FormValues>
                  allowComments={allowComments}
                  error={autoSaveError ? 'Could not save content' : undefined}
                  focusOnInit
                  hideLabel={hideLabel}
                  label={label}
                  name="content"
                  validateElements={validateElements}
                  onAutoSave={handleAutoSave}
                  onBlur={onBlur}
                  onCancelComment={toggleCommentAddForm.off}
                  onClickAddComment={toggleCommentAddForm.on}
                  onImageUpload={onImageUpload}
                  onImageUploadCancel={onImageUploadCancel}
                />

                <ButtonGroup>
                  <Button type="submit" disabled={isSaving}>
                    {onAutoSave ? 'Save & close' : 'Save'}
                  </Button>
                  {!onAutoSave && onCancel && (
                    <Button variant="secondary" onClick={onCancel}>
                      Cancel
                    </Button>
                  )}

                  <LoadingSpinner
                    inline
                    hideText
                    loading={isSaving}
                    size="md"
                    text="Saving"
                  />
                </ButtonGroup>
              </Form>
            );
          }}
        </Formik>
      </div>
    </CommentsWrapper>
  );
};
export default EditableContentForm;

function isInvalidImage(element: Node) {
  return (
    (element.name === 'imageBlock' || element.name === 'imageInline') &&
    !element.getAttribute('alt')
  );
}

export function AltTextWarningMessage() {
  return (
    <WarningMessage>
      Alternative text must be added for images, for guidance see{' '}
      <a
        href="https://www.w3.org/WAI/tutorials/images/tips/"
        rel="noopener noreferrer"
        target="_blank"
      >
        W3C tips on writing alternative text
      </a>
      . <br />
      Images without alternative text are outlined in red.
    </WarningMessage>
  );
}
