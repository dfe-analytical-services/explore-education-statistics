import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseImageUpload from '@admin/pages/release/hooks/useReleaseImageUpload';
import { EditableBlock } from '@admin/services/types/content';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import isBrowser from '@common/utils/isBrowser';
import { useEditingContext } from '@admin/contexts/EditingContext';
import React, { useCallback } from 'react';
import { insertReleaseIdPlaceholders } from '@common/modules/release/utils/releaseImageUrls';
import {
  addUnsavedEdit,
  removeUnsavedEdit,
} from '@admin/pages/release/content/components/utils/unsavedEditsUtils';

interface Props {
  allowImages?: boolean;
  releaseId: string;
  block: EditableBlock;
  sectionId: string;
  editable?: boolean;
  onSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
}

const ReleaseEditableBlock = ({
  allowImages = false,
  releaseId,
  block,
  sectionId,
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

  const { unsavedEdits, setUnsavedEdits } = useEditingContext();

  const handleSave = useCallback(
    (content: string) => {
      const contentWithPlaceholders = insertReleaseIdPlaceholders(content);
      onSave(block.id, contentWithPlaceholders);
      setUnsavedEdits(removeUnsavedEdit(unsavedEdits, sectionId, block.id));
    },
    [block.id, sectionId, onSave, unsavedEdits, setUnsavedEdits],
  );

  const handleDelete = useCallback(() => {
    onDelete(block.id);
  }, [block.id, onDelete]);

  const handleBlur = (isDirty: boolean) => {
    if (isDirty) {
      setUnsavedEdits(addUnsavedEdit(unsavedEdits, sectionId, block.id));
    }
  };

  const handleCancel = () => {
    setUnsavedEdits(removeUnsavedEdit(unsavedEdits, sectionId, block.id));
  };

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
          label="Content block"
          hideLabel
          value={block.body}
          useMarkdown={block.type === 'MarkDownBlock'}
          transformImageAttributes={transformImageAttributes}
          handleBlur={handleBlur}
          onCancel={handleCancel}
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
