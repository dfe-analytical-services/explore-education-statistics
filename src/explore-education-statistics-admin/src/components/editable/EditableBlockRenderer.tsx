import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { EditableBlock } from '@admin/services/types/content';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import DataBlockRenderer from '@common/modules/find-statistics/components/DataBlockRenderer';
import React, { useCallback, useMemo } from 'react';

interface Props {
  releaseId?: string;
  allowHeadings?: boolean;
  block: EditableBlock;
  editable?: boolean;
  getInfographic?: GetInfographic;
  onContentSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
}

function EditableBlockRenderer({
  releaseId,
  allowHeadings,
  block,
  editable,
  getInfographic,
  onContentSave,
  onDelete,
}: Props) {
  const blockId = `block-${block.id}`;

  const handleContentSave = useMemo(
    () => (content: string) => {
      onContentSave(block.id, content);
    },
    [block.id, onContentSave],
  );

  const handleDelete = useCallback(() => {
    onDelete(block.id);
  }, [block.id, onDelete]);

  switch (block.type) {
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableBlockWrapper onDelete={editable ? handleDelete : undefined}>
            <DataBlockRenderer
              releaseId={releaseId}
              id={blockId}
              dataBlock={block}
              getInfographic={getInfographic}
            />
          </EditableBlockWrapper>
        </div>
      );
    case 'HtmlBlock':
    case 'MarkDownBlock':
      return (
        <EditableContentBlock
          allowHeadings={allowHeadings}
          editable={editable}
          id={blockId}
          label="Block content"
          value={block.body}
          useMarkdown={block.type === 'MarkDownBlock'}
          onSave={handleContentSave}
          onDelete={handleDelete}
        />
      );
    default:
      return <div>Unable to edit content</div>;
  }
}

export default EditableBlockRenderer;
