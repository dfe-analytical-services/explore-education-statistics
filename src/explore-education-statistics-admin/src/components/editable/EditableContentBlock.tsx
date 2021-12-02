import { useCommentsContext } from '@admin/contexts/comments/CommentsContext';
import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentForm from '@admin/components/editable/EditableContentForm';
import styles from '@admin/components/editable/EditableContentBlock.module.scss';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import toHtml from '@admin/utils/markdown/toHtml';
import Button from '@common/components/Button';
import ContentHtml from '@common/components/ContentHtml';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import sanitizeHtml, {
  defaultSanitizeOptions,
  SanitizeHtmlOptions,
} from '@common/utils/sanitizeHtml';
import classNames from 'classnames';
import React, { useCallback, useMemo } from 'react';

interface EditableContentBlockProps {
  allowComments?: boolean;
  autoSave?: boolean;
  editable?: boolean;
  id: string;
  isSaving?: boolean;
  label: string;
  handleBlur?: (isDirty: boolean) => void;
  hideLabel?: boolean;
  transformImageAttributes?: (
    attributes: Dictionary<string>,
  ) => Dictionary<string>;
  useMarkdown?: boolean;
  value: string;
  onCancel?: () => void;
  onDelete: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSave: (value: string, isAutoSave?: boolean) => void;
}

const EditableContentBlock = ({
  allowComments = false,
  autoSave = false,
  editable = true,
  id,
  isSaving,
  label,
  handleBlur,
  hideLabel = false,
  transformImageAttributes,
  useMarkdown,
  value,
  onCancel,
  onDelete,
  onImageUpload,
  onImageUploadCancel,
  onSave,
}: EditableContentBlockProps) => {
  const { comments } = useCommentsContext();

  const content = useMemo(() => (useMarkdown ? toHtml(value) : value), [
    useMarkdown,
    value,
  ]);

  const [isEditing, toggleEditing] = useToggle(false);

  const sanitizeOptions: SanitizeHtmlOptions = useMemo(() => {
    const commentTags = [
      'comment-start',
      'comment-end',
      'resolvedcomment-start',
      'resolvedcomment-end',
    ];
    const commentAttributes = {
      'comment-start': ['name'],
      'comment-end': ['name'],
      'resolvedcomment-start': ['name'],
      'resolvedcomment-end': ['name'],
    };

    return {
      ...defaultSanitizeOptions,
      allowedTags: defaultSanitizeOptions.allowedTags
        ? [...defaultSanitizeOptions.allowedTags, ...commentTags]
        : [],
      allowedAttributes: defaultSanitizeOptions.allowedAttributes
        ? { ...defaultSanitizeOptions.allowedAttributes, ...commentAttributes }
        : {},
      transformTags: {
        img: (tagName, attribs) => {
          return {
            tagName,
            attribs: transformImageAttributes
              ? transformImageAttributes(attribs)
              : attribs,
          };
        },
      },
    };
  }, [transformImageAttributes]);

  const handleSave = useCallback(
    (nextValue: string, isAutoSave?: boolean) => {
      if (!isAutoSave) {
        toggleEditing.off();
      }
      // No need to handle useMarkdown case
      // as Admin API now converts MarkDownBlocks
      // to HtmlBlocks

      onSave(nextValue, isAutoSave);
    },
    [toggleEditing, onSave],
  );

  if (isEditing) {
    return (
      <EditableContentForm
        allowComments={allowComments}
        autoSave={autoSave}
        content={content ? sanitizeHtml(content, sanitizeOptions) : ''} // NOTE: Sanitize to transform img src attribs
        label={label}
        handleBlur={handleBlur}
        hideLabel={hideLabel}
        id={id}
        isSaving={isSaving}
        onImageUpload={onImageUpload}
        onImageUploadCancel={onImageUploadCancel}
        onCancel={() => {
          toggleEditing.off();
          onCancel?.();
        }}
        onSubmit={handleSave}
      />
    );
  }

  return (
    <>
      {allowComments && comments.length > 0 && (
        <div className={styles.commentsButtonContainer}>
          <Button
            variant="secondary"
            onClick={toggleEditing.on}
            testId="view-comments"
          >
            View comments
            <br />
            <span className="govuk-!-margin-top-1 govuk-body-s">
              ({comments.filter(comment => !comment.resolved).length}{' '}
              unresolved)
            </span>
          </Button>
        </div>
      )}
      <EditableBlockWrapper
        onEdit={editable ? toggleEditing.on : undefined}
        onDelete={editable ? onDelete : undefined}
      >
        <div
          className={classNames(styles.preview, {
            [styles.readOnly]: !isEditing,
          })}
        >
          {editable ? (
            <div
              className={styles.editButton}
              role="button"
              tabIndex={0}
              onClick={toggleEditing.on}
              onKeyPress={e => {
                switch (e.key) {
                  case 'Enter':
                  case ' ':
                    toggleEditing.on();
                    break;
                  default:
                    break;
                }
              }}
            >
              <ContentHtml
                html={content || '<p>This section is empty</p>'}
                sanitizeOptions={sanitizeOptions}
              />
            </div>
          ) : (
            <ContentHtml
              html={content || '<p>This section is empty</p>'}
              sanitizeOptions={sanitizeOptions}
            />
          )}
        </div>
      </EditableBlockWrapper>
    </>
  );
};

export default EditableContentBlock;
