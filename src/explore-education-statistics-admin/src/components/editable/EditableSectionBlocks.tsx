import { useEditingContext } from '@admin/contexts/EditingContext';
import BlockDraggable from '@admin/modules/find-statistics/components/BlockDraggable';
import BlockDroppable from '@admin/modules/find-statistics/components/BlockDroppable';
import Comments, {
  CommentsChangeHandler,
} from '@admin/modules/find-statistics/components/Comments';
import { EditableBlock } from '@admin/services/publicationService';
import SectionBlocks, {
  SectionBlocksProps,
} from '@common/modules/find-statistics/components/SectionBlocks';
import reorder from '@common/utils/reorder';
import React, { useCallback } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import EditableBlockRenderer from './EditableBlockRenderer';

export interface EditableSectionBlockProps extends SectionBlocksProps {
  content: EditableBlock[];
  sectionId: string;
  editable?: boolean;
  isReordering?: boolean;
  allowHeadings?: boolean;
  allowComments?: boolean;
  onBlocksChange?: (nextBlocks: EditableBlock[]) => void;
  onBlockContentSave: (blockId: string, content: string) => void;
  onBlockDelete: (blockId: string) => void;
}

const EditableSectionBlocks = (props: EditableSectionBlockProps) => {
  const {
    content = [],
    sectionId,
    editable = true,
    isReordering = false,
    allowHeadings,
    allowComments = false,
    getInfographic,
    onBlockContentSave,
    onBlockDelete,
    onBlocksChange,
  } = props;

  const { isEditing } = useEditingContext();

  const handleDragEnd = useCallback(
    ({ source, destination }: DropResult) => {
      if (destination && onBlocksChange) {
        onBlocksChange(reorder(content, source.index, destination.index));
      }
    },
    [content, onBlocksChange],
  );

  const handleCommentsChange: CommentsChangeHandler = useCallback(
    (blockId, comments) => {
      if (!onBlocksChange) {
        return;
      }

      const blockIndex = content.findIndex(block => block.id === blockId);

      if (blockIndex > -1) {
        const nextBlocks = [...content];
        nextBlocks[blockIndex].comments = comments;

        onBlocksChange(nextBlocks);
      }
    },
    [content, onBlocksChange],
  );

  if (!isEditing) {
    return <SectionBlocks {...props} />;
  }

  if (content.length === 0) {
    return (
      <div className="govuk-inset-text">
        There is no content for this section.
      </div>
    );
  }

  return (
    <DragDropContext onDragEnd={handleDragEnd}>
      <BlockDroppable droppable={isReordering} droppableId={sectionId}>
        {content.map((block, index) => (
          <div
            key={block.id}
            id={`editableSectionBlocks-${block.id}`}
            className="govuk-!-margin-bottom-9"
          >
            <BlockDraggable
              draggable={isReordering}
              draggableId={block.id}
              key={block.id}
              index={index}
            >
              {!isReordering && allowComments && (
                <Comments
                  sectionId={sectionId}
                  blockId={block.id}
                  comments={block.comments}
                  canResolve={false}
                  canComment
                  onChange={handleCommentsChange}
                />
              )}

              <EditableBlockRenderer
                editable={editable && !isReordering}
                canDelete={!isReordering}
                block={block}
                allowHeadings={allowHeadings}
                getInfographic={getInfographic}
                onContentSave={onBlockContentSave}
                onDelete={onBlockDelete}
              />
            </BlockDraggable>
          </div>
        ))}
      </BlockDroppable>
    </DragDropContext>
  );
};

export default EditableSectionBlocks;
