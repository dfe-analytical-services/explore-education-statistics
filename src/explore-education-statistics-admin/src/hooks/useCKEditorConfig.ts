import {
  alignmentOptions,
  corePlugins,
  headingOptions,
  imageToolbar,
  resizeOptions,
  tableContentToolbar,
  toolbarConfigFull,
} from '@admin/config/ckEditorConfig';
import {
  Editor as EditorType,
  EditorConfig,
  PluginName,
  ToolbarOption,
  ToolbarGroup,
} from '@admin/types/ckeditor';
import Editor from 'explore-education-statistics-ckeditor';
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
  editorInstance,
  includePlugins,
  toolbarConfig = toolbarConfigFull,
  onAutoSave,
  onCancelComment,
  onClickAddComment,
  onClickAddFeaturedTableLink,
  onClickAddGlossaryItem,
  onImageUpload,
  onImageUploadCancel,
  label,
}: {
  allowComments?: boolean;
  allowedHeadings?: string[];
  editorInstance?: MutableRefObject<EditorType | undefined>;
  glossaryItems?: {
    title: string;
    slug: string;
    body: string;
  }[];
  includePlugins?: ReadonlySet<PluginName> | Set<PluginName>;
  toolbarConfig?:
    | ReadonlyArray<ToolbarOption | ToolbarGroup>
    | Array<ToolbarOption | ToolbarGroup>;
  onAutoSave?: (content: string) => void;
  onCancelComment?: () => void;
  onClickAddComment?: () => void;
  onClickAddFeaturedTableLink?: () => void;
  onClickAddGlossaryItem?: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onRemoveCommentMarker?: (commentId: string) => void;
  label: string;
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
      toolbar: ['accessibilityHelp', '|', ...toolbar],
      heading: toolbar.includes('heading')
        ? {
            options: [
              {
                model: 'paragraph',
                title: 'Paragraph',
                class: 'ck-heading_paragraph',
              },
              ...headingOptions.filter(
                option => allowedHeadings?.includes(option.view ?? ''),
              ),
            ],
          }
        : undefined,
      image: hasImageUpload
        ? {
            toolbar: imageToolbar,
            resizeOptions,
            upload: {
              types: ['jpeg', 'png', 'gif', 'bmp', 'tiff', 'svg+xml'],
            },
          }
        : undefined,
      removePlugins: includePlugins
        ? Editor.builtinPlugins?.filter(
            plugin =>
              !corePlugins.has(plugin.pluginName) &&
              !includePlugins.has(plugin.pluginName),
          )
        : undefined,
      table: {
        contentToolbar: tableContentToolbar,
      },
      link: {
        decorators: {
          addDataGlossaryAttributeToGlossaryLinks: {
            mode: 'automatic',
            callback: (url: string) =>
              url?.startsWith(process.env.PUBLIC_URL) &&
              url?.match(/\/glossary#[a-zA-Z-0-9-]+$/),
            attributes: { 'data-glossary': '' },
          },
          addDataFeaturedTableAttributeToFeaturedTableLinks: {
            mode: 'automatic',
            callback: (url: string) =>
              url?.startsWith(process.env.PUBLIC_URL) &&
              url?.match(/\/data-tables\/fast-track\/[a-zA-Z-0-9-]/),
            attributes: { 'data-featured-table': '' },
          },
        },
        defaultProtocol: 'https://',
      },
      label,
      licenseKey: 'GPL',
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
              if (editorInstance?.current) {
                const commentId = markerId.replace('comment:', '');
                removeComment?.current(commentId);
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

              switch (type) {
                case 'undoRemoveComment':
                case 'redoAddComment':
                  reAddComment.current(commentId);
                  break;
                case 'undoAddComment':
                case 'redoRemoveComment':
                  removeComment.current(commentId);
                  break;
                case 'undoResolveComment':
                case 'redoUnresolveComment':
                  unresolveComment.current(commentId);
                  break;
                case 'undoUnresolveComment':
                case 'redoResolveComment':
                  resolveComment.current(commentId);
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
      alignment: alignmentOptions,
      featuredTables: {
        addFeaturedTableLink() {
          onClickAddFeaturedTableLink?.();
        },
      },
      glossary: {
        addGlossaryItem() {
          onClickAddGlossaryItem?.();
        },
      },
    };
  }, [
    allowComments,
    allowedHeadings,
    editorInstance,
    hasImageUpload,
    includePlugins,
    onAutoSave,
    onCancelComment,
    onClickAddComment,
    onClickAddFeaturedTableLink,
    onClickAddGlossaryItem,
    onImageUpload,
    onImageUploadCancel,
    reAddComment,
    removeComment,
    resolveComment,
    setCurrentInteraction,
    setSelectedComment,
    toolbar,
    unresolveComment,
    label,
  ]);

  return config;
};

export default useCKEditorConfig;
