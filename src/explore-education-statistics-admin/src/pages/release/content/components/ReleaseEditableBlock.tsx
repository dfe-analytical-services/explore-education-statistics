import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseImageUpload from '@admin/pages/release/hooks/useReleaseImageUpload';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseContentCommentService, {
  AddComment,
} from '@admin/services/releaseContentCommentService';
import { Comment, EditableBlock } from '@admin/services/types/content';
import Gate from '@common/components/Gate';
import useToggle from '@common/hooks/useToggle';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import { insertReleaseIdPlaceholders } from '@common/modules/release/utils/releaseImageUrls';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback } from 'react';
import { generatePath } from 'react-router';

interface Props {
  allowComments?: boolean;
  allowImages?: boolean;
  block: EditableBlock;
  editable?: boolean;
  publicationId: string;
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
  editable = true,
  publicationId,
  releaseId,
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

  const [isEditing, toggleEditing] = useToggle(false);
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
        toggleEditing.off();
      }

      toggleIsSaving.off();
      removeUnsavedBlock(block.id);
    },
    [
      onSave,
      block.id,
      removeUnsavedBlock,
      clearUnsavedCommentDeletions,
      toggleEditing,
      toggleIsSaving,
    ],
  );

  const handleDelete = useCallback(() => {
    onDelete(block.id);
  }, [block.id, onDelete]);

  const handleBlur = useCallback(
    (isDirty: boolean) => {
      if (isDirty) {
        addUnsavedBlock(block.id);
      }
    },
    [addUnsavedBlock, block.id],
  );

  const handleSaveComment = async (comment: AddComment) =>
    releaseContentCommentService.addContentSectionComment(
      releaseId,
      sectionId,
      block.id,
      comment,
    );

  const handleDeletePendingComment = useCallback(
    async (commentId: string) =>
      releaseContentCommentService.deleteContentSectionComment(commentId),
    [],
  );

  const handleSaveUpdatedComment = useCallback(
    async (comment: Comment) =>
      releaseContentCommentService.updateContentSectionComment(comment),
    [],
  );

  const blockId = `block-${block.id}`;

  switch (block.type) {
    case 'DataBlock':
      return (
        <div className="dfe-content-overflow">
          <EditableBlockWrapper
            dataBlockEditLink={generatePath<ReleaseDataBlockRouteParams>(
              releaseDataBlockEditRoute.path,
              {
                publicationId,
                releaseId,
                dataBlockId: block.id,
              },
            )}
            onDelete={editable ? handleDelete : undefined}
          >
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
        <CommentsContextProvider
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
            isEditing={isEditing}
            isSaving={isSaving}
            label="Content block"
            transformImageAttributes={transformImageAttributes}
            useMarkdown={block.type === 'MarkDownBlock'}
            value={block.body}
            onSave={handleSave}
            onDelete={handleDelete}
            onEditing={toggleEditing.on}
            onImageUpload={allowImages ? handleImageUpload : undefined}
            onImageUploadCancel={
              allowImages ? handleImageUploadCancel : undefined
            }
          />
        </CommentsContextProvider>
      );
    default:
      return <div>Unable to edit content</div>;
  }
};

export default ReleaseEditableBlock;
