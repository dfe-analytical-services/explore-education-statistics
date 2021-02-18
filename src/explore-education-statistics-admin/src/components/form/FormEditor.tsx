import styles from '@admin/components/form/FormEditor.module.scss';
import { EditorConfig, HeadingOption } from '@admin/types/ckeditor';
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
import { CKEditor, CKEditorProps } from '@ckeditor/ckeditor5-react';
import ErrorMessage from '@common/components/ErrorMessage';
import FormLabel from '@common/components/form/FormLabel';
import SanitizeHtml from '@common/components/SanitizeHtml';
import useToggle from '@common/hooks/useToggle';
import isBrowser from '@common/utils/isBrowser';
import classNames from 'classnames';
import React, {
  MutableRefObject,
  useCallback,
  useEffect,
  useMemo,
  useRef,
} from 'react';

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
  simple: [
    'bold',
    'italic',
    'link',
    '|',
    'bulletedList',
    'numberedList',
    '|',
    'redo',
    'undo',
  ],
};

const defaultAllowedHeadings = ['h3', 'h4', 'h5'];

const headingOptions: HeadingOption[] = [
  {
    model: 'heading1',
    view: 'h1',
    title: 'Heading 1',
    class: 'ck-heading_heading1',
  },
  {
    model: 'heading2',
    view: 'h2',
    title: 'Heading 2',
    class: 'ck-heading_heading2',
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
];

export interface FormEditorProps {
  allowedHeadings?: string[];
  error?: string;
  focusOnInit?: boolean;
  hideLabel?: boolean;
  hint?: string;
  id: string;
  label: string;
  toolbarConfig?: string[];
  value: string;
  testId?: string;
  onBlur?: () => void;
  onChange: (content: string) => void;
}

const FormEditor = ({
  allowedHeadings = defaultAllowedHeadings,
  error,
  focusOnInit,
  hideLabel,
  hint,
  id,
  label,
  toolbarConfig = toolbarConfigs.full,
  value,
  testId,
  onBlur,
  onChange,
}: FormEditorProps) => {
  const editorRef: MutableRefObject<HTMLDivElement | null> = useRef(null);

  const [isFocused, toggleFocused] = useToggle(false);

  const config = useMemo<EditorConfig>(
    () => ({
      toolbar: toolbarConfig,
      heading: toolbarConfig?.includes('heading')
        ? {
            options: [
              {
                model: 'paragraph',
                title: 'Paragraph',
                class: 'ck-heading_paragraph',
              },
              ...headingOptions.filter(option =>
                allowedHeadings?.includes(option.view ?? ''),
              ),
            ],
          }
        : undefined,
    }),
    [allowedHeadings, toolbarConfig],
  );

  useEffect(() => {
    if (process.env.NODE_ENV === 'test') {
      return undefined;
    }

    // Workaround to try and focus/scroll to CKEditor
    // whenever a form validation error link is clicked.
    const handleHashChange = () => {
      if (!editorRef.current || !window.location.hash) {
        return;
      }

      const hashId = window.location.hash.substring(1);

      if (hashId !== id) {
        return;
      }

      editorRef.current.focus();
      editorRef.current.scrollIntoView({
        block: 'center',
        behavior: 'smooth',
      });
    };

    window.addEventListener('hashchange', handleHashChange);

    return () => {
      window.removeEventListener('hashchange', handleHashChange);
    };
  }, [id]);

  const handleLabelClick = useCallback(() => {
    if (!editorRef.current) {
      return;
    }

    editorRef.current.focus();
  }, []);

  const handleChange = useCallback<CKEditorProps['onChange']>(
    (event, editor) => {
      onChange(editor.getData());
    },
    [onChange],
  );

  const handleBlur = useCallback<CKEditorProps['onBlur']>(() => {
    toggleFocused.off();

    if (onBlur) {
      onBlur();
    }
  }, [onBlur, toggleFocused]);

  const handleReady = useCallback<CKEditorProps['onReady']>(
    editor => {
      if (focusOnInit) {
        editor.editing.view.focus();
      }
    },
    [focusOnInit],
  );

  const isReadOnly = isBrowser('IE');

  return (
    <>
      {process.env.NODE_ENV !== 'test' ? (
        // Workaround to emulate standard label behaviour
        // and focus the editor correctly.
        // eslint-disable-next-line jsx-a11y/click-events-have-key-events,jsx-a11y/no-static-element-interactions
        <span
          id={`${id}-label`}
          className={classNames('govuk-label', {
            'govuk-visually-hidden': hideLabel,
          })}
          onClick={handleLabelClick}
        >
          {label}
        </span>
      ) : (
        <FormLabel id={id} label={label} />
      )}

      {hint && (
        <span id={`${id}-hint`} className="govuk-hint">
          {hint}
        </span>
      )}

      {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}

      {!isReadOnly ? (
        <div
          className={classNames(styles.editor, {
            [styles.focused]: isFocused,
          })}
          data-testid={isFocused ? `${testId}-focused` : testId}
          ref={ref => {
            const editorElement = ref?.querySelector<HTMLDivElement>(
              '[role="textbox"]',
            );

            if (editorElement) {
              editorElement.id = id;
              editorRef.current = editorElement;
            }
          }}
        >
          {process.env.NODE_ENV !== 'test' ? (
            <CKEditor
              editor={ClassicEditor}
              config={config}
              data={value}
              onChange={handleChange}
              onFocus={toggleFocused.on}
              onBlur={handleBlur}
              onReady={handleReady}
            />
          ) : (
            <textarea
              id={id}
              value={value}
              onBlur={() => {
                if (onBlur) {
                  onBlur();
                }
              }}
              onChange={event => onChange(event.target.value)}
            />
          )}
        </div>
      ) : (
        <div
          aria-readonly
          aria-labelledby={`${id}-label`}
          className={styles.readOnlyEditor}
          role="textbox"
          id={id}
          tabIndex={0}
        >
          <SanitizeHtml dirtyHtml={value} />
        </div>
      )}
    </>
  );
};

export default FormEditor;
