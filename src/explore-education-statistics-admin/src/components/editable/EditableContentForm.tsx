import { useCommentsContext } from '@admin/contexts/CommentsContext';
import styles from '@admin/components/editable/EditableContentForm.module.scss';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { Element } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import CommentAddForm from '@admin/components/comments/CommentAddForm';
import CommentsList from '@admin/components/comments/CommentsList';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form } from '@common/components/form';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import classNames from 'classnames';
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
  const { comments, clearPendingDeletions } = useCommentsContext();

  const containerRef = useRef<HTMLDivElement>(null);
  const [showCommentAddForm, toggleCommentAddForm] = useToggle(false);

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

  const validateElements = useCallback((elements: Element[]) => {
    let error: string | undefined;

    elements.some(element => {
      if (element.name === 'image' && !element.getAttribute('alt')) {
        error = 'All images must have alternative (alt) text';
        return true;
      }

      return false;
    });

    return error;
  }, []);

  const [
    { isLoading: isAutoSaving, error: autoSaveError },
    handleAutoSave,
  ] = useAsyncCallback(
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
    <div className={styles.container} ref={containerRef}>
      {allowComments && (
        <div data-testid="comments-sidebar">
          {showCommentAddForm && (
            <CommentAddForm
              baseId={id}
              containerRef={containerRef}
              onCancel={toggleCommentAddForm.off}
              onSave={toggleCommentAddForm.off}
            />
          )}
          {comments.length > 0 && (
            <CommentsList
              className={classNames(styles.commentsList, {
                [styles.padTop]: showCommentAddForm,
              })}
            />
          )}
        </div>
      )}

      <div className={styles.form}>
        <Formik<FormValues>
          initialValues={{
            content,
          }}
          validateOnChange={false}
          validationSchema={Yup.object({
            content: Yup.string().required('Enter content'),
          })}
          onSubmit={handleSubmit}
        >
          {form => {
            const isSaving = form.isSubmitting || isAutoSaving;

            return (
              <Form id={`${id}-form`}>
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
    </div>
  );
};
export default EditableContentForm;
