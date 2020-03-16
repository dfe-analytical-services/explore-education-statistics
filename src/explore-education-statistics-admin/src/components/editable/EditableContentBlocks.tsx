import Comments from '@admin/modules/find-statistics/components/Comments';
import ContentBlockDraggable from '@admin/modules/find-statistics/components/ContentBlockDraggable';
import ContentBlockDroppable from '@admin/modules/find-statistics/components/ContentBlockDroppable';
import { EditableRelease } from '@admin/services/publicationService';
import PreviewContentBlocks, {
  ContentBlockProps,
} from '@common/modules/find-statistics/components/ContentBlocks';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types/util';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import EditableContentSubBlockRenderer from './EditableContentSubBlockRenderer';

type ContentType = EditableRelease['content'][0]['content'];

export type EditableContentType = ContentType;

export type ReorderHook = (sectionId?: string) => Promise<void>;

export interface EditableContentBlocksProps extends ContentBlockProps {
  content: ContentType;
  sectionId: string;
  editable?: boolean;
  isReordering?: boolean;
  allowComments: boolean;
  onBlockSaveOrder?: (order: Dictionary<number>) => void;
  onBlockContentChange: (blockId: string, content: string) => void;
  onBlockDelete: (blockId: string) => void;
}

const EditableContentBlocks = ({
  content = [],
  sectionId,
  editable = true,
  isReordering = false,
  allowComments = false,
  onBlockSaveOrder,
  onBlockContentChange,
  onBlockDelete,
}: EditableContentBlocksProps) => {
  const [contentBlocks, setContentBlocks] = useState<ContentType>();
  const isInitialMount = useRef(true);

  useEffect(() => {
    setContentBlocks(content);
  }, [content]);

  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
    } else if (!isReordering) {
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

  const onDragEnd = useCallback(
    (result: DropResult) => {
      const { source, destination, type } = result;

      if (type === 'content' && destination) {
        const newContentBlocks = [...(contentBlocks || [])];
        const [removed] = newContentBlocks.splice(source.index, 1);
        newContentBlocks.splice(destination.index, 0, removed);
        setContentBlocks(newContentBlocks);
      }
    },
    [contentBlocks],
  );

  if (contentBlocks === undefined || contentBlocks.length === 0) {
    return (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }

  return (
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
                  {allowComments && (
                    <Comments
                      sectionId={sectionId}
                      contentBlockId={block.id}
                      initialComments={block.comments}
                      canResolve={false}
                      canComment
                      onCommentsChange={async comments => {
                        const newBlocks = [...contentBlocks];
                        newBlocks[index] = { ...newBlocks[index], comments };
                        setContentBlocks(newBlocks);
                      }}
                    />
                  )}
                </>
              )}

              <EditableContentSubBlockRenderer
                editable={editable && !isReordering}
                canDelete={!isReordering}
                block={block}
                onContentChange={newContent =>
                  onBlockContentChange(block.id, newContent)
                }
                onDelete={() => onBlockDelete(block.id)}
              />
            </ContentBlockDraggable>
          </div>
        ))}
      </ContentBlockDroppable>
    </DragDropContext>
  );
};

const ContentBlocks = wrapEditableComponent(
  EditableContentBlocks,
  PreviewContentBlocks,
);

export default ContentBlocks;
