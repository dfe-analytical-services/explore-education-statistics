import { KeyStatsFormValues } from '@admin/components/editable/EditableKeyStatTile';
import releaseContentService, {
  ContentBlockAttachRequest,
} from '@admin/services/releaseContentService';
import { ContentBlockPostModel } from '@admin/services/types/content';
import { Dictionary } from '@admin/types';
import { useCallback } from 'react';
import { useReleaseDispatch } from './ReleaseContext';
import { ContentSectionKeys } from './ReleaseContextActionTypes';

export default function useReleaseActions() {
  const dispatch = useReleaseDispatch();

  const updateAvailableDataBlocks = useCallback(
    async ({ releaseId }: { releaseId: string }) => {
      const availableDataBlocks = await releaseContentService.getAvailableDataBlocks(
        releaseId,
      );

      dispatch({
        type: 'SET_AVAILABLE_DATABLOCKS',
        payload: availableDataBlocks,
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
        type: 'REMOVE_BLOCK_FROM_SECTION',
        payload: { meta: { sectionId, blockId, sectionKey } },
      });
      // becuase we don't know if a datablock was removed,
      // and so it is now available again
      updateAvailableDataBlocks({ releaseId });
    },
    [dispatch, updateAvailableDataBlocks],
  );

  const updateContentSectionDataBlock = useCallback(
    async ({
      releaseId,
      sectionId,
      blockId,
      sectionKey,
      values,
    }: {
      releaseId: string;
      sectionId: string;
      blockId: string;
      sectionKey: ContentSectionKeys;
      values: KeyStatsFormValues;
    }) => {
      const updateBlock = await releaseContentService.updateContentSectionDataBlock(
        releaseId,
        sectionId,
        blockId,
        values,
      );
      dispatch({
        type: 'UPDATE_BLOCK_FROM_SECTION',
        payload: {
          meta: { sectionId, blockId, sectionKey },
          block: updateBlock,
        },
      });
    },
    [dispatch],
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
        type: 'UPDATE_BLOCK_FROM_SECTION',
        payload: {
          meta: { sectionId, blockId, sectionKey },
          block: updateBlock,
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
        type: 'ADD_BLOCK_TO_SECTION',
        payload: { meta: { sectionId, sectionKey }, block: newBlock },
      });
      // becuase we don't know if a datablock was used,
      // and so it is unavailable
      updateAvailableDataBlocks({ releaseId });
    },
    [dispatch, updateAvailableDataBlocks],
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
        type: 'ADD_BLOCK_TO_SECTION',
        payload: { meta: { sectionId, sectionKey }, block: newBlock },
      });
      updateAvailableDataBlocks({ releaseId });
    },
    [dispatch, updateAvailableDataBlocks],
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

  return {
    updateAvailableDataBlocks,
    deleteContentSectionBlock,
    updateContentSectionDataBlock,
    updateContentSectionBlock,
    addContentSectionBlock,
    attachContentSectionBlock,
    updateSectionBlockOrder,
    addContentSection,
    updateContentSectionsOrder,
    removeContentSection,
    updateContentSectionHeading,
  };
}
