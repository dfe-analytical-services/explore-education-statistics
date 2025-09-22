import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { educationInNumbersToolbarConfig } from '@admin/config/ckEditorConfig';
import useToggle from '@common/hooks/useToggle';
import {
  EinBlockType,
  EinContentBlock,
} from '@common/services/types/einBlocks';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback } from 'react';
import EditableTileGroupBlock from './EditableTileGroupBlock';

interface Props {
  block: EinContentBlock;
  editable?: boolean;
  sectionId: string;
  onSave: (blockId: string, content: string, blockType: EinBlockType) => void;
  onDelete: (blockId: string) => void;
}

const EducationInNumbersEditableBlock = ({
  block,
  editable = true,
  sectionId,
  onSave,
  onDelete,
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

  switch (block.type) {
    case 'HtmlBlock':
      return (
        <EditableContentBlock
          editable={editable && !isBrowser('IE')}
          id={blockId}
          isEditing={isEditing}
          label="Content block"
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
