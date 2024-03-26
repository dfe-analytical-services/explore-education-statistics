import { useCommentsContext } from '@admin/contexts/CommentsContext';
import CommentsWrapper from '@admin/components/comments/CommentsWrapper';
import styles from '@admin/components/editable/EditableContentForm.module.scss';
import RHFFormFieldEditor from '@admin/components/form/RHFFormFieldEditor';
import {
  Element,
  Node,
  ToolbarGroup,
  ToolbarOption,
} from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import formatContentLink from '@common/utils/url/formatContentLink';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import sanitizeHtml, {
  SanitizeHtmlOptions,
  commentTagAttributes,
  commentTags,
  defaultSanitizeOptions,
} from '@common/utils/sanitizeHtml';
import React, { useCallback, useMemo, useRef, useState } from 'react';
import { useIdleTimer } from 'react-idle-timer';

export interface InvalidUrl {
  text: string;
  url: string;
}

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
  const [altTextError, setAltTextError] = useState<string>();
  const [elements, setElements] = useState<Element[] | undefined>();
  const [invalidLinkErrors, setInvalidLinkErrors] = useState<InvalidUrl[]>([]);
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
              href: formatContentLink(attribs.href),
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
              .test('validate alt text', (value, { createError, path }) => {
                const errorMessage = elements?.length
                  ? validateAltText(elements)
                  : '';
                if (errorMessage) {
                  setAltTextError(errorMessage);
                  return createError({
                    path,
                    message: errorMessage,
                  });
                }
                return true;
              })
              .test('validate links', (_, { createError, path }) => {
                const invalidLinks = elements?.length
                  ? getInvalidLinks(elements)
                  : [];
                if (invalidLinks.length) {
                  setInvalidLinkErrors(invalidLinks);
                  return createError({
                    path,
                    message: `${
                      invalidLinks.length === 1
                        ? '1 link has an invalid URL.'
                        : `${invalidLinks.length} links have invalid URLs.`
                    }`,
                  });
                }
                return true;
              }),
          })}
        >
          {({ formState }) => {
            const isSaving = formState.isSubmitting || isAutoSaving;
            return (
              <RHFForm
                id={`${id}-form`}
                visuallyHiddenErrorSummary
                onSubmit={handleSubmit}
              >
                <RHFFormFieldEditor<FormValues>
                  allowComments={allowComments}
                  altTextError={altTextError}
                  error={
                    autoSaveError || submitError
                      ? 'Could not save content'
                      : undefined
                  }
                  focusOnInit
                  hideLabel={hideLabel}
                  invalidLinkErrors={invalidLinkErrors}
                  label={label}
                  name="content"
                  toolbarConfig={toolbarConfig}
                  onAutoSave={handleAutoSave}
                  onBlur={onBlur}
                  onChange={setElements}
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
              </RHFForm>
            );
          }}
        </FormProvider>
      </div>
    </CommentsWrapper>
  );
};
export default EditableContentForm;

function validateAltText(els: Element[]): string {
  const hasInvalidImage = els.some(
    element =>
      isInvalidImage(element) ||
      Array.from(element.getChildren()).some(child => isInvalidImage(child)),
  );

  return hasInvalidImage ? 'All images must have alternative text.' : '';
}

function isInvalidImage(element: Element | Node) {
  return (
    (element.name === 'imageBlock' || element.name === 'imageInline') &&
    !element.getAttribute('alt')
  );
}

function getInvalidLinks(elements: Element[]) {
  return elements
    .flatMap(element =>
      Array.from(element.getChildren()).flatMap(child => child),
    )
    .reduce<InvalidUrl[]>((acc, el) => {
      if (!el.getAttribute('linkHref')) {
        return acc;
      }
      const jsonEl = el.toJSON();
      const attributes = jsonEl.attributes as Record<string, unknown>;
      const url = attributes.linkHref as string;

      try {
        // exclude anchor links, localhost and emails as they fail Yup url validation.
        if (
          url &&
          !url.startsWith('#') &&
          !url.startsWith('http://localhost') &&
          !url.startsWith('mailto:')
        ) {
          Yup.string().url().validateSync(url.trim());
        }
      } catch {
        acc.push({
          text: jsonEl.data as string,
          url,
        });
      }
      return acc;
    }, []);
}
