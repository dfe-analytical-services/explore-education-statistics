import styles from '@admin/components/form/FormEditor.module.scss';
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
// No types generated for ckeditor 5 for react
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
import ErrorMessage from '@common/components/ErrorMessage';
import ModalConfirm from '@common/components/ModalConfirm';
import classNames from 'classnames';
import marked from 'marked';
import React, { ChangeEvent, useCallback, useState } from 'react';
import Turndown from 'turndown';

const turndownService = new Turndown();

export interface FormEditorProps {
  allowHeadings?: boolean;
  canDelete?: boolean;
  value: string;
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
  const [editing, setEditing] = useState(false);
  const [saved, setSaved] = useState(false);

  const [temporaryContent, setTemporaryContent] = useState(() => {
    if (useMarkdown) {
      return marked(value);
    }

    return value;
  });
  const [showConfirmation, setShowConfirmation] = useState(false);

  const handleSave = useCallback(() => {
    setEditing(false);
    let contentChangeContent = temporaryContent;

    if (useMarkdown) {
      contentChangeContent = turndownService.turndown(contentChangeContent);
    }

    onChange(contentChangeContent);
    setSaved(true);
  }, [onChange, temporaryContent, useMarkdown]);

  return (
    <div>
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

      <ModalConfirm
        onConfirm={() => {
          if (onDelete) onDelete();
          setShowConfirmation(false);
        }}
        onExit={() => {
          setShowConfirmation(false);
        }}
        onCancel={() => {
          setShowConfirmation(false);
        }}
        title="Delete section"
        mounted={showConfirmation}
      >
        <p>Are you sure you want to delete this section?</p>
      </ModalConfirm>

      {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events,jsx-a11y/interactive-supports-focus */}
      <div
        role="button"
        className={classNames({
          [styles.editableContentEditing]: editable && editing,
          [styles.editableContent]: editable && !editing,
          [styles.unsaved]: editable && !saved,
        })}
        onClick={() => {
          if (!editing) {
            setEditing(editable === true);
          }
        }}
        tabIndex={undefined}
      >
        {editable && (
          <div className={styles.editableButton}>
            {editing ? (
              <button
                className="govuk-button"
                onClick={handleSave}
                type="button"
              >
                Save
              </button>
            ) : (
              <div className={styles.editableButtonContent}>
                <span className="govuk-button govuk-button--secondary govuk-body-s govuk-!-margin-bottom-0">
                  Edit
                </span>
                {canDelete && (
                  <>
                    <button
                      type="button"
                      onClick={e => {
                        e.preventDefault();
                        e.stopPropagation();
                        setShowConfirmation(true);
                      }}
                      className="govuk-button govuk-button--warning govuk-body-s govuk-!-margin-bottom-0"
                    >
                      Delete
                    </button>
                  </>
                )}
              </div>
            )}
          </div>
        )}

        {editable && editing ? (
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
            data={temporaryContent}
            onChange={(event: ChangeEvent, editor: { getData(): string }) => {
              setTemporaryContent(editor.getData());
            }}
            onInit={(editor: { editing: { view: { focus(): void } } }) => {
              editor.editing.view.focus();
            }}
          />
        ) : (
          <div
            // eslint-disable-next-line react/no-danger
            dangerouslySetInnerHTML={{
              __html: temporaryContent || '<p>This section is empty</p>',
            }}
            className={`${
              styles.preview
            } govuk-!-padding-left-2 govuk-!-padding-right-2 ${!temporaryContent &&
              styles.previewPlaceholder}`}
          />
        )}
      </div>
    </div>
  );
};

export default FormEditor;
