import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import { EditableBlock } from '@admin/services/types/content';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback } from 'react';

interface Props {
  releaseId: string;
  block: EditableBlock;
  editable?: boolean;
  onSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
}

const ReleaseEditableBlock = ({
  releaseId,
  block,
  editable = true,
  onSave,
  onDelete,
}: Props) => {
  const blockId = `block-${block.id}`;

  const getChartFile = useGetChartFile(releaseId);

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
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableBlockWrapper onDelete={editable ? handleDelete : undefined}>
            <DataBlockTabs
              releaseId={releaseId}
              id={blockId}
              dataBlock={block}
              getInfographic={getChartFile}
            />
          </EditableBlockWrapper>
        </div>
      );
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

export default ReleaseEditableBlock;
