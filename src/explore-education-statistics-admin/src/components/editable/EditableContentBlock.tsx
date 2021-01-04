import ButtonGroup from '@common/components/ButtonGroup';
import SanitizeHtml from '@common/components/SanitizeHtml';
import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import FormEditor, { FormEditorProps } from '@admin/components/form/FormEditor';
import toHtml from '@admin/utils/markdown/toHtml';
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
  editable = true,
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

    // No need to handle useMarkdown case
    // as Admin API now converts MarkDownBlocks
    // to HtmlBlocks

    onSave(content);
  }, [onSave, content, toggleEditing]);

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
          focusOnInit
          onChange={setContent}
        />

        <ButtonGroup>
          <Button onClick={handleSave}>Save</Button>
          <Button variant="secondary" onClick={handleCancel}>
            Cancel
          </Button>
        </ButtonGroup>
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
            <SanitizeHtml
              dirtyHtml={content || '<p>This section is empty</p>'}
            />
          </div>
        ) : (
          <SanitizeHtml dirtyHtml={content || '<p>This section is empty</p>'} />
        )}
      </div>
    </EditableBlockWrapper>
  );
};

export default EditableContentBlock;
