import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import EditableEmbedBlock from '@admin/components/editable/EditableEmbedBlock';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { useReleaseContentHubContext } from '@admin/contexts/ReleaseContentHubContext';
import useBlockLock from '@admin/hooks/useBlockLock';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import { EditableEmbedFormValues } from '@admin/components/editable/EditableEmbedForm';
import { ContentSectionKeys } from '@admin/pages/release/content/contexts/ReleaseContentContextActionTypes';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { useReleaseContentDispatch } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import useReleaseImageUpload from '@admin/pages/release/hooks/useReleaseImageUpload';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
} from '@admin/routes/releaseRoutes';
import { CommentCreate } from '@admin/services/releaseContentCommentService';
import { Comment, EditableBlock } from '@admin/services/types/content';
import Gate from '@common/components/Gate';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import { insertReleaseIdPlaceholders } from '@common/modules/release/utils/releaseImageUrls';
import isBrowser from '@common/utils/isBrowser';
import React, { useCallback, useEffect } from 'react';
import { generatePath } from 'react-router';

interface Props {
  allowComments?: boolean;
  allowImages?: boolean;
  block: EditableBlock;
  editable?: boolean;
  publicationId: string;
  releaseId: string;
  sectionId: string;
  sectionKey: ContentSectionKeys;
  visible?: boolean;
}

const ReleaseEditableBlock = ({
  allowComments = false,
  allowImages = false,
  block,
  editable = true,
  publicationId,
  releaseId,
  sectionId,
  sectionKey,
  visible,
}: Props) => {
  const {
    addUnsavedBlock,
    clearUnsavedCommentDeletions,
    removeUnsavedBlock,
    updateUnresolvedComments,
    updateUnsavedCommentDeletions,
  } = useEditingContext();

  const {
    addBlockComment,
    deleteBlockComment,
    updateBlockComment,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    deleteEmbedSectionBlock,
    updateEmbedSectionBlock,
  } = useReleaseContentActions();
  const dispatch = useReleaseContentDispatch();

  const { hub } = useReleaseContentHubContext();

  const getChartFile = useGetChartFile(releaseId);

  const { handleImageUpload, handleImageUploadCancel } = useReleaseImageUpload(
    releaseId,
  );

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseId,
  });

  const updateBlock = useCallback(
    (nextBlock: EditableBlock) => {
      dispatch({
        type: 'UPDATE_SECTION_BLOCK',
        payload: {
          block: nextBlock,
          meta: {
            blockId: block.id,
            sectionId,
            sectionKey,
          },
        },
      });
    },
    [block.id, dispatch, sectionId, sectionKey],
  );

  const {
    isLocking,
    isLockedByOtherUser,
    isLockedByUser,
    locked,
    lockedBy,
    lockThrottle,
    endLock,
    setLock,
    startLock,
    refreshLock,
  } = useBlockLock({
    getLock: () => hub.lockContentBlock(block.id),
    unlock: () => hub.unlockContentBlock(block.id),
    initialLock: {
      locked: block.locked,
      lockedUntil: block.lockedUntil,
      lockedBy: block.lockedBy,
    },
  });

  useEffect(() => {
    const onContentBlockLocked = hub.onContentBlockLocked(event => {
      if (event.id === block.id) {
        setLock({
          locked: event.locked,
          lockedUntil: event.lockedUntil,
          lockedBy: event.lockedBy,
        });

        updateBlock({
          ...block,
          ...event,
        });
      }
    });

    const onContentBlockUnlocked = hub.onContentBlockUnlocked(event => {
      if (event.id === block.id) {
        setLock(undefined);

        updateBlock({
          ...block,
          locked: undefined,
          lockedBy: undefined,
          lockedUntil: undefined,
        });
      }
    });

    const onContentBlockUpdated = hub.onContentBlockUpdated(updatedBlock => {
      if (updatedBlock.id === block.id) {
        updateBlock(updatedBlock);
      }
    });

    return () => {
      onContentBlockLocked.unsubscribe();
      onContentBlockUnlocked.unsubscribe();
      onContentBlockUpdated.unsubscribe();
    };
  }, [block, hub, setLock, updateBlock]);

  const handleSave = useCallback(
    async (content: string) => {
      await updateContentSectionBlock({
        releaseId,
        sectionId,
        sectionKey,
        blockId: block.id,
        bodyContent: insertReleaseIdPlaceholders(content),
      });

      removeUnsavedBlock(block.id);
    },
    [
      block.id,
      updateContentSectionBlock,
      removeUnsavedBlock,
      releaseId,
      sectionId,
      sectionKey,
    ],
  );

  const handleSubmit = useCallback(
    async (content: string) => {
      await handleSave(content);

      clearUnsavedCommentDeletions(block.id);
      await endLock();
    },
    [handleSave, clearUnsavedCommentDeletions, block.id, endLock],
  );

  const handleDelete = useCallback(async () => {
    await deleteContentSectionBlock({
      releaseId,
      sectionId,
      sectionKey,
      blockId: block.id,
    });
  }, [block.id, deleteContentSectionBlock, releaseId, sectionId, sectionKey]);

  const handleSaveEmbedBlock = useCallback(
    async (updatedBlock: EditableEmbedFormValues) => {
      await updateEmbedSectionBlock({
        releaseId,
        sectionId,
        sectionKey,
        blockId: block.id,
        request: {
          title: updatedBlock.title,
          url: updatedBlock.url,
        },
      });

      removeUnsavedBlock(block.id);
    },
    [
      block.id,
      updateEmbedSectionBlock,
      removeUnsavedBlock,
      releaseId,
      sectionId,
      sectionKey,
    ],
  );

  const handleDeleteEmbedBlock = useCallback(async () => {
    await deleteEmbedSectionBlock({
      releaseId,
      sectionId,
      sectionKey,
      blockId: block.id,
    });
  }, [block.id, deleteEmbedSectionBlock, releaseId, sectionId, sectionKey]);

  const handleBlur = useCallback(
    (isDirty: boolean) => {
      if (isDirty) {
        addUnsavedBlock(block.id);
      }
    },
    [addUnsavedBlock, block.id],
  );

  const handleSaveComment = useCallback(
    async (comment: CommentCreate) => {
      const newComment = await addBlockComment({
        releaseId,
        sectionId,
        sectionKey,
        blockId: block.id,
        comment,
      });

      updateUnresolvedComments.current(block.id, newComment.id);

      return newComment;
    },
    [
      addBlockComment,
      block.id,
      releaseId,
      sectionId,
      sectionKey,
      updateUnresolvedComments,
    ],
  );

  const handlePendingDeleteComment = useCallback(
    (commentId: string) => {
      updateUnsavedCommentDeletions.current(block.id, commentId);
    },
    [block.id, updateUnsavedCommentDeletions],
  );

  const handleDeleteComment = useCallback(
    async (commentId: string) => {
      await deleteBlockComment({
        releaseId,
        sectionId,
        sectionKey,
        blockId: block.id,
        commentId,
      });
    },
    [block.id, deleteBlockComment, releaseId, sectionId, sectionKey],
  );

  const handleSaveUpdatedComment = useCallback(
    async (comment: Comment) => {
      await updateBlockComment({
        releaseId,
        sectionId,
        sectionKey,
        blockId: block.id,
        comment,
      });

      updateUnresolvedComments.current(block.id, comment.id);
    },
    [
      block.id,
      releaseId,
      sectionId,
      sectionKey,
      updateBlockComment,
      updateUnresolvedComments,
    ],
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
    case 'EmbedBlockLink': {
      return (
        <EditableEmbedBlock
          editable={editable}
          block={block}
          visible={visible}
          onDelete={handleDeleteEmbedBlock}
          onSubmit={handleSaveEmbedBlock}
        />
      );
    }
    case 'HtmlBlock':
    case 'MarkDownBlock': {
      return (
        <CommentsContextProvider
          comments={block.comments}
          onDelete={handleDeleteComment}
          onPendingDelete={handlePendingDeleteComment}
          onPendingDeleteUndo={handlePendingDeleteComment}
          onCreate={handleSaveComment}
          onUpdate={handleSaveUpdatedComment}
        >
          <EditableContentBlock
            actionThrottle={lockThrottle}
            allowComments={allowComments}
            editable={editable && !isBrowser('IE')}
            hideLabel
            id={blockId}
            isEditing={isLockedByUser}
            isLoading={isLocking}
            label="Content block"
            locked={locked}
            lockedBy={isLockedByOtherUser ? lockedBy : undefined}
            transformImageAttributes={transformImageAttributes}
            useMarkdown={block.type === 'MarkDownBlock'}
            value={block.body}
            onActive={refreshLock}
            onAutoSave={handleSave}
            onBlur={handleBlur}
            onIdle={endLock}
            onSubmit={handleSubmit}
            onDelete={handleDelete}
            onEditing={startLock}
            onImageUpload={allowImages ? handleImageUpload : undefined}
            onImageUploadCancel={
              allowImages ? handleImageUploadCancel : undefined
            }
          />
        </CommentsContextProvider>
      );
    }
    default:
      return <div>Unable to edit content</div>;
  }
};

export default ReleaseEditableBlock;
