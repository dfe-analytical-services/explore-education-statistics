import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import FormEditor, { FormEditorProps } from '@admin/components/form/FormEditor';
import toHtml from '@admin/utils/markdown/toHtml';
import toMarkdown from '@admin/utils/markdown/toMarkdown';
import Button from '@common/components/Button';
import useToggle from '@common/hooks/useToggle';
import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import styles from './EditableContentBlock.module.scss';

interface EditableContentBlockProps
  extends OmitStrict<FormEditorProps, 'onChange'> {
  editable?: boolean;
  id: string;
  onSave: (value: string) => void;
  onDelete: () => void;
  useMarkdown?: boolean;
}

const EditableContentBlock = ({
  editable,
  hideLabel = true,
  useMarkdown,
  value,
  onSave,
  onDelete,
  ...props
}: EditableContentBlockProps) => {
  const [content, setContent] = useState(() => {
    if (useMarkdown) {
      return toHtml(value);
    }

    return value;
  });

  const [editing, toggleEditing] = useToggle(false);

  const handleSave = useCallback(() => {
    toggleEditing.off();

    let nextValue = content;

    if (useMarkdown) {
      nextValue = toMarkdown(nextValue);
    }

    onSave(nextValue);
  }, [onSave, content, toggleEditing, useMarkdown]);

  const handleCancel = useCallback(() => {
    toggleEditing.off();

    let nextContent = value;

    if (useMarkdown) {
      nextContent = toHtml(nextContent);
    }

    setContent(nextContent);
  }, [toggleEditing, useMarkdown, value]);

  if (onSave && editing) {
    return (
      <>
        <FormEditor
          {...props}
          hideLabel={hideLabel}
          value={content}
          onChange={setContent}
        />

        <div>
          <Button onClick={handleSave}>Save</Button>
          <Button variant="secondary" onClick={handleCancel}>
            Cancel
          </Button>
        </div>
      </>
    );
  }

  return (
    <EditableBlockWrapper
      onEdit={editable ? toggleEditing.on : undefined}
      onDelete={editable ? onDelete : undefined}
    >
      <div
        className={classNames(styles.preview, {
          [styles.readOnly]: !editing,
        })}
      >
        {editable ? (
          <div
            className={styles.editButton}
            role="button"
            tabIndex={0}
            // eslint-disable-next-line react/no-danger
            dangerouslySetInnerHTML={{
              __html: content || '<p>This section is empty</p>',
            }}
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
          />
        ) : (
          <div
            // eslint-disable-next-line react/no-danger
            dangerouslySetInnerHTML={{
              __html: content || '<p>This section is empty</p>',
            }}
          />
        )}
      </div>
    </EditableBlockWrapper>
  );
};

export default EditableContentBlock;
