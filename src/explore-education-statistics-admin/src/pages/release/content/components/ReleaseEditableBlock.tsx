import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { useReleaseContentHubContext } from '@admin/contexts/ReleaseContentHubContext';
import useBlockLock from '@admin/hooks/useBlockLock';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import { ContentSectionKeys } from '@admin/pages/release/content/contexts/ReleaseContentContextActionTypes';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { useReleaseContentDispatch } from '@admin/pages/release/content/contexts/ReleaseContentContext';
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
import useAsyncCallback from '@common/hooks/useAsyncCallback';
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
    updateContentSectionBlock,
    deleteContentSectionBlock,
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
      }
    });

    const onContentBlockUnlocked = hub.onContentBlockUnlocked(event => {
      if (event.id === block.id) {
        setLock(undefined);
      }
    });

    const onContentBlockUpdated = hub.onContentBlockUpdated(event => {
      if (event.id === block.id) {
        dispatch({
          type: 'UPDATE_BLOCK_FROM_SECTION',
          payload: {
            block: event,
            meta: {
              blockId: block.id,
              sectionId,
              sectionKey,
            },
          },
        });
      }
    });

    return () => {
      onContentBlockLocked.unsubscribe();
      onContentBlockUnlocked.unsubscribe();
      onContentBlockUpdated.unsubscribe();
    };
  }, [block.id, dispatch, hub, sectionId, sectionKey, setLock]);

  const [{ isLoading: isSaving }, handleSave] = useAsyncCallback(
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

  const handleBlur = useCallback(
    (isDirty: boolean) => {
      if (isDirty) {
        addUnsavedBlock(block.id);
      }
    },
    [addUnsavedBlock, block.id],
  );

  const handleSaveComment = useCallback(
    async (comment: AddComment) =>
      releaseContentCommentService.addContentSectionComment(
        releaseId,
        sectionId,
        block.id,
        comment,
      ),
    [block.id, releaseId, sectionId],
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
    case 'MarkDownBlock': {
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
            actionThrottle={lockThrottle}
            allowComments={allowComments}
            editable={editable && !isBrowser('IE')}
            hideLabel
            id={blockId}
            isEditing={isLockedByUser}
            isLoading={isLocking}
            isSaving={isSaving}
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
