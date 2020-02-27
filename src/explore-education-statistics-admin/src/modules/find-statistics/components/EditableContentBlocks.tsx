import ContentBlockDroppable from '@admin/modules/find-statistics/components/ContentBlockDroppable';
import { EditableRelease } from '@admin/services/publicationService';
import PreviewContentBlocks, {
  ContentBlockProps,
} from '@common/modules/find-statistics/components/ContentBlocks';
import wrapEditableComponent, {
  EditingContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types/util';
import React, { useContext, useState } from 'react';
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
  allowComments: boolean;
  addContentButtonText?: string;
  onContentChange?: (content: ContentType) => void;
  onBlockSaveOrder?: (order: Dictionary<number>) => Promise<void>;
  onBlockContentChange: (blockId: string, content: string) => Promise<void>;
  onBlockDelete: (blockId: string) => () => Promise<void>;
}

const EditableContentBlock = ({
  content = [],
  sectionId,
  editable = true,
  onContentChange,
  canAddBlocks = false,
  canAddSingleBlock = false,
  textOnly = false,
  isReordering = false,
  allowComments = false,
  addContentButtonText = 'Add content',
  onBlockSaveOrder,
  onBlockContentChange,
  onBlockDelete,
}: Props) => {
  const editingContext = useContext(EditingContext);

  const [contentBlocks, setContentBlocks] = useState<ContentType>();

  React.useEffect(() => {
    setContentBlocks(content);
  }, [content]);

  React.useEffect(() => {
    if (!isReordering) {
      //save
      if (onBlockSaveOrder && contentBlocks !== undefined)
        onBlockSaveOrder(
          contentBlocks.reduce<Dictionary<number>>(
            (map, { id: blockId }, index) => ({
              ...map,
              [blockId]: index,
            }),
            {},
          ),
        );
    }
  }, [isReordering]);

  const onDragEnd = React.useCallback(
    (result: DropResult) => {
      const { source, destination, type } = result;

      if (type === 'content' && destination) {
        const newContentBlocks = [...(contentBlocks || [])];
        const [removed] = newContentBlocks.splice(source.index, 1);
        newContentBlocks.splice(destination.index, 0, removed);
        setContentBlocks(newContentBlocks);
      }
    },
    [contentBlocks, onContentChange],
  );

  if (contentBlocks === undefined || contentBlocks.length === 0) {
    return (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }

  return (
    <EditingContext.Provider
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
                    {allowComments &&
                      (editingContext.isCommenting ||
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
                  onContentChange={newContent =>
                    onBlockContentChange(block.id, newContent)
                  }
                  onDelete={() => onBlockDelete(block.id)()}
                />
              </ContentBlockDraggable>
            </div>
          ))}
        </ContentBlockDroppable>
      </DragDropContext>
    </EditingContext.Provider>
  );
};

const ContentBlocks = wrapEditableComponent(
  EditableContentBlock,
  PreviewContentBlocks,
);

export default ContentBlocks;
