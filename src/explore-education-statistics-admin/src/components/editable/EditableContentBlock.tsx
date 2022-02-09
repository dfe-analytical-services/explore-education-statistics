import { useCommentsContext } from '@admin/contexts/CommentsContext';
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
import { Dictionary } from '@common/types';
import sanitizeHtml, {
  defaultSanitizeOptions,
  SanitizeHtmlOptions,
} from '@common/utils/sanitizeHtml';
import classNames from 'classnames';
import React, { useMemo } from 'react';

interface EditableContentBlockProps {
  allowComments?: boolean;
  editable?: boolean;
  id: string;
  isEditing?: boolean;
  isSaving?: boolean;
  label: string;
  hideLabel?: boolean;
  transformImageAttributes?: (
    attributes: Dictionary<string>,
  ) => Dictionary<string>;
  useMarkdown?: boolean;
  value: string;
  onAutoSave?: (value: string) => void;
  onBlur?: (isDirty: boolean) => void;
  onCancel?: () => void;
  onDelete: () => void;
  onEditing: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSubmit: (value: string) => void;
}

const EditableContentBlock = ({
  allowComments = false,
  editable = true,
  id,
  isEditing,
  isSaving,
  label,
  hideLabel = false,
  transformImageAttributes,
  useMarkdown,
  value,
  onAutoSave,
  onBlur,
  onCancel,
  onDelete,
  onEditing,
  onImageUpload,
  onImageUploadCancel,
  onSubmit,
}: EditableContentBlockProps) => {
  const { comments } = useCommentsContext();

  const content = useMemo(() => (useMarkdown ? toHtml(value) : value), [
    useMarkdown,
    value,
  ]);

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

  if (isEditing) {
    return (
      <EditableContentForm
        allowComments={allowComments}
        content={content ? sanitizeHtml(content, sanitizeOptions) : ''} // NOTE: Sanitize to transform img src attribs
        label={label}
        hideLabel={hideLabel}
        id={id}
        isSaving={isSaving}
        onAutoSave={onAutoSave}
        onBlur={onBlur}
        onImageUpload={onImageUpload}
        onImageUploadCancel={onImageUploadCancel}
        onCancel={onCancel}
        onSubmit={onSubmit}
      />
    );
  }

  return (
    <>
      {allowComments && comments.length > 0 && (
        <div className={styles.commentsButtonContainer}>
          <Button
            variant="secondary"
            testId="view-comments"
            onClick={onEditing}
          >
            View comments
            <br />
            <span className="govuk-!-margin-top-1 govuk-body-s">
              {`(${
                comments.filter(comment => !comment.resolved).length
              } unresolved)`}
            </span>
          </Button>
        </div>
      )}
      <EditableBlockWrapper
        onEdit={editable ? onEditing : undefined}
        onDelete={editable ? onDelete : undefined}
      >
        <div
          className={classNames(styles.preview, {
            [styles.readOnly]: !isEditing,
          })}
        >
          {editable ? (
            // eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions
            <div className={styles.editButton} onClick={onEditing}>
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
