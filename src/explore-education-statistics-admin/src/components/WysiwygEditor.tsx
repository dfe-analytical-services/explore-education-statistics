import styles from '@admin/pages/prototypes/components/PrototypeEditableContent.module.scss';
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';

import classnames from 'classnames';
import React, {ChangeEvent} from 'react';
import marked from 'marked';
// No types generated for ckeditor 5 for react
// @ts-ignore
import GFMDataProcessor from '@ckeditor/ckeditor5-markdown-gfm/src/gfmdataprocessor';
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

interface Props {
  editable?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  content: string;
  useMarkdown?: boolean;
  onContentChange?: (content: string) => void;
}

const WysiwygEditor = ({
  editable,
  content,
  onContentChange,
  useMarkdown = false,
}: Props) => {
  const [editing, setEditing] = React.useState(false);
  const [saved, setSaved] = React.useState(false);
  const [temporaryContent, setTemporaryContent] = React.useState(content);

  const plugins = React.useMemo(() => {
    if (useMarkdown) {
      return [
        (editor: unknown) => {
          // @ts-ignore
          editor.data.processor = new GFMDataProcessor(); // eslint-disable-line no-param-reassign
        },
      ];
    }

    return [];
  }, [useMarkdown]);

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
            config={{
              plugins: [...plugins, ...ClassicEditor.builtinPlugins],
              toolbar: [
                'heading',
                '|',
                'bold',
                'italic',
                'link',
                'bulletedList',
                'numberedList',
                'imageUpload',
                'blockQuote',
                'insertTable',
                'mediaEmbed',
                'undo',
                'redo',
                'markdown',
              ],
            }}
            plugins={plugins}
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
              __html: useMarkdown ? marked(content) : content,
            }}
            className="govuk-!-padding-left-1 govuk-!-padding-right-1"
          />
        )}
      </div>
    </div>
  );
};

export default WysiwygEditor;
