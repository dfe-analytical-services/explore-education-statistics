import styles from '@admin/components/editable/EditableSectionBlocks.module.scss';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { EditableBlock } from '@admin/services/types/content';
import InsetText from '@common/components/InsetText';
import ReorderableList from '@common/components/ReorderableList';
import reorder from '@common/utils/reorder';
import React, { ReactNode, useCallback } from 'react';

export interface EditableSectionBlockProps<
  T extends EditableBlock = EditableBlock,
> {
  blocks: T[];
  isReordering?: boolean;
  onBlocksChange?: (nextBlocks: T[]) => void;
  renderBlock: (block: T) => ReactNode;
  renderEditableBlock: (block: T) => ReactNode;
}

const EditableSectionBlocks = <T extends EditableBlock = EditableBlock>({
  blocks = [],
  isReordering = false,
  renderBlock,
  renderEditableBlock,
  onBlocksChange,
}: EditableSectionBlockProps<T>) => {
  const { editingMode } = useEditingContext();

  const handleMoveBlock = useCallback(
    ({ prevIndex, nextIndex }: { prevIndex: number; nextIndex: number }) => {
      onBlocksChange?.(reorder(blocks, prevIndex, nextIndex));
    },
    [blocks, onBlocksChange],
  );

  if (editingMode !== 'edit') {
    return blocks.length > 0 ? (
      <>
        {blocks.map(block => (
          <div
            data-scroll
            id={`editableSectionBlocks-${block.id}`}
            key={block.id}
          >
            {renderBlock(block)}
          </div>
        ))}
      </>
    ) : (
      <InsetText>There is no content for this section.</InsetText>
    );
  }

  if (blocks.length === 0) {
    return <InsetText>There is no content for this section.</InsetText>;
  }

  if (isReordering) {
    return (
      <ReorderableList
        id="reorder-sections"
        list={blocks.map(block => ({
          id: block.id,
          label: <div className={styles.draggable}>{renderBlock(block)}</div>,
        }))}
        onMoveItem={handleMoveBlock}
      />
    );
  }

  return (
    <>
      {blocks.map(block => (
        <div
          key={block.id}
          id={`editableSectionBlocks-${block.id}`}
          className="govuk-!-margin-bottom-9"
          data-scroll
          data-testid="editableSectionBlock"
        >
          {renderEditableBlock(block)}
        </div>
      ))}
    </>
  );
};

export default EditableSectionBlocks;
