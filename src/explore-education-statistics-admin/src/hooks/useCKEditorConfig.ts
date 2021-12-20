import {
  headingOptions,
  imageToolbar,
  resizeOptions,
  tableContentToolbar,
  toolbarConfigs,
} from '@admin/config/ckEditorConfig';
import { Editor, EditorConfig } from '@admin/types/ckeditor';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import customUploadAdapterPlugin from '@admin/utils/ckeditor/customUploadAdapterPlugin';
import { MutableRefObject, useMemo } from 'react';

const useCKEditorConfig = ({
  allowComments,
  allowedHeadings,
  blockId,
  editorInstance,
  toolbarConfig = toolbarConfigs.full,
  onAutoSave,
  onCancelComment,
  onClickAddComment,
  onImageUpload,
  onImageUploadCancel,
}: {
  allowComments?: boolean;
  allowedHeadings?: string[];
  blockId?: string;
  editorInstance?: MutableRefObject<Editor | undefined>;
  toolbarConfig?: string[];
  onAutoSave?: (content: string) => void;
  onCancelComment?: () => void;
  onClickAddComment?: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onRemoveCommentMarker?: (commentId: string) => void;
}) => {
  const {
    removeComment,
    resolveComment,
    reAddComment,
    unresolveComment,
    setCurrentInteraction,
    setSelectedComment,
  } = useCommentsContext();

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

  const config: EditorConfig = useMemo(() => {
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
            // Add comment button in editor clicked
            addComment() {
              onClickAddComment?.();
            },
            // Comment marker selected in the editor
            commentSelected(markerId) {
              const id = markerId ? markerId.replace('comment:', '') : '';
              setSelectedComment({ id, fromEditor: true });
            },
            // Comment marker removed in the editor
            commentRemoved(markerId) {
              if (editorInstance?.current && blockId) {
                const commentId = markerId.replace('comment:', '');
                removeComment?.current(blockId, commentId);
                onAutoSave?.(editorInstance?.current.getData());
              }
            },
            // Adding a comment is cancelled from within the editor (by clicking outside the placeholder marker)
            commentCancelled() {
              onCancelComment?.();
            },
            // Comment actions undone/redone in the editor
            undoRedoComment(type, markerId) {
              if (!blockId) {
                return;
              }
              const commentId = markerId.startsWith('comment:')
                ? markerId.replace('comment:', '')
                : markerId.replace('resolvedcomment:', '');

              switch (type) {
                case 'undoRemoveComment':
                case 'redoAddComment':
                  reAddComment.current(blockId, commentId);
                  break;
                case 'undoAddComment':
                case 'redoRemoveComment':
                  removeComment.current(blockId, commentId);
                  break;
                case 'undoResolveComment':
                case 'redoUnresolveComment':
                  unresolveComment.current(blockId, commentId);
                  break;
                case 'undoUnresolveComment':
                case 'redoResolveComment':
                  resolveComment.current(blockId, commentId);
                  break;
                default:
                  break;
              }
            },
          }
        : undefined,
      autosave: onAutoSave
        ? {
            save() {
              if (editorInstance?.current) {
                setCurrentInteraction(undefined);
                onAutoSave(editorInstance.current.getData());
              }
            },
            waitingTime: 5000,
          }
        : undefined,
    };
  }, [
    allowComments,
    allowedHeadings,
    blockId,
    editorInstance,
    hasImageUpload,
    onAutoSave,
    onCancelComment,
    onClickAddComment,
    onImageUpload,
    onImageUploadCancel,
    reAddComment,
    removeComment,
    resolveComment,
    setCurrentInteraction,
    setSelectedComment,
    toolbar,
    unresolveComment,
  ]);

  return config;
};

export default useCKEditorConfig;
