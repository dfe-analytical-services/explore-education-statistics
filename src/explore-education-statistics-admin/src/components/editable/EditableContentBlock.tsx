import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentForm from '@admin/components/editable/EditableContentForm';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import toHtml from '@admin/utils/markdown/toHtml';
import ContentHtml from '@common/components/ContentHtml';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import sanitizeHtml, {
  defaultSanitizeOptions,
  SanitizeHtmlOptions,
} from '@common/utils/sanitizeHtml';
import classNames from 'classnames';
import React, { useCallback, useMemo } from 'react';
import styles from './EditableContentBlock.module.scss';

interface EditableContentBlockProps {
  editable?: boolean;
  id: string;
  label: string;
  hideLabel?: boolean;
  value: string;
  handleBlur?: (isDirty: boolean) => void;
  onCancel?: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSave: (value: string) => void;
  onDelete: () => void;
  transformImageAttributes?: (
    attributes: Dictionary<string>,
  ) => Dictionary<string>;
  useMarkdown?: boolean;
}

const EditableContentBlock = ({
  editable = true,
  id,
  label,
  hideLabel = false,
  value,
  handleBlur,
  onCancel,
  onImageUpload,
  onImageUploadCancel,
  onSave,
  onDelete,
  transformImageAttributes,
  useMarkdown,
}: EditableContentBlockProps) => {
  const content = useMemo(() => (useMarkdown ? toHtml(value) : value), [
    useMarkdown,
    value,
  ]);

  const [isEditing, toggleEditing] = useToggle(false);

  const sanitizeOptions: SanitizeHtmlOptions = useMemo(() => {
    return {
      ...defaultSanitizeOptions,
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
    (nextValue: string) => {
      toggleEditing.off();

      // No need to handle useMarkdown case
      // as Admin API now converts MarkDownBlocks
      // to HtmlBlocks

      onSave(nextValue);
    },
    [onSave, toggleEditing],
  );

  if (isEditing) {
    return (
      <EditableContentForm
        id={id}
        label={label}
        hideLabel={hideLabel}
        content={content ? sanitizeHtml(content, sanitizeOptions) : ''} // NOTE: Sanitize to transform img src attribs
        onImageUpload={onImageUpload}
        onImageUploadCancel={onImageUploadCancel}
        onCancel={() => {
          toggleEditing.off();
          if (onCancel) {
            onCancel();
          }
        }}
        onSubmit={handleSave}
        handleBlur={handleBlur}
      />
    );
  }

  return (
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
  );
};

export default EditableContentBlock;
