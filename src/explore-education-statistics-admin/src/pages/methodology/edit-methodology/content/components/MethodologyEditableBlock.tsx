import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { EditableContentBlock as EditableContentBlockType } from '@admin/services/types/content';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback } from 'react';

interface Props {
  block: EditableContentBlockType;
  editable?: boolean;
  onSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
}

const MethodologyEditableBlock = ({
  block,
  editable = true,
  onSave,
  onDelete,
}: Props) => {
  const blockId = `block-${block.id}`;

  const handleSave = useCallback(
    (content: string) => {
      onSave(block.id, content);
    },
    [block.id, onSave],
  );

  const handleDelete = useCallback(() => {
    onDelete(block.id);
  }, [block.id, onDelete]);

  switch (block.type) {
    case 'HtmlBlock':
    case 'MarkDownBlock':
      return (
        <EditableContentBlock
          editable={editable && !isBrowser('IE')}
          id={blockId}
          label="Block content"
          value={block.body}
          useMarkdown={block.type === 'MarkDownBlock'}
          onSave={handleSave}
          onDelete={handleDelete}
        />
      );
    default:
      return <div>Unable to edit content</div>;
  }
};

export default MethodologyEditableBlock;
