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
  onContentChange: (content: string) => void;
  onDelete: (blockId: string) => void;
}

function EditableBlockRenderer({
  block,
  editable,
  allowHeadings,
  onContentChange,
  canDelete = false,
  onDelete,
}: Props) {
  const { releaseId } = useManageReleaseContext();

  const handleDelete = useCallback(() => {
    if (onDelete) {
      onDelete(block.id);
    }
  }, [block.id, onDelete]);

  switch (block.type) {
    case 'MarkDownBlock':
      return (
        <EditableMarkdownRenderer
          editable={editable}
          allowHeadings={allowHeadings}
          source={block.body}
          canDelete={canDelete}
          onDelete={handleDelete}
          onContentChange={onContentChange}
        />
      );
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableDataBlock
            id={`editableBlockRenderer-dataBlock-${block.id}`}
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
          editable={editable}
          allowHeadings={allowHeadings}
          source={block.body}
          canDelete={canDelete}
          onDelete={handleDelete}
          onContentChange={onContentChange}
        />
      );
    default:
      return <div>Unable to edit content</div>;
  }
}

export default EditableBlockRenderer;
