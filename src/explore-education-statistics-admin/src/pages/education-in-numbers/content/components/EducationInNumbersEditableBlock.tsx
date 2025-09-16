import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { educationInNumbersToolbarConfig } from '@admin/config/ckEditorConfig';
import { EditableContentBlock as EditableContentBlockType } from '@admin/services/types/content';
import useToggle from '@common/hooks/useToggle';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback } from 'react';

interface Props {
  block: EditableContentBlockType;
  editable?: boolean;
  onSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
}

const EducationInNumbersEditableBlock = ({
  block,
  editable = true,
  onSave,
  onDelete,
}: Props) => {
  const blockId = `block-${block.id}`;

  const [isEditing, toggleEditing] = useToggle(false);

  const handleSave = useCallback(
    (content: string) => {
      onSave(block.id, content);
      toggleEditing.off();
    },
    [block.id, onSave, toggleEditing],
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
    default:
      return <div>Unable to edit content</div>;
  }
};

export default EducationInNumbersEditableBlock;
