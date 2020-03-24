import BlockDraggable from '@admin/modules/find-statistics/components/BlockDraggable';
import BlockDroppable from '@admin/modules/find-statistics/components/BlockDroppable';
import Comments from '@admin/modules/find-statistics/components/Comments';
import { EditableBlock } from '@admin/services/publicationService';
import SectionBlocks, { BlocksProps } from '@common/modules/find-statistics/components/SectionBlocks';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types/util';
import orderBy from 'lodash/orderBy';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import EditableBlockRenderer from './EditableBlockRenderer';

export interface Props extends BlocksProps {
  content: EditableBlock[];
  sectionId: string;
  editable?: boolean;
  isReordering?: boolean;
  insideAccordion?: boolean;
  allowComments: boolean;
  onBlockSaveOrder?: (order: Dictionary<number>) => void;
  onBlockContentChange: (blockId: string, content: string) => void;
  onBlockDelete: (blockId: string) => void;
}

const EditableSectionBlocks = ({
  content = [],
  sectionId,
  editable = true,
  isReordering = false,
  insideAccordion,
  allowComments = false,
  onBlockSaveOrder,
  onBlockContentChange,
  onBlockDelete,
}: Props) => {
  const [blocks, setBlocks] = useState<EditableBlock[]>();
  const isInitialMount = useRef(true);

  useEffect(() => {
    setBlocks(orderBy(content, 'order'));
  }, [content]);

  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
    } else if (!isReordering) {
      if (onBlockSaveOrder && blocks !== undefined)
        onBlockSaveOrder(
          blocks.reduce<Dictionary<number>>(
            (map, { id: blockId }, index) => ({
              ...map,
              [blockId]: index,
            }),
            {},
          ),
        );
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isReordering]);

  const onDragEnd = useCallback(
    (result: DropResult) => {
      const { source, destination, type } = result;

      if (type === 'content' && destination) {
        const newBlocks = [...(blocks || [])];
        const [removed] = newBlocks.splice(source.index, 1);
        newBlocks.splice(destination.index, 0, removed);
        setBlocks(newBlocks);
      }
    },
    [blocks],
  );

  if (blocks === undefined || blocks.length === 0) {
    return (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }

  return (
    <DragDropContext onDragEnd={onDragEnd}>
      <BlockDroppable draggable={isReordering} droppableId={`${sectionId}`}>
        {blocks.map((block, index) => (
          <div
            key={`content-section-${block.id}`}
            id={`content-section-${block.id}`}
            className="govuk-!-margin-bottom-9"
          >
            <BlockDraggable
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
                        const newBlocks = [...blocks];
                        newBlocks[index] = { ...newBlocks[index], comments };
                        setBlocks(newBlocks);
                      }}
                    />
                  )}
                </>
              )}

              <EditableBlockRenderer
                editable={editable && !isReordering}
                canDelete={!isReordering}
                block={block}
                insideAccordion={insideAccordion}
                onContentChange={newContent =>
                  onBlockContentChange(block.id, newContent)
                }
                onDelete={() => onBlockDelete(block.id)}
              />
            </BlockDraggable>
          </div>
        ))}
      </BlockDroppable>
    </DragDropContext>
  );
};

export default wrapEditableComponent(EditableSectionBlocks, SectionBlocks);
