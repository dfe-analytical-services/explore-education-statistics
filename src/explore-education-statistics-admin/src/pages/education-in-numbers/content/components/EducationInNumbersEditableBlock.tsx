import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { educationInNumbersToolbarConfig } from '@admin/config/ckEditorConfig';
import useToggle from '@common/hooks/useToggle';
import {
  EinBlockType,
  EinContentBlock,
} from '@common/services/types/einBlocks';
import isBrowser from '@common/utils/isBrowser';
import React, { ReactNode, useCallback } from 'react';
import EditableTileGroupBlock from './EditableTileGroupBlock';

interface Props {
  block: EinContentBlock;
  editable?: boolean;
  editButtonLabel?: ReactNode | string;
  groupButtonsLabel?: ReactNode | string;
  removeButtonLabel?: ReactNode | string;
  sectionId: string;
  onSave: (blockId: string, content: string, blockType: EinBlockType) => void;
  onDelete: (blockId: string) => void;
  sectionHeading?: string;
  contentBlockNumber?: number;
}

const EducationInNumbersEditableBlock = ({
  block,
  editable = true,
  editButtonLabel,
  groupButtonsLabel,
  removeButtonLabel,
  sectionId,
  onSave,
  onDelete,
  sectionHeading,
  contentBlockNumber,
}: Props) => {
  const blockId = `block-${block.id}`;

  const [isEditing, toggleEditing] = useToggle(false);

  // Handles saving both block types, with content
  // being either the HTML body or the TileGroup title
  const handleSave = useCallback(
    (content: string) => {
      onSave(block.id, content, block.type);
      toggleEditing.off();
    },
    [block.type, block.id, onSave, toggleEditing],
  );

  const handleDelete = useCallback(() => {
    onDelete(block.id);
  }, [block.id, onDelete]);

  const labelWithHeading = sectionHeading
    ? `Content block ${contentBlockNumber} for ${sectionHeading} section`
    : 'Content block';

  switch (block.type) {
    case 'HtmlBlock':
      return (
        <EditableContentBlock
          editable={editable && !isBrowser('IE')}
          editButtonLabel={editButtonLabel}
          removeButtonLabel={removeButtonLabel}
          id={blockId}
          isEditing={isEditing}
          label={labelWithHeading} // Because hideLabel is true, this only affects the aria-label of the ckeditor // Because hideLabel is true, this only affects the aria-label of the ckeditor
          hideLabel
          value={block.body}
          toolbarConfig={educationInNumbersToolbarConfig}
          onCancel={toggleEditing.off}
          onEditing={toggleEditing.on}
          onSubmit={handleSave}
          onDelete={handleDelete}
        />
      );
    case 'TileGroupBlock':
      return (
        <EditableTileGroupBlock
          block={block}
          editable={editable && !isBrowser('IE')}
          groupButtonsLabel={groupButtonsLabel}
          removeButtonLabel={removeButtonLabel}
          sectionId={sectionId}
          onDelete={handleDelete}
          onSave={handleSave}
        />
      );
    default:
      return <div>Unable to edit content</div>;
  }
};

export default EducationInNumbersEditableBlock;
