import {
  headingOptions,
  imageToolbar,
  resizeOptions,
  tableContentToolbar,
  toolbarConfigs,
} from '@admin/config/ckEditorConfig';
import { Editor, EditorConfig } from '@admin/types/ckeditor';
import { useCommentsContext } from '@admin/contexts/comments/CommentsContext';
import useEditingActions from '@admin/contexts/editing/useEditingActions';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import customUploadAdapterPlugin from '@admin/utils/ckeditor/customUploadAdapterPlugin';
import { MutableRefObject } from 'react';

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
  blockId: string;
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
    onDeleteComment,
    onResolveComment,
    onUndeleteComment,
    onUnresolveComment,
    setCurrentInteraction,
    setSelectedComment,
  } = useCommentsContext();
  const editingActions = useEditingActions();

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

  const config: EditorConfig = {
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
            const commentId = markerId ? markerId.replace('comment:', '') : '';
            setSelectedComment({ commentId, fromEditor: true });
          },
          // Comment marker removed in the editor
          commentRemoved(markerId) {
            if (editorInstance?.current) {
              const commentId = markerId.replace('comment:', '');
              onDeleteComment(commentId);
              editingActions.updateUnsavedCommentDeletions(blockId, commentId);
              onAutoSave?.(editorInstance?.current.getData());
            }
          },
          // Adding a comment is cancelled from within the editor (by clicking outside the placeholder marker)
          commentCancelled() {
            onCancelComment?.();
          },
          // Comment actions undone/redone in the editor
          undoRedoComment(type, markerId) {
            const commentId = markerId.startsWith('comment:')
              ? markerId.replace('comment:', '')
              : markerId.replace('resolvedcomment:', '');

            if (type === 'undoRemoveComment' || type === 'redoAddComment') {
              onUndeleteComment(commentId);
              editingActions.updateUnsavedCommentDeletions(blockId, commentId);
              return;
            }
            if (type === 'undoAddComment' || type === 'redoRemoveComment') {
              onDeleteComment(commentId);
              editingActions.updateUnsavedCommentDeletions(blockId, commentId);
              return;
            }

            if (
              type === 'undoResolveComment' ||
              type === 'redoUnresolveComment'
            ) {
              onUnresolveComment(commentId);
              editingActions.updateUnresolvedComments(blockId, commentId);
              return;
            }
            if (
              type === 'undoUnresolveComment' ||
              type === 'redoResolveComment'
            ) {
              onResolveComment(commentId);
              editingActions.updateUnresolvedComments(blockId, commentId);
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

  return config;
};

export default useCKEditorConfig;
