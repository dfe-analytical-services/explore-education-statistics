import EditableDataBlock from '@admin/components/editable/EditableDataBlock';
import EditableHtmlRenderer from '@admin/components/editable/EditableHtmlRenderer';
import EditableMarkdownRenderer from '@admin/components/editable/EditableMarkdownRenderer';
import EditableProps from '@admin/components/editable/types/EditableProps';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import { EditableBlock } from '@admin/services/publicationService';
import { OmitStrict } from '@common/types';
import React, { useCallback } from 'react';

interface Props extends OmitStrict<EditableProps, 'onDelete'> {
  allowHeadings?: boolean;
  block: EditableBlock;
  onSave: (content: string) => void;
  onDelete: (blockId: string) => void;
}

function EditableBlockRenderer({
  block,
  editable,
  allowHeadings,
  onSave,
  canDelete = false,
  onDelete,
}: Props) {
  const { releaseId } = useManageReleaseContext();
  const id = `editableDataBlockRenderer-dataBlock-${block.id}`;

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
          onSave={onSave}
        />
      );
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableDataBlock
            id={id}
            dataBlock={block}
            editable={editable}
            releaseId={releaseId}
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
          onSave={onSave}
        />
      );
    default:
      return <div>Unable to edit content</div>;
  }
}

export default EditableBlockRenderer;
