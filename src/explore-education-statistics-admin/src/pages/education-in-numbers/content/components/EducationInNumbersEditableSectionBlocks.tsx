import styles from '@admin/components/editable/EditableSectionBlocks.module.scss';
import { useEditingContext } from '@admin/contexts/EditingContext';
import InsetText from '@common/components/InsetText';
import { ReorderResult } from '@common/components/ReorderableItem';
import ReorderableList from '@common/components/ReorderableList';
import { EinContentBlock } from '@common/services/types/einBlocks';
import reorder from '@common/utils/reorder';
import React, { ReactNode, useCallback } from 'react';

export interface EditableSectionBlockProps<
  T extends EinContentBlock = EinContentBlock,
> {
  blocks: T[];
  isReordering?: boolean;
  onBlocksChange?: (nextBlocks: T[]) => void;
  renderBlock: (block: T) => ReactNode;
  renderEditableBlock: (block: T, ix?: number) => ReactNode;
}

const EditableSectionBlocks = <T extends EinContentBlock = EinContentBlock>({
  blocks = [],
  isReordering = false,
  renderBlock,
  renderEditableBlock,
  onBlocksChange,
}: EditableSectionBlockProps<T>) => {
  const { editingMode } = useEditingContext();

  const handleMoveBlock = useCallback(
    ({ prevIndex, nextIndex }: ReorderResult) => {
      onBlocksChange?.(reorder(blocks, prevIndex, nextIndex));
    },
    [blocks, onBlocksChange],
  );

  const renderReorderableBlockPreview = useCallback(
    (block: T) => {
      switch (block.type) {
        case 'HtmlBlock':
          return block.body ? renderBlock(block) : 'This section is empty';
        case 'TileGroupBlock':
          return renderBlock(block);
        default:
          return 'This section is empty';
      }
    },
    [renderBlock],
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
          label: (
            <div className={styles.draggable}>
              {renderReorderableBlockPreview(block)}
            </div>
          ),
        }))}
        onMoveItem={handleMoveBlock}
      />
    );
  }

  return (
    <div>
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
    </div>
  );
};

export default EditableSectionBlocks;
