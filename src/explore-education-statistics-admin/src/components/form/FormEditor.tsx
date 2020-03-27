import styles from '@admin/components/form/FormEditor.module.scss';
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
// No types generated for ckeditor 5 for react
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
import Button from '@common/components/Button';
import ErrorMessage from '@common/components/ErrorMessage';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import marked from 'marked';
import React, { ChangeEvent, useCallback, useState } from 'react';
import Turndown from 'turndown';

const turndownService = new Turndown();

export interface FormEditorProps {
  allowHeadings?: boolean;
  canDelete?: boolean;
  editable?: boolean;
  error?: string;
  hideLabel?: boolean;
  hint?: string;
  id: string;
  label: string;
  reviewing?: boolean;
  resolveComments?: boolean;
  toolbarConfig?: string[];
  useMarkdown?: boolean;
  value: string;
  onChange: (content: string) => void;
  onDelete?: () => void;
}

export const toolbarConfigs = {
  full: [
    'heading',
    '|',
    'bold',
    'italic',
    'link',
    '|',
    'bulletedList',
    'numberedList',
    '|',
    'blockQuote',
    'insertTable',
    '|',
    'redo',
    'undo',
  ],
  reduced: ['bold', 'link', '|', 'bulletedList'],
};

const FormEditor = ({
  allowHeadings,
  canDelete = false,
  editable,
  error,
  hideLabel,
  hint,
  id,
  label,
  toolbarConfig = toolbarConfigs.full,
  value,
  onChange,
  onDelete,
  useMarkdown = false,
}: FormEditorProps) => {
  const [editing, toggleEditing] = useToggle(false);

  const [content, setContent] = useState(() => {
    if (useMarkdown) {
      return marked(value);
    }

    return value;
  });
  const [showConfirmation, toggleConfirmation] = useToggle(false);

  const handleSave = useCallback(() => {
    toggleEditing.off();

    let nextValue = content;

    if (useMarkdown) {
      nextValue = turndownService.turndown(nextValue);
    }

    onChange(nextValue);
  }, [onChange, content, toggleEditing, useMarkdown]);

  const handleCancel = useCallback(() => {
    toggleEditing.off();

    if (useMarkdown) {
      return setContent(marked(value));
    }

    return setContent(value);
  }, [toggleEditing, useMarkdown, value]);

  return (
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

      {editing ? (
        <>
          <span
            className={classNames('govuk-label', {
              'govuk-visually-hidden': hideLabel,
            })}
          >
            {label}
          </span>

          {hint && (
            <span id={`${id}-hint`} className="govuk-hint">
              {hint}
            </span>
          )}

          {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}

          <div className={styles.editor}>
            <CKEditor
              editor={ClassicEditor}
              config={{
                toolbar: toolbarConfig,
                heading: allowHeadings && {
                  options: [
                    {
                      model: 'paragraph',
                      title: 'Paragraph',
                      class: 'ck-heading_paragraph',
                    },
                    {
                      model: 'heading3',
                      view: 'h3',
                      title: 'Heading 3',
                      class: 'ck-heading_heading3',
                    },
                    {
                      model: 'heading4',
                      view: 'h4',
                      title: 'Heading 4',
                      class: 'ck-heading_heading4',
                    },
                    {
                      model: 'heading5',
                      view: 'h5',
                      title: 'Heading 5',
                      class: 'ck-heading_heading5',
                    },
                  ],
                },
              }}
              data={content}
              onChange={(event: ChangeEvent, editor: { getData(): string }) => {
                setContent(editor.getData());
              }}
              onInit={(editor: { editing: { view: { focus(): void } } }) => {
                editor.editing.view.focus();
              }}
            />
          </div>

          <div>
            <Button onClick={handleSave}>Save</Button>
            <Button variant="secondary" onClick={handleCancel}>
              Cancel
            </Button>
          </div>
        </>
      ) : (
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
      )}
    </div>
  );
};

export default FormEditor;
