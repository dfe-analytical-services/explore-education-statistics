import { useCommentsContext } from '@admin/contexts/comments/CommentsContext';
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
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { Formik } from 'formik';
import React, { useCallback, useRef } from 'react';
import classNames from 'classnames';

interface FormValues {
  content: string;
}

export interface Props {
  allowComments?: boolean;
  autoSave?: boolean;
  content: string;
  hideLabel?: boolean;
  handleBlur?: (isDirty: boolean) => void;
  id: string;
  isSaving?: boolean;
  label: string;
  onCancel: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSubmit: (content: string, isAutoSave?: boolean) => void;
}

const EditableContentForm = ({
  allowComments = false,
  autoSave = false,
  content,
  hideLabel = false,
  handleBlur,
  id,
  isSaving = false,
  label,
  onCancel,
  onImageUpload,
  onImageUploadCancel,
  onSubmit,
}: Props) => {
  const { comments, onDeletePendingComments } = useCommentsContext();
  const containerRef = useRef<HTMLDivElement>(null);
  const [showCommentAddForm, toggleCommentAddForm] = useToggle(false);

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

  return (
    <div className={styles.container} ref={containerRef}>
      {showCommentAddForm && (
        <CommentAddForm
          blockId={id}
          containerRef={containerRef}
          onCancel={() => {
            toggleCommentAddForm.off();
          }}
          onSave={() => {
            toggleCommentAddForm.off();
          }}
        />
      )}
      <div
        className={classNames(styles.commentsSidebar, {
          [styles.showCommentAddForm]: showCommentAddForm,
        })}
      >
        {allowComments && comments.length > 0 && <CommentsList blockId={id} />}
      </div>

      <div className={styles.form}>
        <Formik<FormValues>
          initialValues={{
            content,
          }}
          validateOnChange={false}
          validationSchema={Yup.object({
            content: Yup.string().required('Enter content'),
          })}
          onSubmit={async values => {
            await onDeletePendingComments?.();
            onSubmit(values.content);
          }}
        >
          <Form id={`${id}-form`}>
            <FormFieldEditor<FormValues>
              id={id}
              allowComments={allowComments}
              focusOnInit
              handleBlur={handleBlur}
              hideLabel={hideLabel}
              label={label}
              name="content"
              validateElements={validateElements}
              onAutoSave={values => {
                onSubmit(values, true);
              }}
              onCancelComment={toggleCommentAddForm.off}
              onClickAddComment={toggleCommentAddForm.on}
              onImageUpload={onImageUpload}
              onImageUploadCancel={onImageUploadCancel}
            />

            <ButtonGroup>
              <Button type="submit" disabled={isSaving}>
                {autoSave ? 'Save & close' : 'Save'}
              </Button>
              <LoadingSpinner
                inline
                hideText
                loading={autoSave && isSaving}
                size="md"
                text="Saving"
              />
              {!autoSave && (
                <Button variant="secondary" onClick={onCancel}>
                  Cancel
                </Button>
              )}
            </ButtonGroup>
          </Form>
        </Formik>
      </div>
    </div>
  );
};

export default EditableContentForm;
