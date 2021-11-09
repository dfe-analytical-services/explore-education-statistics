import styles from '@admin/components/form/FormEditor.module.scss';
import { SelectedComment } from '@admin/components/editable/EditableContentForm';
import {
  CommentsPlugin,
  CommentUndoRedoActions,
  Editor as EditorType,
  EditorConfig,
  Element,
  Marker,
} from '@admin/types/ckeditor';
import {
  defaultAllowedHeadings,
  headingOptions,
  imageToolbar,
  resizeOptions,
  tableContentToolbar,
  toolbarConfigs,
} from '@admin/utils/ckeditor/ckEditorConfig';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import customUploadAdapterPlugin from '@admin/utils/ckeditor/customUploadAdapterPlugin';
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
export interface BlockCommentsState {
  adding?: string;
  removing?: string;
  resolving?: string;
  unresolving?: string;
}

export interface FormEditorProps {
  allowComments?: boolean;
  allowedHeadings?: string[];
  blockCommentsState?: BlockCommentsState;
  error?: string;
  focusOnInit?: boolean;
  hideLabel?: boolean;
  hint?: string;
  id: string;
  label: string;
  selectedComment?: SelectedComment;
  testId?: string;
  toolbarConfig?: string[];
  value: string;
  onAddCommentClicked?: () => void;
  onAutoSave?: (values: string) => void;
  onBlur?: () => void;
  onCancelComment?: () => void;
  onChange: EditorChangeHandler;
  onCommentMarkerClicked?: (commentId: string) => void;
  onCommentMarkerRemoved?: (commentId: string) => void;
  onElementsChange?: EditorElementsHandler;
  onElementsReady?: EditorElementsHandler;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onUndoRedo?: (type: CommentUndoRedoActions, commentId: string) => void;
  onUpdateMarkersOrder?: (markerIds: string[]) => void;
}

const FormEditor = ({
  allowComments = false,
  allowedHeadings = defaultAllowedHeadings,
  blockCommentsState,
  error,
  focusOnInit,
  hideLabel,
  hint,
  id,
  label,
  selectedComment,
  testId,
  toolbarConfig = toolbarConfigs.full,
  value,
  onAutoSave,
  onAddCommentClicked,
  onBlur,
  onCancelComment,
  onChange,
  onCommentMarkerClicked,
  onCommentMarkerRemoved,
  onElementsChange,
  onElementsReady,
  onImageUpload,
  onImageUploadCancel,
  onUndoRedo,
  onUpdateMarkersOrder,
}: FormEditorProps) => {
  const editorInstance = useRef<EditorType>();
  const commentsPlugin = useRef<CommentsPlugin>();

  const editorRef: MutableRefObject<HTMLDivElement | null> = useRef(null);
  const [isFocused, toggleFocused] = useToggle(false);

  // Get the order of the markers based on their position in the editor.
  // Used to order the comments list.
  const getMarkersOrder = (markers: Marker[]) => {
    const orderedMarkerIds: string[] = [];
    markers.sort((a, b) => {
      if (a.getStart().isAfter(b.getStart())) {
        return 1;
      }
      if (a.getStart().isBefore(b.getStart())) {
        return -1;
      }
      return 0;
    });

    markers.forEach(marker => {
      const markerId = marker.name.startsWith('comment:')
        ? marker.name.replace('comment:', '')
        : marker.name.replace('resolvedcomment:', '');
      orderedMarkerIds.push(markerId);
    });

    return orderedMarkerIds;
  };

  const config = useMemo<EditorConfig>(() => {
    const toolbar = toolbarConfig?.filter(tool => {
      // Disable image upload if no callback provided
      if (tool === 'imageUpload' && !onImageUpload) {
        return false;
      }
      // Disable comments if not allowed.
      if (tool === 'comment' && !allowComments) {
        return false;
      }

      return true;
    });

    const hasImageUpload = toolbar.includes('imageUpload');

    return {
      toolbar,
      heading: toolbar.includes('heading')
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
      image: hasImageUpload
        ? {
            toolbar: imageToolbar,
            resizeOptions,
          }
        : undefined,
      table: {
        contentToolbar: tableContentToolbar,
      },
      link: {
        decorators: {
          addDataGlossaryAttributeToGlossaryLinks: {
            mode: 'automatic',
            callback: (url: string) =>
              url.startsWith(process.env.PUBLIC_URL) &&
              url.match(/\/glossary#[a-zA-Z-0-9-]+$/),
            attributes: { 'data-glossary': '' },
          },
        },
      },
      extraPlugins:
        hasImageUpload && onImageUpload
          ? [customUploadAdapterPlugin(onImageUpload, onImageUploadCancel)]
          : undefined,
      comments: allowComments
        ? {
            addComment() {
              // Add comment button in editor clicked
              if (onAddCommentClicked) {
                onAddCommentClicked();
              }
            },
            commentSelected(markerId?: string) {
              // Comment marker selected in the editor
              if (onCommentMarkerClicked) {
                const commentId = markerId
                  ? markerId.replace('comment:', '')
                  : '';
                onCommentMarkerClicked(commentId);
              }
            },
            commentRemoved(markerId: string) {
              // Comment marker removed in the editor
              if (
                onCommentMarkerRemoved &&
                onAutoSave &&
                editorInstance.current
              ) {
                onCommentMarkerRemoved(markerId.replace('comment:', ''));
                onAutoSave(editorInstance.current.getData());
              }
            },
            commentCancelled() {
              // if adding a comment is cancelled from within the editor (by clicking outside the placeholder marker)
              if (onCancelComment) {
                onCancelComment();
              }
            },
            undoRedoComment(type: CommentUndoRedoActions, markerId: string) {
              if (onUndoRedo) {
                const commentId = markerId.startsWith('comment:')
                  ? markerId.replace('comment:', '')
                  : markerId.replace('resolvedcomment:', '');
                onUndoRedo(type, commentId);
              }
            },
          }
        : undefined,
      autosave: onAutoSave
        ? {
            save() {
              if (editorInstance.current) {
                return onAutoSave(editorInstance.current.getData());
              }
              return false;
            },
            waitingTime: 5000,
          }
        : undefined,
    };
  }, [
    allowComments,
    allowedHeadings,
    editorInstance,
    onAddCommentClicked,
    onAutoSave,
    onCancelComment,
    onCommentMarkerClicked,
    onCommentMarkerRemoved,
    onImageUpload,
    onImageUploadCancel,
    onUndoRedo,
    toolbarConfig,
  ]);

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
      if (onElementsChange) {
        onElementsChange(
          Array.from(editor.model.document.getRoot('main').getChildren()),
        );
      }

      onChange(editor.getData());
    },
    [onChange, onElementsChange],
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

      if (onElementsReady) {
        onElementsReady(
          Array.from(editor.model.document.getRoot('main').getChildren()),
        );
      }
      editorInstance.current = editor;
      commentsPlugin.current = editor.plugins.get('Comments') as CommentsPlugin;

      if (onUpdateMarkersOrder) {
        onUpdateMarkersOrder(getMarkersOrder([...editor.model.markers]));
      }
    },
    [focusOnInit, onElementsReady, onUpdateMarkersOrder],
  );

  useEffect(() => {
    if (
      !selectedComment?.fromEditor &&
      selectedComment?.commentId &&
      commentsPlugin.current
    ) {
      commentsPlugin.current.selectCommentMarker(selectedComment.commentId);
    }
  }, [selectedComment]);

  useEffect(() => {
    function updateMarker() {
      if (!commentsPlugin.current || !blockCommentsState) {
        return;
      }
      if (blockCommentsState.adding) {
        commentsPlugin.current.addCommentMarker(blockCommentsState.adding);
        if (onUpdateMarkersOrder && editorInstance.current) {
          onUpdateMarkersOrder(
            getMarkersOrder([...editorInstance.current.model.markers]),
          );
        }
        return;
      }
      if (blockCommentsState.removing) {
        commentsPlugin.current.removeCommentMarker(blockCommentsState.removing);
        return;
      }
      if (blockCommentsState.resolving) {
        commentsPlugin.current.resolveCommentMarker(
          blockCommentsState.resolving,
          false,
        );
        return;
      }
      if (blockCommentsState.unresolving) {
        commentsPlugin.current.resolveCommentMarker(
          blockCommentsState.unresolving,
          true,
        );
      }
    }

    updateMarker();
    if (onAutoSave && editorInstance.current) {
      onAutoSave(editorInstance.current.getData());
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    blockCommentsState?.adding,
    blockCommentsState?.removing,
    blockCommentsState?.resolving,
    blockCommentsState?.unresolving,
  ]);

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
