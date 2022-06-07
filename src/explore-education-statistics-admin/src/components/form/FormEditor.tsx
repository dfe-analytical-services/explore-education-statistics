import { useCommentsContext } from '@admin/contexts/CommentsContext';
import styles from '@admin/components/form/FormEditor.module.scss';
import {
  CommentsPlugin,
  DowncastWriter,
  Editor as EditorType,
  Element,
} from '@admin/types/ckeditor';
import { defaultAllowedHeadings } from '@admin/config/ckEditorConfig';
import useCKEditorConfig from '@admin/hooks/useCKEditorConfig';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import getMarkersOrder from '@admin/utils/ckeditor/getMarkersOrder';
import { CKEditor, CKEditorProps } from '@ckeditor/ckeditor5-react';
import ErrorMessage from '@common/components/ErrorMessage';
import FormLabel from '@common/components/form/FormLabel';
import ContentHtml from '@common/components/ContentHtml';
import useToggle from '@common/hooks/useToggle';
import isBrowser from '@common/utils/isBrowser';
import classNames from 'classnames';
import Editor from 'explore-education-statistics-ckeditor';
import React, {
  MutableRefObject,
  useCallback,
  useEffect,
  useMemo,
  useRef,
} from 'react';

export type EditorChangeHandler = (value: string) => void;
export type EditorElementsHandler = (elements: Element[]) => void;

export interface FormEditorProps {
  allowComments?: boolean;
  allowedHeadings?: string[];
  error?: string;
  focusOnInit?: boolean;
  hideLabel?: boolean;
  hint?: string;
  id: string;
  label: string;
  testId?: string;
  toolbarConfig?: string[];
  value: string;
  onAutoSave?: (values: string) => void;
  onBlur?: () => void;
  onCancelComment?: () => void;
  onChange: EditorChangeHandler;
  onClickAddComment?: () => void;
  onElementsChange?: EditorElementsHandler;
  onElementsReady?: EditorElementsHandler;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
}

const FormEditor = ({
  allowComments = false,
  allowedHeadings = defaultAllowedHeadings,
  error,
  focusOnInit,
  hideLabel,
  hint,
  id,
  label,
  testId,
  toolbarConfig,
  value,
  onAutoSave,
  onBlur,
  onCancelComment,
  onChange,
  onClickAddComment,
  onElementsChange,
  onElementsReady,
  onImageUpload,
  onImageUploadCancel,
}: FormEditorProps) => {
  const editorInstance = useRef<EditorType>();
  const commentsPlugin = useRef<CommentsPlugin>();
  const editorRef: MutableRefObject<HTMLDivElement | null> = useRef(null);
  const {
    currentInteraction,
    selectedComment,
    setMarkersOrder,
  } = useCommentsContext();
  const config = useCKEditorConfig({
    allowComments,
    allowedHeadings,
    editorInstance,
    toolbarConfig,
    onAutoSave,
    onCancelComment,
    onClickAddComment,
    onImageUpload,
    onImageUploadCancel,
  });

  const [isFocused, toggleFocused] = useToggle(false);

  const describedBy = useMemo(
    () =>
      classNames({
        [`${id}-error`]: !!error,
        [`${id}-hint`]: !!hint,
      }),
    [error, hint, id],
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
      onElementsChange?.(
        Array.from(editor.model.document.getRoot('main').getChildren()),
      );
      onChange(editor.getData());
    },
    [onChange, onElementsChange],
  );

  const handleBlur = useCallback<CKEditorProps['onBlur']>(() => {
    toggleFocused.off();

    // Add a tiny timeout to prevent form submit events being lost during
    // the blur (but only when there is a large DOM change e.g. element renders).
    // By 'deferring' the blur event slightly, the higher priority submit
    // event can take place without interruption.
    //
    // I'm fairly sure this is due to CKEditor's event system not being
    // handled by React. This means React is not aware the CKEditor
    // blur should not take precedence over the submit event.
    // This may be fixable in React 18 with the new `useTransition` hook.
    setTimeout(() => {
      onBlur?.();
    }, 100);
  }, [onBlur, toggleFocused]);

  // Change editor to add attributes like `aria-describedby`
  // linking the editor textbox to error messages.
  const changeEditingView = useCallback(
    (writer: DowncastWriter) => {
      const root = writer.document.getRoot();

      writer.setAttribute('id', id, root);

      if (describedBy) {
        writer.setAttribute('aria-describedby', describedBy, root);
      } else {
        writer.removeAttribute('aria-describedby', root);
      }
    },
    [describedBy, id],
  );

  const handleReady = useCallback<CKEditorProps['onReady']>(
    editor => {
      if (focusOnInit) {
        editor.editing.view.focus();
      }

      editor.editing.view.change(changeEditingView);

      onElementsReady?.(
        Array.from(editor.model.document.getRoot('main').getChildren()),
      );

      editorInstance.current = editor;
      commentsPlugin.current = editor.plugins.get<CommentsPlugin>('Comments');

      setMarkersOrder(getMarkersOrder([...editor.model.markers]));
    },
    [changeEditingView, focusOnInit, onElementsReady, setMarkersOrder],
  );

  useEffect(() => {
    editorInstance.current?.editing.view.change(changeEditingView);
  }, [changeEditingView]);

  useEffect(() => {
    if (!selectedComment?.fromEditor && selectedComment?.id) {
      commentsPlugin.current?.selectCommentMarker(selectedComment.id);
    }
  }, [selectedComment]);

  useEffect(() => {
    function updateMarker() {
      if (!commentsPlugin.current || !currentInteraction) {
        return;
      }

      switch (currentInteraction.type) {
        case 'adding':
          commentsPlugin.current.addCommentMarker(currentInteraction.id);
          if (editorInstance.current) {
            setMarkersOrder(
              getMarkersOrder([...editorInstance.current.model.markers]),
            );
          }
          break;
        case 'removing':
          commentsPlugin.current.removeCommentMarker(currentInteraction.id);
          break;
        case 'resolving':
          commentsPlugin.current.resolveCommentMarker(
            currentInteraction.id,
            false,
          );
          break;
        case 'unresolving':
          commentsPlugin.current.resolveCommentMarker(
            currentInteraction.id,
            true,
          );
          break;
        default:
          break;
      }
    }

    updateMarker();
  }, [currentInteraction, setMarkersOrder]);

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
          data-testid={testId && isFocused ? `${testId}-focused` : testId}
          ref={thisRef => {
            const editorElement = thisRef?.querySelector<HTMLDivElement>(
              '[role="textbox"]',
            );

            if (editorElement) {
              editorRef.current = editorElement;
            }
          }}
        >
          {process.env.NODE_ENV !== 'test' ? (
            <CKEditor
              editor={Editor}
              config={config}
              data={value}
              onChange={handleChange}
              onFocus={toggleFocused.on}
              onBlur={handleBlur}
              onReady={handleReady}
            />
          ) : (
            <textarea
              aria-describedby={describedBy}
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
          <ContentHtml html={value} />
        </div>
      )}
    </>
  );
};

export default FormEditor;
