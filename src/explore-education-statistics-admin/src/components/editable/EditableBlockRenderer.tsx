import EditableDataBlock from '@admin/components/editable/EditableDataBlock';
import EditableHtmlRenderer from '@admin/components/editable/EditableHtmlRenderer';
import EditableMarkdownRenderer from '@admin/components/editable/EditableMarkdownRenderer';
import EditableProps from '@admin/components/editable/types/EditableProps';
import { EditableBlock } from '@admin/services/publicationService';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import { OmitStrict } from '@common/types';
import React, { useCallback } from 'react';

interface Props extends OmitStrict<EditableProps, 'onDelete'> {
  allowHeadings?: boolean;
  block: EditableBlock;
  getInfographic?: GetInfographic;
  onContentSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
}

function EditableBlockRenderer({
  allowHeadings,
  block,
  canDelete = false,
  editable,
  getInfographic,
  onContentSave,
  onDelete,
}: Props) {
  const id = `editableDataBlockRenderer-dataBlock-${block.id}`;

  const handleSave = useCallback(
    (content: string) => {
      onContentSave(block.id, content);
    },
    [block.id, onContentSave],
  );

  const handleDelete = useCallback(() => {
    if (onDelete) {
      onDelete(block.id);
    }
  }, [block.id, onDelete]);

  switch (block.type) {
    case 'MarkDownBlock':
      return (
        <EditableMarkdownRenderer
          id={id}
          label="Block content"
          editable={editable}
          allowHeadings={allowHeadings}
          source={block.body}
          canDelete={canDelete}
          onDelete={handleDelete}
          onSave={handleSave}
        />
      );
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableDataBlock
            id={id}
            dataBlock={block}
            editable={editable}
            getInfographic={getInfographic}
            canDelete={canDelete}
            onDelete={handleDelete}
          />
        </div>
      );
    case 'HtmlBlock':
      return (
        <EditableHtmlRenderer
          id={id}
          label="Block content"
          editable={editable}
          allowHeadings={allowHeadings}
          source={block.body}
          canDelete={canDelete}
          onDelete={handleDelete}
          onSave={handleSave}
        />
      );
    default:
      return <div>Unable to edit content</div>;
  }
}

export default EditableBlockRenderer;
