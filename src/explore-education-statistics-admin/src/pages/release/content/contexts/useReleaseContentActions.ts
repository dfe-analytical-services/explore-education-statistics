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
  KeyStatisticTextSaveRequest,
  KeyStatisticTextUpdateRequest,
} from '@admin/services/keyStatisticService';
import { KeyStatistic } from '@common/services/publicationService';
import dataBlockService from '@admin/services/dataBlockService';

export default function useReleaseContentActions() {
  const dispatch = useReleaseContentDispatch();

  const updateUnattachedDataBlocks = useCallback(
    async ({ releaseVersionId }: { releaseVersionId: string }) => {
      const unattachedDataBlocks =
        await dataBlockService.getUnattachedDataBlocks(releaseVersionId);

      dispatch({
        type: 'SET_UNATTACHED_DATABLOCKS',
        payload: unattachedDataBlocks,
      });
    },
    [dispatch],
  );

  const deleteContentSectionBlock = useCallback(
    async ({
      releaseVersionId,
      sectionId,
      blockId,
      sectionKey,
    }: {
      releaseVersionId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
    }) => {
      await releaseContentService.deleteContentSectionBlock(
        releaseVersionId,
        sectionId,
        blockId,
      );
      dispatch({
        type: 'REMOVE_SECTION_BLOCK',
        payload: { meta: { sectionId, blockId, sectionKey } },
      });

      await updateUnattachedDataBlocks({ releaseVersionId });
    },
    [dispatch, updateUnattachedDataBlocks],
  );

  const updateContentSectionBlock = useCallback(
    async ({
      releaseVersionId,
      sectionId,
      blockId,
      sectionKey,
      bodyContent,
    }: {
      releaseVersionId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
      bodyContent: string;
    }) => {
      const updateBlock = await releaseContentService.updateContentSectionBlock(
        releaseVersionId,
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
      releaseVersionId,
      sectionId,
      sectionKey,
      blockId,
      comment,
    }: {
      releaseVersionId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      blockId: string;
      comment: CommentCreate;
    }): Promise<Comment> => {
      const newComment =
        await releaseContentCommentService.addContentSectionComment(
          releaseVersionId,
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
      releaseVersionId: string;
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
      releaseVersionId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      blockId: string;
      comment: Comment;
    }) => {
      const updatedComment =
        await releaseContentCommentService.updateContentSectionComment(comment);

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
      releaseVersionId,
      sectionId,
      sectionKey,
      block,
    }: {
      releaseVersionId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      block: ContentBlockPostModel;
    }) => {
      const newBlock = await releaseContentService.addContentSectionBlock(
        releaseVersionId,
        sectionId,
        block,
      );

      dispatch({
        type: 'ADD_SECTION_BLOCK',
        payload: { meta: { sectionId, sectionKey }, block: newBlock },
      });

      await updateUnattachedDataBlocks({ releaseVersionId });

      return newBlock;
    },
    [dispatch, updateUnattachedDataBlocks],
  );

  const addEmbedSectionBlock = useCallback(
    async ({
      releaseVersionId,
      sectionId,
      sectionKey,
      request,
    }: {
      releaseVersionId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      request: EmbedBlockCreateRequest;
    }) => {
      const newBlock = await releaseContentService.addEmbedSectionBlock(
        releaseVersionId,
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
      releaseVersionId,
      sectionId,
      blockId,
      sectionKey,
    }: {
      releaseVersionId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
    }) => {
      await releaseContentService.deleteEmbedSectionBlock(
        releaseVersionId,
        blockId,
      );
      dispatch({
        type: 'REMOVE_SECTION_BLOCK',
        payload: { meta: { sectionId, blockId, sectionKey } },
      });
    },
    [dispatch],
  );

  const updateEmbedSectionBlock = useCallback(
    async ({
      releaseVersionId,
      sectionId,
      blockId,
      sectionKey,
      request,
    }: {
      releaseVersionId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
      request: EmbedBlockUpdateRequest;
    }) => {
      const updateBlock = await releaseContentService.updateEmbedSectionBlock(
        releaseVersionId,
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
      releaseVersionId,
      sectionId,
      sectionKey,
      block,
    }: {
      releaseVersionId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      block: ContentBlockAttachRequest;
    }) => {
      const newBlock = await releaseContentService.attachContentSectionBlock(
        releaseVersionId,
        sectionId,
        block,
      );

      dispatch({
        type: 'ADD_SECTION_BLOCK',
        payload: { meta: { sectionId, sectionKey }, block: newBlock },
      });

      await updateUnattachedDataBlocks({ releaseVersionId });
    },
    [dispatch, updateUnattachedDataBlocks],
  );

  const updateSectionBlockOrder = useCallback(
    async ({
      releaseVersionId,
      sectionId,
      sectionKey,
      order,
    }: {
      releaseVersionId: string;
      sectionId: string;
      sectionKey: ContentSectionKeys;
      order: Dictionary<number>;
    }) => {
      const sectionContent =
        await releaseContentService.updateContentSectionBlocksOrder(
          releaseVersionId,
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
    async ({
      releaseVersionId,
      order,
    }: {
      releaseVersionId: string;
      order: number;
    }) => {
      const newSection = await releaseContentService.addContentSection(
        releaseVersionId,
        order,
      );
      dispatch({
        type: 'ADD_CONTENT_SECTION',
        payload: {
          section: newSection,
        },
      });
      return newSection;
    },
    [dispatch],
  );

  const updateContentSectionsOrder = useCallback(
    async ({
      releaseVersionId,
      order,
    }: {
      releaseVersionId: string;
      order: Dictionary<number>;
    }) => {
      const content = await releaseContentService.updateContentSectionsOrder(
        releaseVersionId,
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
      releaseVersionId,
      sectionId,
    }: {
      releaseVersionId: string;
      sectionId: string;
    }) => {
      const content = await releaseContentService.removeContentSection(
        releaseVersionId,
        sectionId,
      );

      dispatch({
        type: 'SET_CONTENT',
        payload: {
          content,
        },
      });

      return content;
    },
    [dispatch],
  );

  const updateContentSectionHeading = useCallback(
    async ({
      releaseVersionId,
      sectionId,
      title,
    }: {
      releaseVersionId: string;
      sectionId: string;
      title: string;
    }) => {
      const section = await releaseContentService.updateContentSectionHeading(
        releaseVersionId,
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
    async ({
      releaseVersionId,
      dataBlockId,
    }: {
      releaseVersionId: string;
      dataBlockId: string;
    }) => {
      const keyStatisticDataBlock =
        await keyStatisticService.createKeyStatisticDataBlock(
          releaseVersionId,
          {
            dataBlockId,
          },
        );
      dispatch({
        type: 'ADD_KEY_STATISTIC',
        payload: { keyStatistic: keyStatisticDataBlock },
      });
    },
    [dispatch],
  );

  const addKeyStatisticText = useCallback(
    async ({
      releaseVersionId,
      keyStatisticText,
    }: {
      releaseVersionId: string;
      keyStatisticText: KeyStatisticTextSaveRequest;
    }) => {
      const createdKeyStatText =
        await keyStatisticService.createKeyStatisticText(
          releaseVersionId,
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
      releaseVersionId,
      keyStatisticId,
      request,
    }: {
      releaseVersionId: string;
      keyStatisticId: string;
      request: KeyStatisticDataBlockUpdateRequest;
    }) => {
      const updatedKeyStatisticDataBlock =
        await keyStatisticService.updateKeyStatisticDataBlock(
          releaseVersionId,
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
      releaseVersionId,
      keyStatisticId,
      request,
    }: {
      releaseVersionId: string;
      keyStatisticId: string;
      request: KeyStatisticTextUpdateRequest;
    }) => {
      const updatedKeyStatisticText =
        await keyStatisticService.updateKeyStatisticText(
          releaseVersionId,
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
      releaseVersionId,
      keyStatisticId,
    }: {
      releaseVersionId: string;
      keyStatisticId: string;
    }) => {
      await keyStatisticService.deleteKeyStatistic(
        releaseVersionId,
        keyStatisticId,
      );

      dispatch({
        type: 'REMOVE_KEY_STATISTIC',
        payload: { keyStatisticId },
      });
    },
    [dispatch],
  );

  const reorderKeyStatistics = useCallback(
    async ({
      releaseVersionId,
      keyStatistics,
    }: {
      releaseVersionId: string;
      keyStatistics: KeyStatistic[];
    }) => {
      const reorderedKeyStatistics =
        await keyStatisticService.reorderKeyStatistics(
          releaseVersionId,
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
