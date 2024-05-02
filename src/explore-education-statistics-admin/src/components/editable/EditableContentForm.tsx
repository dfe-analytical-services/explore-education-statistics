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
import parseNumber from '@common/utils/number/parseNumber';

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
  const [accessibilityErrors, setAccessibilityErrors] = useState<
    AccessibilityError[]
  >([]);
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

  console.log('accessibilityErrors', accessibilityErrors);

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
      <div>
        <h3>Accessibility errors</h3>
        {accessibilityErrors.map((error, index) => (
          <p key={`${index.toString()}`}>
            <strong>{error.type}</strong>: {error.message}
          </p>
        ))}
      </div>
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
                  onChange={els => {
                    if (els) {
                      setElements(els);
                      const errs = checkStuff(els);
                      setAccessibilityErrors(errs);
                    }
                  }}
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

function isHeading(element: Element) {
  return (
    element.name === 'heading3' ||
    element.name === 'heading4' ||
    element.name === 'heading5'
  );
}

interface AccessibilityError {
  type:
    | 'clickHereLink'
    | 'matchingLinkText'
    | 'oneWordLinkText'
    | 'urlLinkText'
    | 'headingLevel'
    | 'table'
    | 'boldAsHeading'
    | 'emptyHeading';
  message?: string;
}
function checkStuff(elements: Element[]) {
  const errors: AccessibilityError[] = [];
  elements.forEach(element => {
    // Check for lines that are entirely bold so should be headings
    if (element.name === 'paragraph') {
      const children = Array.from(element.getChildren());
      if (children.length === 1 && children[0].getAttribute('bold')) {
        // console.log('bold not a heading', element);
        errors.push({
          type: 'boldAsHeading',
          message: children[0].toJSON().data as string,
        });
      }
    }

    // Check for empty heading elements
    // Could use css to highlight this?
    if (isHeading(element) && element.isEmpty) {
      // console.log('is empty heading', element);
      errors.push({
        type: 'emptyHeading',
        message: 'Empty heading element present',
      });
    }

    // Check for table header
    if (
      element.name === 'table' &&
      !element.getAttribute('headingRows') &&
      !element.getAttribute('headingColumns')
    ) {
      // console.log('table has no headers', element);
      errors.push({
        type: 'table',
        message: 'All tables should have a header row or column',
      });
    }
  });

  const allHeadings = elements.filter(element => isHeading(element));

  // Check if headings only increase by one
  allHeadings.forEach((heading, index) => {
    if (index === 0) {
      return;
    }
    const level = parseNumber(heading.name.split('heading')[1]);
    const previousLevel = parseNumber(
      allHeadings[index - 1].name.split('heading')[1],
    );
    if (
      level &&
      previousLevel &&
      level > previousLevel &&
      previousLevel - 1 !== level
    ) {
      // console.log('skipped a level', heading, level, previousLevel);
      errors.push({
        type: 'headingLevel',
        message: `h${previousLevel} to h${level}`,
      });
    }
  });

  const flat = elements.flatMap(element =>
    Array.from(element.getChildren()).flatMap(child => child),
  );

  const allLinks = flat.reduce<{ text: string; href: string }[]>((acc, el) => {
    if (el.getAttribute('linkHref')) {
      const json = el.toJSON();
      acc.push({
        text: json.data as string,
        href: (json.attributes as Record<string, unknown>).linkHref as string,
      });
    }
    return acc;
  }, []);

  // Check for repeated link text within the same page where the links are different
  allLinks.forEach(link => {
    const matchingText = allLinks.filter(
      l => l.text === link.text && l.href !== link.href,
    );

    if (matchingText.length) {
      // console.log('has matching link text', link);
      errors.push({
        type: 'matchingLinkText',
        message: link.text,
      });
    }
  });

  // Check for links with only one word, just urls and click here
  allLinks.forEach(link => {
    if (link.text.toLowerCase().trim() === 'click here') {
      // console.log('link is click here');
      errors.push({
        type: 'clickHereLink',
        message: 'Do not use "click here" as link text',
      });
      return;
    }
    if (link.text.split(' ').length === 1) {
      if (link.text.startsWith('http')) {
        // console.log('link text is url', link);
        errors.push({
          type: 'urlLinkText',
          message: link.text,
        });
        return;
      }
      // console.log('link text is jsut one word', link);
      errors.push({
        type: 'oneWordLinkText',
        message: link.text,
      });
    }
  });

  // console.log('errors', errors);
  return errors;

  // attribute linkOpenInNewTab

  // hasAttribute instead of get?
}
