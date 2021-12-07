import { CommentsProvider } from '@admin/contexts/comments/CommentsContext';
import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import useEditingActions from '@admin/contexts/editing/useEditingActions';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseImageUpload from '@admin/pages/release/hooks/useReleaseImageUpload';
import { EditableBlock } from '@admin/services/types/content';
import releaseContentCommentService, {
  AddComment,
  UpdateComment,
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
  const editingActions = useEditingActions();
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
        editingActions.removeUnsavedDeletionsForBlock(block.id);
      }

      toggleIsSaving.off();
      editingActions.removeUnsavedBlock(block.id);
    },
    [block.id, editingActions, onSave, toggleIsSaving],
  );

  const handleDelete = useCallback(() => {
    onDelete(block.id);
  }, [block.id, onDelete]);

  const handleBlur = (isDirty: boolean) => {
    if (isDirty) {
      editingActions.addUnsavedBlock(block.id);
    }
  };

  const handleCancel = () => {
    editingActions.removeUnsavedBlock(block.id);
  };

  const handleSaveComment = async (comment: AddComment) => {
    const addedComment = await releaseContentCommentService.addContentSectionComment(
      releaseId,
      sectionId,
      blockId.replace('block-', ''),
      comment,
    );
    return addedComment;
  };

  const handleDeletePendingComment = async (commentId: string) => {
    await releaseContentCommentService.deleteContentSectionComment(commentId);
  };

  const handleSaveUpdatedComment = async (comment: UpdateComment) => {
    const updatedComment = await releaseContentCommentService.updateContentSectionComment(
      comment,
    );
    return updatedComment;
  };

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
          value={{
            comments: block.comments,
            onSaveComment: handleSaveComment,
            onDeletePendingComment: handleDeletePendingComment,
            onSaveUpdatedComment: handleSaveUpdatedComment,
          }}
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
