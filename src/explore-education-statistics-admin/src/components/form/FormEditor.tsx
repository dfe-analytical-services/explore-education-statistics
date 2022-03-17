import { useCommentsContext } from '@admin/contexts/CommentsContext';
import styles from '@admin/components/form/FormEditor.module.scss';
import {
  CommentsPlugin,
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
import React, { MutableRefObject, useCallback, useEffect, useRef } from 'react';

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
    onBlur?.();
  }, [onBlur, toggleFocused]);

  const handleReady = useCallback<CKEditorProps['onReady']>(
    editor => {
      if (focusOnInit) {
        editor.editing.view.focus();
      }
      onElementsReady?.(
        Array.from(editor.model.document.getRoot('main').getChildren()),
      );
      editorInstance.current = editor;
      commentsPlugin.current = editor.plugins.get<CommentsPlugin>('Comments');
      setMarkersOrder(getMarkersOrder([...editor.model.markers]));
    },
    [focusOnInit, onElementsReady, setMarkersOrder],
  );

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
    if (editorInstance.current) {
      onAutoSave?.(editorInstance.current.getData());
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentInteraction]);

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
          ref={thisRef => {
            const editorElement = thisRef?.querySelector<HTMLDivElement>(
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
