import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { methodologyToolbarConfigFull } from '@admin/config/ckEditorConfig';
import { EditableContentBlock as EditableContentBlockType } from '@admin/services/types/content';
import useToggle from '@common/hooks/useToggle';
import isBrowser from '@common/utils/isBrowser';
import React, { ReactNode, useCallback } from 'react';
import useMethodologyImageUpload from '@admin/pages/methodology/hooks/useMethodologyImageUpload';
import { insertMethodologyIdPlaceholders } from '@common/modules/methodology/utils/methodologyImageUrls';
import useMethodologyImageAttributeTransformer from '@common/modules/methodology/hooks/useMethodologyImageAttributeTransformer';

interface Props {
  allowImages?: boolean;
  editButtonLabel?: ReactNode | string;
  removeButtonLabel?: ReactNode | string;
  methodologyId: string;
  block: EditableContentBlockType;
  editable?: boolean;
  onSave: (blockId: string, content: string) => void;
  onDelete: (blockId: string) => void;
  sectionHeading?: string;
  contentBlockNumber?: number;
}

const MethodologyEditableBlock = ({
  allowImages = false,
  editButtonLabel,
  removeButtonLabel,
  methodologyId,
  block,
  editable = true,
  onSave,
  onDelete,
  sectionHeading,
  contentBlockNumber,
}: Props) => {
  const blockId = `block-${block.id}`;

  const [isEditing, toggleEditing] = useToggle(false);

  const { handleImageUpload, handleImageUploadCancel } =
    useMethodologyImageUpload(methodologyId);

  const transformImageAttributes = useMethodologyImageAttributeTransformer({
    methodologyId,
  });

  const handleSave = useCallback(
    (content: string) => {
      const contentWithPlaceholders = insertMethodologyIdPlaceholders(content);
      onSave(block.id, contentWithPlaceholders);
      toggleEditing.off();
    },
    [block.id, onSave, toggleEditing],
  );

  const handleDelete = useCallback(() => {
    onDelete(block.id);
  }, [block.id, onDelete]);

  const labelWithHeading = sectionHeading
    ? `Content block ${contentBlockNumber} for ${sectionHeading} section`
    : 'Content block';

  switch (block.type) {
    case 'HtmlBlock':
      return (
        <EditableContentBlock
          editable={editable && !isBrowser('IE')}
          editButtonLabel={editButtonLabel}
          id={blockId}
          isEditing={isEditing}
          label={labelWithHeading} // Because hideLabel is true, this only affects the aria-label of the ckeditor
          hideLabel
          removeButtonLabel={removeButtonLabel}
          toolbarConfig={methodologyToolbarConfigFull}
          value={block.body || ''}
          transformImageAttributes={transformImageAttributes}
          onCancel={toggleEditing.off}
          onEditing={toggleEditing.on}
          onSubmit={handleSave}
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
