import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import EditableEmbedBlock from '@admin/components/editable/EditableEmbedBlock';
import CommentsWrapper from '@admin/components/comments/CommentsWrapper';
import { releaseToolbarConfigFull } from '@admin/config/ckEditorConfig';
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
import React, { ReactNode, useCallback, useEffect } from 'react';
import { generatePath } from 'react-router';
import useToggle from '@common/hooks/useToggle';

interface Props {
  allowComments?: boolean;
  allowImages?: boolean;
  block: EditableBlock;
  editable?: boolean;
  editButtonLabel?: ReactNode | string;
  publicationId: string;
  releaseVersionId: string;
  removeButtonLabel?: ReactNode | string;
  sectionId: string;
  sectionKey: ContentSectionKeys;
  visible?: boolean;
  onAfterDeleteBlock?: () => void;
  sectionHeading?: string;
  ix?: number;
}

const ReleaseEditableBlock = ({
  allowComments = false,
  allowImages = false,
  block,
  editable = true,
  editButtonLabel,
  publicationId,
  releaseVersionId,
  removeButtonLabel,
  sectionId,
  sectionKey,
  visible,
  onAfterDeleteBlock,
  sectionHeading,
  ix = 0,
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

  const getChartFile = useGetChartFile(releaseVersionId);

  const { handleImageUpload, handleImageUploadCancel } =
    useReleaseImageUpload(releaseVersionId);

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseVersionId,
  });

  const [showCommentAddForm, toggleCommentAddForm] = useToggle(false);

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
        releaseVersionId,
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
      releaseVersionId,
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
      releaseVersionId,
      sectionId,
      sectionKey,
      blockId: block.id,
    });

    onAfterDeleteBlock?.();
  }, [
    block.id,
    deleteContentSectionBlock,
    onAfterDeleteBlock,
    releaseVersionId,
    sectionId,
    sectionKey,
  ]);

  const handleSaveEmbedBlock = useCallback(
    async (updatedBlock: EditableEmbedFormValues) => {
      await updateEmbedSectionBlock({
        releaseVersionId,
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
      releaseVersionId,
      sectionId,
      sectionKey,
    ],
  );

  const handleDeleteEmbedBlock = useCallback(async () => {
    await deleteEmbedSectionBlock({
      releaseVersionId,
      sectionId,
      sectionKey,
      blockId: block.id,
    });
  }, [
    block.id,
    deleteEmbedSectionBlock,
    releaseVersionId,
    sectionId,
    sectionKey,
  ]);

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
        releaseVersionId,
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
      releaseVersionId,
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
        releaseVersionId,
        sectionId,
        sectionKey,
        blockId: block.id,
        commentId,
      });
    },
    [block.id, deleteBlockComment, releaseVersionId, sectionId, sectionKey],
  );

  const handleSaveUpdatedComment = useCallback(
    async (comment: Comment) => {
      await updateBlockComment({
        releaseVersionId,
        sectionId,
        sectionKey,
        blockId: block.id,
        comment,
      });

      updateUnresolvedComments.current(block.id, comment.id);
    },
    [
      block.id,
      releaseVersionId,
      sectionId,
      sectionKey,
      updateBlockComment,
      updateUnresolvedComments,
    ],
  );

  const blockId = `block-${block.id}`;

  const labelWithHeading = getVoiceActivationFriendlyLabel();

  function renderBlock() {
    switch (block.type) {
      case 'DataBlock':
        return (
          <CommentsWrapper
            commentType="block"
            id={block.id}
            showCommentAddForm={showCommentAddForm}
            testId={`data-block-comments-${block.name}`}
            onAddCancel={toggleCommentAddForm.off}
            onAddSave={toggleCommentAddForm.off}
            onAdd={toggleCommentAddForm.on}
          >
            <EditableBlockWrapper
              editButtonLabel={editButtonLabel}
              dataBlockEditLink={generatePath<ReleaseDataBlockRouteParams>(
                releaseDataBlockEditRoute.path,
                {
                  publicationId,
                  releaseVersionId,
                  dataBlockId: block.id,
                },
              )}
              removeButtonLabel={removeButtonLabel}
              onDelete={editable ? handleDelete : undefined}
            >
              <Gate condition={!!visible}>
                <DataBlockTabs
                  releaseVersionId={releaseVersionId}
                  id={blockId}
                  dataBlock={block}
                  getInfographic={getChartFile}
                />
              </Gate>
            </EditableBlockWrapper>
          </CommentsWrapper>
        );
      case 'EmbedBlockLink': {
        return (
          <CommentsWrapper
            commentType="block"
            id={block.id}
            showCommentAddForm={showCommentAddForm}
            testId="embed-block-comments"
            onAddCancel={toggleCommentAddForm.off}
            onAddSave={toggleCommentAddForm.off}
            onAdd={toggleCommentAddForm.on}
          >
            <EditableEmbedBlock
              block={block}
              editable={editable}
              editButtonLabel={editButtonLabel}
              removeButtonLabel={removeButtonLabel}
              visible={visible}
              onDelete={handleDeleteEmbedBlock}
              onSubmit={handleSaveEmbedBlock}
            />
          </CommentsWrapper>
        );
      }
      case 'HtmlBlock': {
        return (
          <EditableContentBlock
            actionThrottle={lockThrottle}
            allowComments={allowComments}
            editable={editable && !isBrowser('IE')}
            editButtonLabel={editButtonLabel}
            hideLabel
            id={blockId}
            isEditing={isLockedByUser}
            isLoading={isLocking}
            label={labelWithHeading}
            locked={locked}
            lockedBy={isLockedByOtherUser ? lockedBy : undefined}
            removeButtonLabel={removeButtonLabel}
            toolbarConfig={releaseToolbarConfigFull}
            transformImageAttributes={transformImageAttributes}
            value={block.body || ''}
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
        );
      }
      default:
        return <div>Unable to edit content</div>;
    }
  }

  function getVoiceActivationFriendlyLabel(): string {
    let label: string;
    switch (sectionKey) {
      case 'summarySection':
        label = 'Summary Section';
        break;
      case 'keyStatisticsSecondarySection':
        label = 'Key Statistics Secondary Section';
        break;
      case 'headlinesSection':
        label = 'Headlines Section';
        break;
      case 'relatedDashboardsSection':
        label = 'Related Dashboards Section';
        break;
      case 'content':
      default:
        label = 'Content block';
        break;
    }

    return sectionHeading
      ? `Content block${ix ? ` ${ix}` : ''} for ${sectionHeading} section`
      : label;
  }

  return (
    <CommentsContextProvider
      comments={block.comments}
      onDelete={handleDeleteComment}
      onPendingDelete={handlePendingDeleteComment}
      onPendingDeleteUndo={handlePendingDeleteComment}
      onCreate={handleSaveComment}
      onUpdate={handleSaveUpdatedComment}
    >
      {renderBlock()}
    </CommentsContextProvider>
  );
};

export default ReleaseEditableBlock;
