import {
  headingOptions,
  imageToolbar,
  resizeOptions,
  tableContentToolbar,
  toolbarConfigs,
} from '@admin/config/ckEditorConfig';
import { Editor, EditorConfig } from '@admin/types/ckeditor';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
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
    removeComment,
    resolveComment,
    reAddComment,
    unresolveComment,
    setCurrentInteraction,
    setSelectedComment,
  } = useCommentsContext();
  const {
    updateUnresolvedComments,
    updateUnsavedCommentDeletions,
  } = useEditingContext();

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
              removeComment?.current(commentId);
              updateUnsavedCommentDeletions.current(blockId, commentId);
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
              reAddComment.current(commentId);
              updateUnsavedCommentDeletions.current(blockId, commentId);
              return;
            }
            if (type === 'undoAddComment' || type === 'redoRemoveComment') {
              removeComment.current(commentId);
              updateUnsavedCommentDeletions.current(blockId, commentId);
              return;
            }

            if (
              type === 'undoResolveComment' ||
              type === 'redoUnresolveComment'
            ) {
              unresolveComment.current(commentId);
              updateUnresolvedComments.current(blockId, commentId);
              return;
            }
            if (
              type === 'undoUnresolveComment' ||
              type === 'redoResolveComment'
            ) {
              resolveComment.current(commentId);
              updateUnresolvedComments.current(blockId, commentId);
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
