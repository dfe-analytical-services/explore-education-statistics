import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import AddContentButton from '@admin/modules/find-statistics/components/AddContentButton';
import ContentBlockDroppable from '@admin/modules/find-statistics/components/ContentBlockDroppable';
import { EditableRelease } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import PreviewContentBlocks, {
  ContentBlockProps,
} from '@common/modules/find-statistics/components/ContentBlocks';
import wrapEditableComponent, {
  EditingContext,
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types/util';
import React, { useContext } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
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
  textOnly?: boolean;
  isReordering?: boolean;
  resolveComments?: boolean;
  addContentButtonText?: string;
  onContentChange?: (content: ContentType) => void;
  onReorder?: (callback: ReorderHook) => void;
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
  textOnly = false,
  isReordering = false,
  addContentButtonText = 'Add content',
  onReorder = undefined,
}: Props) => {
  const editingContext = useContext(EditingContext);

  const { handleApiErrors } = useContext(ErrorControlContext);

  React.useEffect(() => {
    if (onReorder) {
      onReorder(async contentSectionId => {
        if (editingContext.releaseId && contentSectionId) {
          if (content) {
            const newOrder = content.reduce<Dictionary<number>>(
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
              onContentChange(content);
            }
          }
        }
      });
    }
  }, [
    content,
    editingContext.releaseId,
    onContentChange,
    onReorder,
    handleApiErrors,
  ]);

  const onAddContentCallback = (
    type: string,
    data: string,
    order: number | undefined,
  ) => {
    if (editingContext.releaseId && sectionId) {
      const { releaseId } = editingContext;

      let addPromise: Promise<unknown>;

      if (type === 'DataBlock') {
        addPromise = releaseContentService
          .attachContentSectionBlock(releaseId, sectionId, {
            contentBlockId: data,
            order: order || 0,
          })
          .then(v => {
            if (editingContext.updateAvailableDataBlocks) {
              editingContext.updateAvailableDataBlocks();
            }
            return v;
          });
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
        })
        .catch(handleApiErrors);
    }
  };

  const onContentBlockChange = React.useCallback(
    (index: number, newContent: string) => {
      const newBlocks = [...(content || [])];
      newBlocks[index].body = newContent;

      if (onContentChange) {
        onContentChange(newBlocks);
      }
    },
    [content, onContentChange],
  );

  const onDeleteContent = async (contentId: string) => {
    const { releaseId } = editingContext;
    if (releaseId && sectionId && contentId) {
      await releaseContentService
        .deleteContentSectionBlock(releaseId, sectionId, contentId)
        .catch(handleApiErrors);

      if (editingContext.updateAvailableDataBlocks) {
        editingContext.updateAvailableDataBlocks();
      }

      const {
        content: newContentBlocks,
      } = await releaseContentService.getContentSection(releaseId, sectionId);

      if (onContentChange) onContentChange(newContentBlocks);
    }
  };

  const onDragEnd = React.useCallback(
    (result: DropResult) => {
      const { source, destination, type } = result;

      if (type === 'content' && destination) {
        const newContentBlocks = [...(content || [])];
        const [removed] = newContentBlocks.splice(source.index, 1);
        newContentBlocks.splice(destination.index, 0, removed);
        if (onContentChange) onContentChange(newContentBlocks);
      }
    },
    [content, onContentChange],
  );

  if (content === undefined || content.length === 0) {
    return (
      <>
        <div className="govuk-inset-text">
          There is no content for this section.
        </div>
        {(canAddBlocks || canAddSingleBlock) && (
          <AddContentButton
            textOnly={textOnly}
            buttonText={addContentButtonText}
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
          {content.map((block, index) => (
            <div
              key={`content-section-${block.id}`}
              id={`content-section-${block.id}`}
              className="govuk-!-margin-bottom-9"
            >
              <ContentBlockDraggable
                draggable={isReordering}
                draggableId={`${block.id}`}
                key={`${block.id}`}
                index={index}
              >
                {!isReordering && (
                  <>
                    {(editingContext.isCommenting ||
                      editingContext.isReviewing) && (
                      <Comments
                        contentBlockId={block.id}
                        initialComments={block.comments}
                        canResolve={editingContext.isReviewing}
                        canComment={editingContext.isCommenting}
                        onCommentsChange={async comments => {
                          const newBlocks = [...content];
                          newBlocks[index].comments = comments;

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
                  index === content.length - 1 && (
                    <AddContentButton
                      textOnly={textOnly}
                      onClick={(type, data) =>
                        onAddContentCallback(type, data, content.length)
                      }
                      availableDataBlocks={editingContext.availableDataBlocks}
                    />
                  )}
              </ContentBlockDraggable>
            </div>
          ))}
        </ContentBlockDroppable>
      </DragDropContext>
    </EditingContentBlockContext.Provider>
  );
};

const ContentBlocks = wrapEditableComponent(
  EditableContentBlock,
  PreviewContentBlocks,
);

export default ContentBlocks;
