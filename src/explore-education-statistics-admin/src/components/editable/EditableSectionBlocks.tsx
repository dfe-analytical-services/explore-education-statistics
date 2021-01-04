import BlockDraggable from '@admin/components/editable/BlockDraggable';
import BlockDroppable from '@admin/components/editable/BlockDroppable';
import Comments, {
  CommentsChangeHandler,
} from '@admin/components/editable/Comments';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { EditableBlock } from '@admin/services/types/content';
import InsetText from '@common/components/InsetText';
import SectionBlocks, {
  SectionBlocksProps,
} from '@common/modules/find-statistics/components/SectionBlocks';
import isBrowser from '@common/utils/isBrowser';
import reorder from '@common/utils/reorder';
import React, { useCallback, useState } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import classNames from 'classnames';
import EditableBlockRenderer from './EditableBlockRenderer';

export interface EditableSectionBlockProps extends SectionBlocksProps {
  content: EditableBlock[];
  sectionId: string;
  isReordering?: boolean;
  allowComments?: boolean;
  onBlocksChange?: (nextBlocks: EditableBlock[]) => void;
  onBlockContentSave: (blockId: string, content: string) => void;
  onBlockDelete: (blockId: string) => void;
  onBlockCommentsChange?: CommentsChangeHandler;
}

const EditableSectionBlocks = (props: EditableSectionBlockProps) => {
  const [openedCommentIds, setOpenedCommentIds] = useState<string[]>([]);

  const {
    releaseId,
    content = [],
    sectionId,
    isReordering = false,
    allowComments = false,
    getInfographic,
    onBlockContentSave,
    onBlockDelete,
    onBlocksChange,
    onBlockCommentsChange,
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
      if (!onBlockCommentsChange) {
        return;
      }

      if (content.find(block => block.id === blockId)) {
        onBlockCommentsChange(blockId, comments);
      }
    },
    [content, onBlockCommentsChange],
  );

  if (!isEditing) {
    return <SectionBlocks {...props} />;
  }

  if (content.length === 0) {
    return <InsetText>There is no content for this section.</InsetText>;
  }

  return (
    <DragDropContext onDragEnd={handleDragEnd}>
      <BlockDroppable droppable={isReordering} droppableId={sectionId}>
        {content.map((block, index) => (
          <div
            key={block.id}
            id={`editableSectionBlocks-${block.id}`}
            className={classNames('govuk-!-margin-bottom-9', {
              openSectionBlock: openedCommentIds.includes(block.id),
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
                          openedCommentIds.filter(e => e !== block.id),
                        )
                  }
                />
              )}

              <EditableBlockRenderer
                releaseId={releaseId}
                block={block}
                editable={!isReordering && !isBrowser('IE')}
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
