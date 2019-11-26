import styles from '@admin/pages/prototypes/components/PrototypeEditableContent.module.scss';
// No types generated for ckeditor 5 for react
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

import classnames from 'classnames';
import React, { ChangeEvent } from 'react';
import marked from 'marked';
import TurndownService from 'turndown';

interface Props {
  editable?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  content: string;
  useMarkdown?: boolean;
  onContentChange?: (content: string) => Promise<unknown>;
}

const WysiwygEditor = ({
  editable,
  content,
  onContentChange,
  useMarkdown = false,
}: Props) => {
  const [editing, setEditing] = React.useState(false);
  const [saved, setSaved] = React.useState(false);
  const [temporaryContent, setTemporaryContent] = React.useState(() => {
    if (useMarkdown) return marked(content);
    return content;
  });

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
                <span className="govuk-button govuk-body-s govuk-!-margin-bottom-0">
                  Edit this section
                </span>
              </div>
            )}
          </div>
        )}

        {editable && editing ? (
          <CKEditor
            editor={ClassicEditor}
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
              __html: temporaryContent,
            }}
            className="govuk-!-padding-left-1 govuk-!-padding-right-1"
          />
        )}
      </div>
    </div>
  );
};

export default WysiwygEditor;
