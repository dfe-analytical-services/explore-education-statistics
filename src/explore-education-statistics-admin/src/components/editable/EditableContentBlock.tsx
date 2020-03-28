import EditableProps from '@admin/components/editable/types/EditableProps';
import FormEditor, { FormEditorProps } from '@admin/components/form/FormEditor';
import styles from '@admin/components/form/FormEditor.module.scss';
import toHtml from '@admin/utils/markdown/toHtml';
import toMarkdown from '@admin/utils/markdown/toMarkdown';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import React, { useCallback, useState } from 'react';

interface EditableContentBlockRendererProps
  extends EditableProps,
    OmitStrict<FormEditorProps, 'onChange'> {
  id: string;
  onSave: (value: string) => void;
  useMarkdown?: boolean;
}

const EditableContentBlock = ({
  canDelete = false,
  editable,
  hideLabel = true,
  useMarkdown,
  value,
  onSave,
  onDelete,
  ...props
}: EditableContentBlockRendererProps) => {
  const [content, setContent] = useState(() => {
    if (useMarkdown) {
      return toHtml(value);
    }

    return value;
  });

  const [editing, toggleEditing] = useToggle(false);
  const [showConfirmation, toggleConfirmation] = useToggle(false);

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

  return editing ? (
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
  ) : (
    <div>
      <ModalConfirm
        title="Delete section"
        mounted={showConfirmation}
        onConfirm={() => {
          if (onDelete) {
            onDelete();
          }

          toggleConfirmation.off();
        }}
        onExit={toggleConfirmation.off}
        onCancel={toggleConfirmation.off}
      >
        <p>Are you sure you want to delete this section?</p>
      </ModalConfirm>

      <div
        role="button"
        className={classNames({
          [styles.readOnly]: editable && !editing,
        })}
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
        <div className={styles.readOnlyButtons}>
          <Button variant="secondary" className={styles.readOnlyButton}>
            Edit
          </Button>
          {canDelete && (
            <>
              <Button
                variant="warning"
                className={styles.readOnlyButton}
                onClick={e => {
                  e.preventDefault();
                  e.stopPropagation();
                  toggleConfirmation.on();
                }}
              >
                Delete
              </Button>
            </>
          )}
        </div>

        <div
          className={styles.preview}
          // eslint-disable-next-line react/no-danger
          dangerouslySetInnerHTML={{
            __html: content || '<p>This section is empty</p>',
          }}
        />
      </div>
    </div>
  );
};

export default EditableContentBlock;
