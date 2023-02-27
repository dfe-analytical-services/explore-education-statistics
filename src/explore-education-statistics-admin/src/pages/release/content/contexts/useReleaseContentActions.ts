import { useReleaseContentDispatch } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ContentSectionKeys } from '@admin/pages/release/content/contexts/ReleaseContentContextActionTypes';
import releaseContentCommentService, {
  CommentCreate,
} from '@admin/services/releaseContentCommentService';
import releaseContentService, {
  ContentBlockAttachRequest,
} from '@admin/services/releaseContentService';
import {
  Comment,
  ContentBlockPostModel,
  EmbedBlockCreateRequest,
  EmbedBlockUpdateRequest,
} from '@admin/services/types/content';
import { Dictionary } from '@admin/types';
import { useCallback, useMemo } from 'react';
import keyStatisticService, {
  KeyStatisticDataBlockUpdateRequest,
  KeyStatisticTextUpdateRequest,
} from '@admin/services/keyStatisticService';
import { KeyStatistic } from '@common/services/publicationService';
import dataBlockService from '@admin/services/dataBlockService';

export default function useReleaseContentActions() {
  const dispatch = useReleaseContentDispatch();

  const updateUnattachedDataBlocks = useCallback(
    async ({ releaseId }: { releaseId: string }) => {
      const unattachedDataBlocks = await dataBlockService.getUnattachedDataBlocks(
        releaseId,
      );

      dispatch({
        type: 'SET_UNATTACHED_DATABLOCKS',
        payload: unattachedDataBlocks,
      });
    },
    [dispatch],
  );

  const deleteContentSectionBlock = useCallback(
    async ({
      releaseId,
      sectionId,
      blockId,
      sectionKey,
    }: {
      releaseId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
    }) => {
      await releaseContentService.deleteContentSectionBlock(
        releaseId,
        sectionId,
        blockId,
      );
      dispatch({
        type: 'REMOVE_SECTION_BLOCK',
        payload: { meta: { sectionId, blockId, sectionKey } },
      });

      await updateUnattachedDataBlocks({ releaseId });
    },
    [dispatch, updateUnattachedDataBlocks],
  );

  const updateContentSectionBlock = useCallback(
    async ({
      releaseId,
      sectionId,
      blockId,
      sectionKey,
      bodyContent,
    }: {
      releaseId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
      bodyContent: string;
    }) => {
      const updateBlock = await releaseContentService.updateContentSectionBlock(
        releaseId,
        sectionId,
        blockId,
        { body: bodyContent },
      );

      dispatch({
        type: 'UPDATE_SECTION_BLOCK',
        payload: {
          meta: { sectionId, blockId, sectionKey },
          block: updateBlock,
        },
      });
    },
    [dispatch],
  );

  const addBlockComment = useCallback(
    async ({
      releaseId,
      sectionId,
      sectionKey,
      blockId,
      comment,
    }: {
      releaseId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      blockId: string;
      comment: CommentCreate;
    }): Promise<Comment> => {
      const newComment = await releaseContentCommentService.addContentSectionComment(
        releaseId,
        sectionId,
        blockId,
        comment,
      );

      dispatch({
        type: 'ADD_BLOCK_COMMENT',
        payload: {
          comment: newComment,
          meta: { sectionId, blockId, sectionKey },
        },
      });

      return newComment;
    },
    [dispatch],
  );

  const deleteBlockComment = useCallback(
    async ({
      sectionId,
      sectionKey,
      blockId,
      commentId,
    }: {
      releaseId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      blockId: string;
      commentId: string;
    }) => {
      await releaseContentCommentService.deleteContentSectionComment(commentId);

      dispatch({
        type: 'REMOVE_BLOCK_COMMENT',
        payload: {
          commentId,
          meta: { sectionId, blockId, sectionKey },
        },
      });
    },
    [dispatch],
  );

  const updateBlockComment = useCallback(
    async ({
      sectionId,
      sectionKey,
      blockId,
      comment,
    }: {
      releaseId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      blockId: string;
      comment: Comment;
    }) => {
      const updatedComment = await releaseContentCommentService.updateContentSectionComment(
        comment,
      );

      dispatch({
        type: 'UPDATE_BLOCK_COMMENT',
        payload: {
          comment: updatedComment,
          meta: { sectionId, blockId, sectionKey },
        },
      });
    },
    [dispatch],
  );

  const addContentSectionBlock = useCallback(
    async ({
      releaseId,
      sectionId,
      sectionKey,
      block,
    }: {
      releaseId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      block: ContentBlockPostModel;
    }) => {
      const newBlock = await releaseContentService.addContentSectionBlock(
        releaseId,
        sectionId,
        block,
      );

      dispatch({
        type: 'ADD_SECTION_BLOCK',
        payload: { meta: { sectionId, sectionKey }, block: newBlock },
      });

      await updateUnattachedDataBlocks({ releaseId });
    },
    [dispatch, updateUnattachedDataBlocks],
  );

  const addEmbedSectionBlock = useCallback(
    async ({
      releaseId,
      sectionId,
      sectionKey,
      request,
    }: {
      releaseId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      request: EmbedBlockCreateRequest;
    }) => {
      const newBlock = await releaseContentService.addEmbedSectionBlock(
        releaseId,
        request,
      );

      dispatch({
        type: 'ADD_SECTION_BLOCK',
        payload: { meta: { sectionId, sectionKey }, block: newBlock },
      });
    },
    [dispatch],
  );

  const deleteEmbedSectionBlock = useCallback(
    async ({
      releaseId,
      sectionId,
      blockId,
      sectionKey,
    }: {
      releaseId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
    }) => {
      await releaseContentService.deleteEmbedSectionBlock(releaseId, blockId);
      dispatch({
        type: 'REMOVE_SECTION_BLOCK',
        payload: { meta: { sectionId, blockId, sectionKey } },
      });
    },
    [dispatch],
  );

  const updateEmbedSectionBlock = useCallback(
    async ({
      releaseId,
      sectionId,
      blockId,
      sectionKey,
      request,
    }: {
      releaseId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
      request: EmbedBlockUpdateRequest;
    }) => {
      const updateBlock = await releaseContentService.updateEmbedSectionBlock(
        releaseId,
        blockId,
        request,
      );

      dispatch({
        type: 'UPDATE_SECTION_BLOCK',
        payload: {
          meta: { sectionId, blockId, sectionKey },
          block: updateBlock,
        },
      });
    },
    [dispatch],
  );

  const attachContentSectionBlock = useCallback(
    async ({
      releaseId,
      sectionId,
      sectionKey,
      block,
    }: {
      releaseId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      block: ContentBlockAttachRequest;
    }) => {
      const newBlock = await releaseContentService.attachContentSectionBlock(
        releaseId,
        sectionId,
        block,
      );

      dispatch({
        type: 'ADD_SECTION_BLOCK',
        payload: { meta: { sectionId, sectionKey }, block: newBlock },
      });

      await updateUnattachedDataBlocks({ releaseId });
    },
    [dispatch, updateUnattachedDataBlocks],
  );

  const updateSectionBlockOrder = useCallback(
    async ({
      releaseId,
      sectionId,
      sectionKey,
      order,
    }: {
      releaseId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      order: Dictionary<number>;
    }) => {
      const sectionContent = await releaseContentService.updateContentSectionBlocksOrder(
        releaseId,
        sectionId,
        order,
      );

      dispatch({
        type: 'UPDATE_SECTION_CONTENT',
        payload: {
          meta: { sectionId, sectionKey },
          sectionContent,
        },
      });
    },
    [dispatch],
  );

  const addContentSection = useCallback(
    async ({ releaseId, order }: { releaseId: string; order: number }) => {
      const newSection = await releaseContentService.addContentSection(
        releaseId,
        order,
      );
      dispatch({
        type: 'ADD_CONTENT_SECTION',
        payload: {
          section: newSection,
        },
      });
    },
    [dispatch],
  );

  const updateContentSectionsOrder = useCallback(
    async ({
      releaseId,
      order,
    }: {
      releaseId: string;
      order: Dictionary<number>;
    }) => {
      const content = await releaseContentService.updateContentSectionsOrder(
        releaseId,
        order,
      );

      dispatch({
        type: 'SET_CONTENT',
        payload: {
          content,
        },
      });
    },
    [dispatch],
  );

  const removeContentSection = useCallback(
    async ({
      releaseId,
      sectionId,
    }: {
      releaseId: string;
      sectionId: string;
    }) => {
      const content = await releaseContentService.removeContentSection(
        releaseId,
        sectionId,
      );

      dispatch({
        type: 'SET_CONTENT',
        payload: {
          content,
        },
      });
    },
    [dispatch],
  );

  const updateContentSectionHeading = useCallback(
    async ({
      releaseId,
      sectionId,
      title,
    }: {
      releaseId: string;
      sectionId: string;
      title: string;
    }) => {
      const section = await releaseContentService.updateContentSectionHeading(
        releaseId,
        sectionId,
        title,
      );

      dispatch({
        type: 'UPDATE_CONTENT_SECTION',
        payload: {
          meta: { sectionId },
          section,
        },
      });
    },
    [dispatch],
  );

  const addKeyStatisticDataBlock = useCallback(
    async ({ releaseId, dataBlockId }) => {
      const keyStatisticDataBlock = await keyStatisticService.createKeyStatisticDataBlock(
        releaseId,
        { dataBlockId },
      );
      dispatch({
        type: 'ADD_KEY_STATISTIC',
        payload: { keyStatistic: keyStatisticDataBlock },
      });
    },
    [dispatch],
  );

  const addKeyStatisticText = useCallback(
    async ({ releaseId, keyStatisticText }) => {
      const createdKeyStatText = await keyStatisticService.createKeyStatisticText(
        releaseId,
        keyStatisticText,
      );
      dispatch({
        type: 'ADD_KEY_STATISTIC',
        payload: { keyStatistic: createdKeyStatText },
      });
    },
    [dispatch],
  );

  const updateKeyStatisticDataBlock = useCallback(
    async ({
      releaseId,
      keyStatisticId,
      request,
    }: {
      releaseId: string;
      keyStatisticId: string;
      request: KeyStatisticDataBlockUpdateRequest;
    }) => {
      const updatedKeyStatisticDataBlock = await keyStatisticService.updateKeyStatisticDataBlock(
        releaseId,
        keyStatisticId,
        request,
      );
      dispatch({
        type: 'UPDATE_KEY_STATISTIC',
        payload: { keyStatistic: updatedKeyStatisticDataBlock },
      });
    },
    [dispatch],
  );

  const updateKeyStatisticText = useCallback(
    async ({
      releaseId,
      keyStatisticId,
      request,
    }: {
      releaseId: string;
      keyStatisticId: string;
      request: KeyStatisticTextUpdateRequest;
    }) => {
      const updatedKeyStatisticText = await keyStatisticService.updateKeyStatisticText(
        releaseId,
        keyStatisticId,
        request,
      );
      dispatch({
        type: 'UPDATE_KEY_STATISTIC',
        payload: { keyStatistic: updatedKeyStatisticText },
      });
    },
    [dispatch],
  );

  const deleteKeyStatistic = useCallback(
    async ({
      releaseId,
      keyStatisticId,
    }: {
      releaseId: string;
      keyStatisticId: string;
    }) => {
      await keyStatisticService.deleteKeyStatistic(releaseId, keyStatisticId);

      dispatch({
        type: 'REMOVE_KEY_STATISTIC',
        payload: { keyStatisticId },
      });
    },
    [dispatch],
  );

  const reorderKeyStatistics = useCallback(
    async ({
      releaseId,
      keyStatistics,
    }: {
      releaseId: string;
      keyStatistics: KeyStatistic[];
    }) => {
      const reorderedKeyStatistics = await keyStatisticService.reorderKeyStatistics(
        releaseId,
        keyStatistics.map(ks => ks.id),
      );

      dispatch({
        type: 'SET_KEY_STATISTICS',
        payload: { keyStatistics: reorderedKeyStatistics },
      });
    },
    [dispatch],
  );

  return useMemo(
    () => ({
      addBlockComment,
      addContentSection,
      addContentSectionBlock,
      addEmbedSectionBlock,
      addKeyStatisticDataBlock,
      addKeyStatisticText,
      attachContentSectionBlock,
      deleteBlockComment,
      deleteContentSectionBlock,
      deleteEmbedSectionBlock,
      deleteKeyStatistic,
      removeContentSection,
      reorderKeyStatistics,
      updateUnattachedDataBlocks,
      updateBlockComment,
      updateContentSectionBlock,
      updateContentSectionHeading,
      updateContentSectionsOrder,
      updateEmbedSectionBlock,
      updateSectionBlockOrder,
      updateKeyStatisticDataBlock,
      updateKeyStatisticText,
    }),
    [
      addBlockComment,
      addContentSection,
      addContentSectionBlock,
      addEmbedSectionBlock,
      addKeyStatisticDataBlock,
      addKeyStatisticText,
      attachContentSectionBlock,
      deleteBlockComment,
      deleteContentSectionBlock,
      deleteEmbedSectionBlock,
      deleteKeyStatistic,
      removeContentSection,
      reorderKeyStatistics,
      updateUnattachedDataBlocks,
      updateBlockComment,
      updateContentSectionBlock,
      updateContentSectionHeading,
      updateContentSectionsOrder,
      updateEmbedSectionBlock,
      updateSectionBlockOrder,
      updateKeyStatisticDataBlock,
      updateKeyStatisticText,
    ],
  );
}
