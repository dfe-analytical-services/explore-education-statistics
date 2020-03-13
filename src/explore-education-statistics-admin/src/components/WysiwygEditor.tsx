import styles from '@admin/components/wysiwyg.module.scss';
// No types generated for ckeditor 5 for react
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

import classnames from 'classnames';
import React, { ChangeEvent } from 'react';
import marked from 'marked';
import TurndownService from 'turndown';
import ModalConfirm from '@common/components/ModalConfirm';

interface Props {
  editable?: boolean;
  canDelete?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  content: string;
  toolbarConfig?: string[];
  insideAccordion?: boolean;
  useMarkdown?: boolean;
  onContentChange?: (content: string) => Promise<unknown>;
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

export const accordionHeadingConfig = {
  heading: {
    options: [
      { model: 'paragraph', title: 'Paragraph', class: 'ck-heading_paragraph' },
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
};

const WysiwygEditor = ({
  editable,
  canDelete = false,
  content,
  onContentChange,
  insideAccordion,
  toolbarConfig,
  onDelete,
  useMarkdown = false,
}: Props) => {
  const [editing, setEditing] = React.useState(false);
  const [saved, setSaved] = React.useState(false);
  const [temporaryContent, setTemporaryContent] = React.useState(() => {
    if (useMarkdown) return marked(content);
    return content;
  });
  const [showConfirmation, setShowConfirmation] = React.useState(false);

  const turndownService = React.useMemo(() => new TurndownService(), []);

  const save = () => {
    if (onContentChange) {
      let contentChangeContent = temporaryContent;

      if (useMarkdown) {
        contentChangeContent = turndownService.turndown(contentChangeContent);
      }
      onContentChange(contentChangeContent).then(() => {
        setEditing(false);
        setSaved(true);
      });
    } else {
      setEditing(false);
      setSaved(true);
    }
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
              heading: insideAccordion && accordionHeadingConfig.heading,
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
