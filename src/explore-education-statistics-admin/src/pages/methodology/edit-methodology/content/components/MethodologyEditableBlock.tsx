import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { EditableContentBlock as EditableContentBlockType } from '@admin/services/types/content';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback } from 'react';
import useMethodologyImageUpload from '@admin/pages/methodology/hooks/useMethodologyImageUpload';
import { insertMethodologyIdPlaceholders } from '@common/modules/methodology/utils/methodologyImageUrls';
import useMethodologyImageAttributeTransformer from '@common/modules/methodology/hooks/useMethodologyImageAttributeTransformer';

interface Props {
  allowImages?: boolean;
  methodologyId: string;
  block: EditableContentBlockType;
  editable?: boolean;
  onSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
}

const MethodologyEditableBlock = ({
  allowImages = false,
  methodologyId,
  block,
  editable = true,
  onSave,
  onDelete,
}: Props) => {
  const blockId = `block-${block.id}`;

  const {
    handleImageUpload,
    handleImageUploadCancel,
  } = useMethodologyImageUpload(methodologyId);

  const transformImageAttributes = useMethodologyImageAttributeTransformer({
    methodologyId,
  });

  const handleSave = useCallback(
    (content: string) => {
      const contentWithPlaceholders = insertMethodologyIdPlaceholders(content);
      onSave(block.id, contentWithPlaceholders);
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
          label="Content block"
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

export default MethodologyEditableBlock;
