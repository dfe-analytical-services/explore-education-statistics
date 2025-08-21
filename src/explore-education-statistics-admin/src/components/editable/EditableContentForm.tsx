import { useCommentsContext } from '@admin/contexts/CommentsContext';
import InvalidContentDetails from '@admin/components/editable/InvalidContentDetails';
import CommentsWrapper from '@admin/components/comments/CommentsWrapper';
import styles from '@admin/components/editable/EditableContentForm.module.scss';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import getInvalidContent, {
  InvalidContentError,
} from '@admin/components/editable/utils/getInvalidContent';
import getInvalidImages from '@admin/components/editable/utils/getInvalidImages';
import getInvalidLinks, {
  InvalidUrl,
} from '@admin/components/editable/utils/getInvalidLinks';
import {
  Element,
  JsonElement,
  ToolbarGroup,
  ToolbarOption,
} from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import WarningMessage from '@common/components/WarningMessage';
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
import React, { useCallback, useMemo, useRef, useState } from 'react';
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
  const [invalidImageErrors, setInvalidImageErrors] = useState<JsonElement[]>(
    [],
  );
  const [invalidContentErrors, setInvalidContentErrors] = useState<
    InvalidContentError[]
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

  const contentErrorDetails = useMemo(() => {
    if (
      invalidImageErrors.length ||
      invalidLinkErrors.length ||
      invalidContentErrors.length
    ) {
      return (
        <>
          <WarningMessage className="govuk-!-margin-bottom-1">
            The following problems must be resolved before saving:
          </WarningMessage>
          {!!invalidImageErrors.length && (
            <InvalidImagesDetails errors={invalidImageErrors} />
          )}
          {!!invalidLinkErrors.length && (
            <InvalidLinksDetails errors={invalidLinkErrors} />
          )}
          {!!invalidContentErrors.length && (
            <InvalidContentDetails errors={invalidContentErrors} />
          )}
        </>
      );
    }
    return null;
  }, [invalidContentErrors, invalidImageErrors, invalidLinkErrors]);

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

                // Convert to json to make it easier to process and test.
                // Have to convert from Record<string | unknown> to unknown then to our
                // JsonElement type to be able to access object properties
                const elementsJson = elements.map(
                  element => element.toJSON() as unknown,
                );

                const invalidImages = getInvalidImages(
                  elementsJson as JsonElement[],
                );
                const invalidLinks = getInvalidLinks(
                  elementsJson as JsonElement[],
                );
                const invalidContent = getInvalidContent(
                  elementsJson as JsonElement[],
                );

                if (
                  invalidImages.length ||
                  invalidLinks.length ||
                  invalidContent.length
                ) {
                  setInvalidImageErrors(invalidImages);
                  setInvalidContentErrors(invalidContent);
                  setInvalidLinkErrors(invalidLinks);

                  const invalidImagesMessage =
                    invalidImages.length === 1
                      ? '1 image does not have alternative text.'
                      : `${invalidImages.length} images do not have alternative text.`;

                  const invalidLinksMessage =
                    invalidLinks.length === 1
                      ? '1 link has an invalid URL.'
                      : `${invalidLinks.length} links have invalid URLs.`;

                  const invalidContentMessage =
                    invalidContent.length === 1
                      ? '1 accessibility error.'
                      : `${invalidContent.length} accessibility errors.`;

                  const errorMessage = `Content errors have been found: ${
                    invalidImages.length ? invalidImagesMessage : ''
                  }  ${invalidLinks.length ? invalidLinksMessage : ''} ${
                    invalidContent.length ? invalidContentMessage : ''
                  }`;

                  return createError({
                    path,
                    message: errorMessage,
                  });
                }

                setInvalidImageErrors([]);
                setInvalidContentErrors([]);
                setInvalidLinkErrors([]);

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
                  contentErrorDetails={contentErrorDetails}
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

function InvalidImagesDetails({ errors }: { errors: JsonElement[] }) {
  return (
    <>
      <p>
        {errors.length === 1
          ? '1 image does not have alternative text.'
          : `${errors.length} images do not have alternative text.`}
      </p>
      <ul>
        <li>
          Alternative text must be added for all images, for guidance see{' '}
          <a
            href="https://www.w3.org/WAI/tutorials/images/tips/"
            rel="noopener noreferrer"
            target="_blank"
          >
            W3C tips on writing alternative text
          </a>
          .
        </li>
        <li>Images without alternative text are outlined in red.</li>
      </ul>
    </>
  );
}

function InvalidLinksDetails({ errors }: { errors: InvalidUrl[] }) {
  return (
    <>
      <p>The following links have invalid URLs:</p>
      <ul>
        {errors.map(error => (
          <li key={error?.text}>
            {error?.text} ({error?.url})
          </li>
        ))}
      </ul>
    </>
  );
}
