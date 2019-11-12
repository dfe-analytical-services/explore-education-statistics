import styles from '@admin/pages/prototypes/components/PrototypeEditableContent.module.scss';
// No types generated for ckeditor 5 for react
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
import classnames from 'classnames';
import React, { ChangeEvent } from 'react';

interface Props {
  editable?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  content: string;
  onContentChange?: (content: string) => void;
}

const WysiwygEditor = ({
  editable,
  content,
  onContentChange,
}: Props) => {

  const [editing, setEditing] = React.useState(false);
  const [saved, setSaved] = React.useState(false);
  const [temporaryContent, setTemporaryContent] = React.useState(content);

  const save = () => {
    setEditing(false);
    setSaved(true);

    if (onContentChange) {
      onContentChange(temporaryContent);
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
            data={content}
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
            dangerouslySetInnerHTML={{ __html: content }}
            className="govuk-!-padding-left-1 govuk-!-padding-right-1"
          />
        )}
      </div>
    </div>
  );
};

export default WysiwygEditor;
