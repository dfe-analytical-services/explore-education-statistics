import BlockDraggable from '@admin/components/editable/BlockDraggable';
import BlockDroppable from '@admin/components/editable/BlockDroppable';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { EditableBlock } from '@admin/services/types/content';
import InsetText from '@common/components/InsetText';
import reorder from '@common/utils/reorder';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import React, { Fragment, ReactNode, useCallback } from 'react';

export interface EditableSectionBlockProps<
  T extends EditableBlock = EditableBlock
> {
  blocks: T[];
  isReordering?: boolean;
  sectionId: string;
  onBlocksChange?: (nextBlocks: T[]) => void;
  renderBlock: (block: T) => ReactNode;
  renderEditableBlock: (block: T) => ReactNode;
}

const EditableSectionBlocks = <T extends EditableBlock = EditableBlock>({
  blocks = [],
  isReordering = false,
  sectionId,
  renderBlock,
  renderEditableBlock,
  onBlocksChange,
}: EditableSectionBlockProps<T>) => {
  const { editingMode } = useEditingContext();

  const handleDragEnd = useCallback(
    ({ source, destination }: DropResult) => {
      if (destination && onBlocksChange) {
        onBlocksChange(reorder(blocks, source.index, destination.index));
      }
    },
    [blocks, onBlocksChange],
  );

  if (editingMode !== 'edit') {
    return blocks.length > 0 ? (
      <>
        {blocks.map(block => (
          <Fragment key={block.id}>{renderBlock(block)}</Fragment>
        ))}
      </>
    ) : (
      <InsetText>There is no content for this section.</InsetText>
    );
  }

  if (blocks.length === 0) {
    return <InsetText>There is no content for this section.</InsetText>;
  }

  return (
    <DragDropContext onDragEnd={handleDragEnd}>
      <BlockDroppable droppable={isReordering} droppableId={sectionId}>
        {blocks.map((block, index) => (
          <div
            key={block.id}
            id={`editableSectionBlocks-${block.id}`}
            className="govuk-!-margin-bottom-9"
            data-testid="editableSectionBlock"
          >
            <BlockDraggable
              draggable={isReordering}
              draggableId={block.id}
              key={block.id}
              index={index}
            >
              {renderEditableBlock(block)}
            </BlockDraggable>
          </div>
        ))}
      </BlockDroppable>
    </DragDropContext>
  );
};

export default EditableSectionBlocks;
