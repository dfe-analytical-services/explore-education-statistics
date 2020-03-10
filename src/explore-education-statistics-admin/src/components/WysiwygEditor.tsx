import styles from '@admin/components/wysiwyg.module.scss';
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
// No types generated for ckeditor 5 for react
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
import ModalConfirm from '@common/components/ModalConfirm';
import classnames from 'classnames';
import marked from 'marked';
import React, { ChangeEvent, useMemo, useState } from 'react';
import TurndownService from 'turndown';

interface Props {
  editable?: boolean;
  canDelete?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  content: string;
  toolbarConfig?: string[];
  useMarkdown?: boolean;
  onContentChange: (content: string) => void;
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

const WysiwygEditor = ({
  editable,
  canDelete = false,
  content,
  onContentChange,
  toolbarConfig,
  onDelete,
  useMarkdown = false,
}: Props) => {
  const [editing, setEditing] = useState(false);
  const [saved, setSaved] = useState(false);
  const [temporaryContent, setTemporaryContent] = useState(() => {
    if (useMarkdown) return marked(content);
    return content;
  });
  const [showConfirmation, setShowConfirmation] = useState(false);

  const turndownService = useMemo(() => new TurndownService(), []);

  const save = () => {
    setEditing(false);
    let contentChangeContent = temporaryContent;

    if (useMarkdown) {
      contentChangeContent = turndownService.turndown(contentChangeContent);
    }

    onContentChange(contentChangeContent);
    setSaved(true);
  };

  return (
    <div>
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
        className={classnames({
          [styles.editableContentEditing]: editable && editing,
          [styles.editableContent]: editable && !editing,
          [styles.unsaved]: editable && !saved,
        })}
        onClick={() => {
          if (!editing) setEditing(editable === true);
        }}
        tabIndex={undefined}
      >
        {editable && (
          <div className={styles.editableButton}>
            {editing ? (
              <button
                className="govuk-button"
                onClick={() => save()}
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
              toolbar: toolbarConfig || toolbarConfigs.full,
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
              styles.wysiwygPreview
            } govuk-!-padding-left-2 govuk-!-padding-right-2 ${!temporaryContent &&
              styles.wysiwygPlaceholder}`}
          />
        )}
      </div>
    </div>
  );
};

export default WysiwygEditor;
