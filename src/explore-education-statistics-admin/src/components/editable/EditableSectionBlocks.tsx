import BlockDraggable from '@admin/components/editable/BlockDraggable';
import BlockDroppable from '@admin/components/editable/BlockDroppable';
import Comments, {
  CommentsChangeHandler,
} from '@admin/components/editable/Comments';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { EditableBlock } from '@admin/services/types/content';
import InsetText from '@common/components/InsetText';
import reorder from '@common/utils/reorder';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import classNames from 'classnames';
import React, { Fragment, ReactNode, useCallback, useState } from 'react';
import styles from './EditableSectionBlocks.module.scss';

export interface EditableSectionBlockProps<
  T extends EditableBlock = EditableBlock
> {
  allowComments?: boolean;
  blocks: T[];
  isReordering?: boolean;
  sectionId: string;
  onBlocksChange?: (nextBlocks: T[]) => void;
  onBlockCommentsChange?: CommentsChangeHandler;
  renderBlock: (block: T) => ReactNode;
  renderEditableBlock: (block: T, index: number) => ReactNode;
}

const EditableSectionBlocks = <T extends EditableBlock = EditableBlock>({
  allowComments = false,
  blocks = [],
  isReordering = false,
  sectionId,
  renderBlock,
  renderEditableBlock,
  onBlocksChange,
  onBlockCommentsChange,
}: EditableSectionBlockProps<T>) => {
  const { editingMode } = useEditingContext();

  const [openedCommentIds, setOpenedCommentIds] = useState<string[]>([]);

  const handleDragEnd = useCallback(
    ({ source, destination }: DropResult) => {
      if (destination && onBlocksChange) {
        onBlocksChange(reorder(blocks, source.index, destination.index));
      }
    },
    [blocks, onBlocksChange],
  );

  const handleCommentsChange: CommentsChangeHandler = useCallback(
    (blockId, comments) => {
      if (!onBlockCommentsChange) {
        return;
      }

      if (blocks.find(block => block.id === blockId)) {
        onBlockCommentsChange(blockId, comments);
      }
    },
    [blocks, onBlockCommentsChange],
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
            className={classNames('govuk-!-margin-bottom-9', {
              [styles.openSectionBlock]: openedCommentIds.includes(block.id),
            })}
            data-testid="editableSectionBlock"
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
                  canComment
                  onChange={handleCommentsChange}
                  onToggle={opened =>
                    opened
                      ? setOpenedCommentIds([block.id, ...openedCommentIds])
                      : setOpenedCommentIds(
                          openedCommentIds.filter(id => id !== block.id),
                        )
                  }
                />
              )}

              {renderEditableBlock(block, index)}
            </BlockDraggable>
          </div>
        ))}
      </BlockDroppable>
    </DragDropContext>
  );
};

export default EditableSectionBlocks;
