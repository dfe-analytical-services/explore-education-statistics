import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import AddContentButton from '@admin/modules/find-statistics/components/AddContentButton';
import ContentBlockDroppable from '@admin/modules/find-statistics/components/ContentBlockDroppable';
import { EditableRelease } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import ContentBlock, {
  ContentBlockProps,
} from '@common/modules/find-statistics/components/ContentBlock';
import wrapEditableComponent, {
  EditingContext,
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types/util';
import React, { useContext, useState } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import { DataBlock } from '@common/services/dataBlockService';
import Comments from './Comments';
import ContentBlockDraggable from './ContentBlockDraggable';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';

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
  isCommenting: false,
  isReviewing: false,
  isEditing: false,
  sectionId: undefined,
  availableDataBlocks: [],
});

const EditableContentBlock = ({
  content = [],
  id = '',
  sectionId,
  editable = true,
  onContentChange,
  canAddBlocks = false,
  canAddSingleBlock = false,
  isReordering = false,
  onReorderHook = undefined,
}: Props) => {
  const editingContext = useContext(EditingContext);

  const [contentBlocks, setContentBlocks] = useState<ContentType>(content);

  const { handleApiErrors } = useContext(ErrorControlContext);

  React.useEffect(() => {
    if (onReorderHook) {
      const saveOrder: ReorderHook = async contentSectionId => {
        if (editingContext.releaseId && contentSectionId) {
          if (contentBlocks) {
            const newOrder = contentBlocks.reduce<Dictionary<number>>(
              (order, next, index) => ({ ...order, [next.id]: index }),
              {},
            );

            await releaseContentService
              .updateContentSectionBlocksOrder(
                editingContext.releaseId,
                contentSectionId,
                newOrder,
              )
              .catch(handleApiErrors);

            if (onContentChange) {
              onContentChange(contentBlocks);
            }
          }
        }
      };
      onReorderHook(saveOrder);
    }
  }, [
    contentBlocks,
    editingContext.releaseId,
    onContentChange,
    onReorderHook,
    handleApiErrors,
  ]);

  const onAddContentCallback = (
    type: string,
    data: string,
    order: number | undefined,
  ) => {
    if (editingContext.releaseId && sectionId) {
      const { releaseId } = editingContext;

      let addPromise;

      if (type === 'DataBlock') {
        addPromise = releaseContentService.attachContentSectionBlock(
          releaseId,
          sectionId,
          {
            contentBlockId: data,
            order: order || 0
          }
        )
      } else {
        addPromise = releaseContentService.addContentSectionBlock(
          releaseId,
          sectionId,
          {
            body: data,
            type,
            order,
          },
        );
      }

      addPromise
        .then(() =>
          releaseContentService.getContentSection(releaseId, sectionId),
        )
        .then(section => {
          if (onContentChange) onContentChange(section.content);
          setContentBlocks(section.content);
        })
        .catch(handleApiErrors);
    }
  };

  const onContentBlockChange = React.useCallback(
    (index: number, newContent: string) => {
      const newBlocks = [...(contentBlocks || [])];
      newBlocks[index].body = newContent;
      setContentBlocks(newBlocks);
      if (onContentChange) {
        onContentChange(newBlocks);
      }
    },
    [contentBlocks, onContentChange],
  );

  const onDeleteContent = async (contentId: string) => {
    const { releaseId } = editingContext;
    if (releaseId && sectionId && contentId) {
      await releaseContentService
        .deleteContentSectionBlock(releaseId, sectionId, contentId)
        .catch(handleApiErrors);

      const {
        content: newContentBlocks,
      } = await releaseContentService.getContentSection(releaseId, sectionId);

      setContentBlocks(newContentBlocks);

      if (onContentChange) onContentChange(newContentBlocks);
    }
  };

  const onDragEnd = React.useCallback(
    (result: DropResult) => {
      const { source, destination, type } = result;

      if (type === 'content' && destination) {
        const newContentBlocks = [...(contentBlocks || [])];
        const [removed] = newContentBlocks.splice(source.index, 1);
        newContentBlocks.splice(destination.index, 0, removed);
        setContentBlocks(newContentBlocks);
        if (onContentChange) onContentChange(newContentBlocks);
      }
    },
    [contentBlocks, onContentChange],
  );

  if (contentBlocks === undefined || contentBlocks.length === 0) {
    return (
      <>
        <div className="govuk-inset-text">
          There is no content for this section.
        </div>
        {(canAddBlocks || canAddSingleBlock) && (
          <AddContentButton
            onClick={(type, data) => onAddContentCallback(type, data, 0)}
            availableDataBlocks={editingContext.availableDataBlocks}
          />
        )}
      </>
    );
  }

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
                  {canAddBlocks && (
                    <AddContentButton
                      onClick={(type, data) =>
                        onAddContentCallback(type, data, index)
                      }
                      availableDataBlocks={editingContext.availableDataBlocks}
                    />
                  )}
                  {(editingContext.isCommenting ||
                    editingContext.isReviewing) && (
                    <Comments
                      contentBlockId={block.id}
                      initialComments={block.comments}
                      canResolve={editingContext.isReviewing}
                      canComment={editingContext.isCommenting}
                      onCommentsChange={async comments => {
                        const newBlocks = [...contentBlocks];
                        newBlocks[index].comments = comments;
                        setContentBlocks(newBlocks);
                        if (onContentChange) {
                          onContentChange(newBlocks);
                        }
                      }}
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
                onContentChange={(newContent: string) =>
                  onContentBlockChange(index, newContent)
                }
                onDelete={() => onDeleteContent(block.id)}
              />

              {!isReordering &&
                canAddBlocks &&
                index === contentBlocks.length - 1 && (
                  <AddContentButton
                    onClick={(type, data) =>
                      onAddContentCallback(type, data, contentBlocks.length)
                    }
                    availableDataBlocks={editingContext.availableDataBlocks}
                  />
                )}
            </ContentBlockDraggable>
          ))}
        </ContentBlockDroppable>
      </DragDropContext>
    </EditingContentBlockContext.Provider>
  );
};

export default wrapEditableComponent(EditableContentBlock, ContentBlock);
