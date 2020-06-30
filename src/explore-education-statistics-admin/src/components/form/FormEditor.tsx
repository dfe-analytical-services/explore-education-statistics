import styles from '@admin/components/form/FormEditor.module.scss';
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
// No types generated for ckeditor 5 for react
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
import ErrorMessage from '@common/components/ErrorMessage';
import FormTextArea from '@common/components/form/FormTextArea';
import isBrowser from '@common/utils/isBrowser';
import classNames from 'classnames';
import React, { ChangeEvent, useCallback, useMemo } from 'react';

export interface FormEditorProps {
  allowHeadings?: boolean;
  error?: string;
  hideLabel?: boolean;
  hint?: string;
  id: string;
  label: string;
  name: string;
  toolbarConfig?: string[];
  value: string;
  onChange: (content: string) => void;
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
  error,
  hideLabel,
  hint,
  id,
  label,
  name,
  toolbarConfig = toolbarConfigs.full,
  value,
  onChange,
}: FormEditorProps) => {
  const config = useMemo(
    () => ({
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
    }),
    [allowHeadings, toolbarConfig],
  );

  const handleChange = useCallback(
    (event: ChangeEvent, editor: { getData(): string }) => {
      onChange(editor.getData());
    },
    [onChange],
  );

  const handleInit = useCallback(
    (editor: { editing: { view: { focus(): void } } }) => {
      editor.editing.view.focus();
    },
    [],
  );

  if (isBrowser('IE')) {
    return <FormTextArea id={id} name={name} label={label} disabled />;
  }

  return (
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
          config={config}
          data={value}
          onChange={handleChange}
          onInit={handleInit}
        />
      </div>
    </>
  );
};

export default FormEditor;
