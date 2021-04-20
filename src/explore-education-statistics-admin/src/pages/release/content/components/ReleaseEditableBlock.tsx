import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseImageUpload from '@admin/pages/release/hooks/useReleaseImageUpload';
import { EditableBlock } from '@admin/services/types/content';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback } from 'react';
import { insertReleaseIdPlaceholders } from '@common/modules/release/utils/releaseImageUrls';

interface Props {
  allowImages?: boolean;
  releaseId: string;
  block: EditableBlock;
  editable?: boolean;
  onSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
}

const ReleaseEditableBlock = ({
  allowImages = false,
  releaseId,
  block,
  editable = true,
  onSave,
  onDelete,
}: Props) => {
  const blockId = `block-${block.id}`;

  const getChartFile = useGetChartFile(releaseId);

  const { handleImageUpload, handleImageUploadCancel } = useReleaseImageUpload(
    releaseId,
  );

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId,
  });

  const handleSave = useCallback(
    (content: string) => {
      const contentWithPlaceholders = insertReleaseIdPlaceholders(content);
      onSave(block.id, contentWithPlaceholders);
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
          label=""
          hideLabel
          value={block.body}
          useMarkdown={block.type === 'MarkDownBlock'}
          transformImageAttributes={transformImageAttributes}
          onSave={handleSave}
          onDelete={handleDelete}
          onImageUpload={allowImages ? handleImageUpload : undefined}
          onImageUploadCancel={
            allowImages ? handleImageUploadCancel : undefined
          }
        />
      );
    default:
      return <div>Unable to edit content</div>;
  }
};

export default ReleaseEditableBlock;
