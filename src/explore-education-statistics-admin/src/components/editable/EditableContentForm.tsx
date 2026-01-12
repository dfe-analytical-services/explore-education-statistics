import { useCommentsContext } from '@admin/contexts/CommentsContext';
import CommentsWrapper from '@admin/components/comments/CommentsWrapper';
import styles from '@admin/components/editable/EditableContentForm.module.scss';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { Element, ToolbarGroup, ToolbarOption } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import formatContentLinkUrl from '@common/utils/url/formatContentLinkUrl';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import sanitizeHtml, {
  SanitizeHtmlOptions,
  commentTagAttributes,
  commentTags,
  defaultSanitizeOptions,
} from '@common/utils/sanitizeHtml';
import React, {
  ReactNode,
  useCallback,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useIdleTimer } from 'react-idle-timer';
import getContentErrors from '@admin/components/editable/utils/getContentErrors';

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
  toolbarConfig?:
    | ReadonlyArray<ToolbarOption | ToolbarGroup>
    | Array<ToolbarOption | ToolbarGroup>;
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
  toolbarConfig,
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
  const [elements, setElements] = useState<Element[] | undefined>();
  const [invalidContentErrors, setInvalidContentErrors] = useState<ReactNode>();
  const [submitError, setSubmitError] = useState<string>();

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

  const sanitizeOptions: SanitizeHtmlOptions = useMemo(() => {
    return {
      ...defaultSanitizeOptions,
      allowedTags: [
        ...(defaultSanitizeOptions.allowedTags ?? []),
        ...commentTags,
      ],
      allowedAttributes: {
        ...(defaultSanitizeOptions.allowedAttributes ?? {}),
        ...commentTagAttributes,
      },
      transformTags: {
        a: (tagName, attribs) => {
          return {
            tagName,
            attribs: {
              ...attribs,
              href: formatContentLinkUrl(attribs.href),
            },
          };
        },
      },
    };
  }, []);

  const [{ isLoading: isAutoSaving, error: autoSaveError }, handleAutoSave] =
    useAsyncCallback(
      async (nextContent: string) => {
        await onAutoSave?.(sanitizeHtml(nextContent, sanitizeOptions));
      },
      [onAutoSave],
    );

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      try {
        await clearPendingDeletions?.();
        await onSubmit(sanitizeHtml(values.content, sanitizeOptions));
      } catch (err) {
        logger.error(err);
        setSubmitError('Could not save content');
      }
    },
    [clearPendingDeletions, sanitizeOptions, onSubmit],
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
        <FormProvider
          initialValues={{
            content,
          }}
          validationSchema={Yup.object({
            content: Yup.string()
              .required('Enter content')
              .test('validate content', (_, { createError, path }) => {
                if (!elements?.length) {
                  return true;
                }
                const contentErrors = getContentErrors(elements);

                if (contentErrors) {
                  const { errorMessage, contentErrorDetails } = contentErrors;
                  setInvalidContentErrors(contentErrorDetails);

                  return createError({
                    path,
                    message: errorMessage,
                  });
                }
                setInvalidContentErrors(undefined);

                return true;
              }),
          })}
        >
          {({ formState }) => {
            const isSaving = formState.isSubmitting || isAutoSaving;
            return (
              <Form
                id={`${id}-form`}
                visuallyHiddenErrorSummary
                onSubmit={handleSubmit}
              >
                <FormFieldEditor<FormValues>
                  allowComments={allowComments}
                  contentErrorDetails={invalidContentErrors}
                  error={
                    autoSaveError || submitError
                      ? 'Could not save content'
                      : undefined
                  }
                  focusOnInit
                  hideLabel={hideLabel}
                  label={label}
                  name="content"
                  toolbarConfig={toolbarConfig}
                  onAutoSave={handleAutoSave}
                  onBlur={onBlur}
                  onChange={setElements}
                  onCancelComment={toggleCommentAddForm.off}
                  onClickAddComment={toggleCommentAddForm.on}
                  onElementsReady={setElements}
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
        </FormProvider>
      </div>
    </CommentsWrapper>
  );
};
export default EditableContentForm;
