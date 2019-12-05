/* eslint-disable react/no-array-index-key */
import { EditableRelease } from '@admin/services/publicationService';
import ContentBlock, {
  ContentBlockProps,
} from '@common/modules/find-statistics/components/ContentBlock';
import React from 'react';
import wrapEditableComponent, {
  EditingContext,
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import AddComment from '@admin/pages/prototypes/components/PrototypeEditableContentAddComment';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import ResolveComment from '@admin/pages/prototypes/components/PrototypeEditableContentResolveComment';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import AddContentButton from '@admin/modules/find-statistics/components/AddContentButton';
import ContentBlockDroppable from '@admin/modules/find-statistics/components/ContentBlockDroppable';
import { Dictionary } from '@common/types/util';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';
import ContentBlockDraggable from './ContentBlockDraggable';

type ContentType = EditableRelease['content'][0]['content'];

export type EditableContentType = ContentType;

export type ReorderHook = (sectionId?: string) => Promise<void>;

export interface Props extends ContentBlockProps {
  content: ContentType;

  sectionId: string;

  editable?: boolean;
  canAddBlocks: boolean;
  canAddSingleBlock?: boolean;
  isReordering?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  onContentChange?: (content: ContentType) => void;
  onReorderHook?: (callback: ReorderHook) => void;
}

interface EditingContentBlockContext extends ReleaseContentContext {
  sectionId?: string;
}

export const EditingContentBlockContext = React.createContext<
  EditingContentBlockContext
>({
  releaseId: undefined,
  isEditing: false,
  sectionId: undefined,
});

const EditableContentBlock = ({
  content = [],
  id = '',
  sectionId,
  editable = true,
  onContentChange,
  reviewing,
  resolveComments,
  canAddBlocks = false,
  canAddSingleBlock = false,
  isReordering = false,
  onReorderHook = undefined,
}: Props) => {
  const editingContext = React.useContext(EditingContext);

  const [contentBlocks, setContentBlocks] = React.useState<ContentType>(
    content,
  );

  React.useEffect(() => {
    if (onReorderHook) {
      const saveOrder: ReorderHook = async contentSectionId => {
        if (editingContext.releaseId && contentSectionId) {
          if (contentBlocks) {
            const newOrder = contentBlocks.reduce<Dictionary<number>>(
              (order, next, index) => ({ ...order, [next.id]: index }),
              {},
            );

            await releaseContentService.updateContentSectionBlocksOrder(
              editingContext.releaseId,
              contentSectionId,
              newOrder,
            );

            if (onContentChange) {
              onContentChange(contentBlocks);
            }
          }
        }
      };
      onReorderHook(saveOrder);
    }
  }, [contentBlocks, editingContext.releaseId, onContentChange, onReorderHook]);

  const onAddContentCallback = (type: string, order: number | undefined) => {
    if (editingContext.releaseId && sectionId) {
      const { releaseId } = editingContext;

      releaseContentService
        .addContentSectionBlock(releaseId, sectionId, {
          body: 'Click to edit',
          type,
          order,
        })
        .then(() =>
          releaseContentService.getContentSection(releaseId, sectionId),
        )
        .then(section => {
          if (onContentChange) onContentChange(section.content);
          setContentBlocks(section.content);
        });
    }
  };

  if (contentBlocks === undefined || contentBlocks.length === 0) {
    return (
      <>
        <div className="govuk-inset-text">
          There is no content for this section.
        </div>
        {(canAddBlocks || canAddSingleBlock) && (
          <AddContentButton order={0} onClick={onAddContentCallback} />
        )}
      </>
    );
  }

  const onDeleteContent = async (contentId: string) => {
    const { releaseId } = editingContext;
    if (releaseId && sectionId && contentId) {
      await releaseContentService.deleteContentSectionBlock(
        releaseId,
        sectionId,
        contentId,
      );

      const {
        content: newContentBlocks,
      } = await releaseContentService.getContentSection(releaseId, sectionId);

      setContentBlocks(newContentBlocks);

      if (onContentChange) onContentChange(newContentBlocks);
    }
  };

  const onDragEnd = (result: DropResult) => {
    const { source, destination, type } = result;

    if (type === 'content' && destination) {
      const newContentBlocks = [...contentBlocks];
      const [removed] = newContentBlocks.splice(source.index, 1);
      newContentBlocks.splice(destination.index, 0, removed);
      setContentBlocks(newContentBlocks);
      if (onContentChange) onContentChange(newContentBlocks);
    }
  };

  return (
    <EditingContentBlockContext.Provider
      value={{
        ...editingContext,
        sectionId,
      }}
    >
      <DragDropContext onDragEnd={onDragEnd}>
        <ContentBlockDroppable
          draggable={isReordering}
          droppableId={`${sectionId}`}
        >
          {contentBlocks.map((block, index) => (
            <ContentBlockDraggable
              draggable={isReordering}
              draggableId={`${block.id}`}
              key={`${block.id}`}
              index={index}
            >
              {!isReordering && (
                <>
                  {reviewing && <AddComment initialComments={block.comments} />}
                  {resolveComments && (
                    <ResolveComment initialComments={block.comments} />
                  )}
                  {canAddBlocks && (
                    <AddContentButton
                      order={index}
                      onClick={onAddContentCallback}
                    />
                  )}
                </>
              )}

              <EditableContentSubBlockRenderer
                editable={editable && !isReordering}
                canDelete={!!canAddBlocks && !isReordering}
                block={block}
                id={id}
                index={index}
                onContentChange={newContent => {
                  const newBlocks = [...contentBlocks];
                  newBlocks[index].body = newContent;
                  setContentBlocks(newBlocks);
                  if (onContentChange) {
                    onContentChange(newBlocks);
                  }
                }}
                onDelete={() => onDeleteContent(block.id)}
              />

              {!isReordering &&
                canAddBlocks &&
                index === contentBlocks.length - 1 && (
                  <AddContentButton onClick={onAddContentCallback} />
                )}
            </ContentBlockDraggable>
          ))}
        </ContentBlockDroppable>
      </DragDropContext>
    </EditingContentBlockContext.Provider>
  );
};

export default wrapEditableComponent(EditableContentBlock, ContentBlock);
