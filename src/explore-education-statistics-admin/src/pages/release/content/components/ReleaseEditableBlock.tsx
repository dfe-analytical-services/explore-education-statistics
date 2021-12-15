import { CommentsProvider } from '@admin/contexts/CommentsContext';
import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseImageUpload from '@admin/pages/release/hooks/useReleaseImageUpload';
import { Comment, EditableBlock } from '@admin/services/types/content';
import releaseContentCommentService, {
  AddComment,
} from '@admin/services/releaseContentCommentService';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import Gate from '@common/components/Gate';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback } from 'react';
import { insertReleaseIdPlaceholders } from '@common/modules/release/utils/releaseImageUrls';
import useToggle from '@common/hooks/useToggle';

interface Props {
  allowComments?: boolean;
  allowImages?: boolean;
  block: EditableBlock;
  editable?: boolean;
  releaseId: string;
  sectionId: string;
  visible?: boolean;
  onDelete: (blockId: string) => void;
  onSave: (blockId: string, content: string) => void;
}

const ReleaseEditableBlock = ({
  allowComments = false,
  allowImages = false,
  block,
  releaseId,
  editable = true,
  sectionId,
  visible,
  onDelete,
  onSave,
}: Props) => {
  const {
    addUnsavedBlock,
    clearUnsavedCommentDeletions,
    removeUnsavedBlock,
    updateUnresolvedComments,
    updateUnsavedCommentDeletions,
  } = useEditingContext();
  const blockId = `block-${block.id}`;
  const [isSaving, toggleIsSaving] = useToggle(false);

  const getChartFile = useGetChartFile(releaseId);

  const { handleImageUpload, handleImageUploadCancel } = useReleaseImageUpload(
    releaseId,
  );

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId,
  });

  const handleSave = useCallback(
    async (content: string, isAutoSave?: boolean) => {
      toggleIsSaving.on();
      const contentWithPlaceholders = insertReleaseIdPlaceholders(content);
      await onSave(block.id, contentWithPlaceholders);

      if (!isAutoSave) {
        clearUnsavedCommentDeletions(block.id);
      }

      toggleIsSaving.off();
      removeUnsavedBlock(block.id);
    },
    [
      block.id,
      clearUnsavedCommentDeletions,
      removeUnsavedBlock,
      onSave,
      toggleIsSaving,
    ],
  );

  const handleDelete = useCallback(() => {
    onDelete(block.id);
  }, [block.id, onDelete]);

  const handleBlur = (isDirty: boolean) => {
    if (isDirty) {
      addUnsavedBlock(block.id);
    }
  };

  const handleCancel = () => {
    removeUnsavedBlock(block.id);
  };

  const handleSaveComment = async (comment: AddComment) =>
    releaseContentCommentService.addContentSectionComment(
      releaseId,
      sectionId,
      blockId.replace('block-', ''),
      comment,
    );

  const handleDeletePendingComment = async (commentId: string) =>
    releaseContentCommentService.deleteContentSectionComment(commentId);

  const handleSaveUpdatedComment = async (comment: Comment) =>
    releaseContentCommentService.updateContentSectionComment(comment);

  switch (block.type) {
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableBlockWrapper onDelete={editable ? handleDelete : undefined}>
            <Gate condition={!!visible}>
              <DataBlockTabs
                releaseId={releaseId}
                id={blockId}
                dataBlock={block}
                getInfographic={getChartFile}
              />
            </Gate>
          </EditableBlockWrapper>
        </div>
      );
    case 'HtmlBlock':
    case 'MarkDownBlock':
      return (
        <CommentsProvider
          comments={block.comments}
          onDeleteComment={handleDeletePendingComment}
          onSaveComment={handleSaveComment}
          onSaveUpdatedComment={handleSaveUpdatedComment}
          onUpdateUnresolvedComments={updateUnresolvedComments}
          onUpdateUnsavedCommentDeletions={updateUnsavedCommentDeletions}
        >
          <EditableContentBlock
            allowComments={allowComments}
            autoSave
            editable={editable && !isBrowser('IE')}
            handleBlur={handleBlur}
            hideLabel
            id={blockId}
            isSaving={isSaving}
            label="Content block"
            transformImageAttributes={transformImageAttributes}
            useMarkdown={block.type === 'MarkDownBlock'}
            value={block.body}
            onCancel={handleCancel}
            onSave={handleSave}
            onDelete={handleDelete}
            onImageUpload={allowImages ? handleImageUpload : undefined}
            onImageUploadCancel={
              allowImages ? handleImageUploadCancel : undefined
            }
          />
        </CommentsProvider>
      );
    default:
      return <div>Unable to edit content</div>;
  }
};

export default ReleaseEditableBlock;
