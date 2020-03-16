import methodologyService from '@admin/services/methodology/methodologyService';
import permissionsService from '@admin/services/permissions/service';
import { Dictionary } from '@admin/types';
import { useCallback } from 'react';
import { ContentBlockPostModel } from 'src/services/release/edit-release/content/types';
import { useMethodologyDispatch } from './MethodologyContext';
import { ContentSectionKeys } from './MethodologyContextActionTypes';

export default function useMethodologyActions() {
  const dispatch = useMethodologyDispatch();
  const annexes = 'annexes';

  const getMethodologyContent = useCallback(
    async (methodologyId: string) => {
      dispatch({ type: 'CLEAR_STATE' });
      const methodology = await methodologyService.getMethodologyContent(
        methodologyId,
      );
      const canUpdateMethodology = await permissionsService.canUpdateMethodology(
        methodologyId,
      );
      dispatch({
        type: 'SET_STATE',
        payload: {
          methodology,
          canUpdateMethodology,
        },
      });
    },
    [dispatch],
  );

  async function deleteContentSectionBlock({
    methodologyId,
    sectionId,
    blockId,
    isAnnex = false,
  }: {
    methodologyId: string;
    sectionId: string;
    blockId: string;
    isAnnex?: boolean;
  }) {
    await methodologyService.deleteContentSectionBlock(
      methodologyId,
      sectionId,
      blockId,
      isAnnex,
    );
    dispatch({
      type: 'REMOVE_BLOCK_FROM_SECTION',
      payload: {
        meta: { sectionId, blockId, sectionKey: isAnnex ? annexes : 'content' },
      },
    });
  }

  async function updateContentSectionBlock({
    methodologyId,
    sectionId,
    blockId,
    bodyContent,
    isAnnex = false,
  }: {
    methodologyId: string;
    sectionId: string;
    blockId: string;
    bodyContent: string;
    isAnnex?: boolean;
  }) {
    const updateBlock = await methodologyService.updateContentSectionBlock(
      methodologyId,
      sectionId,
      blockId,
      { body: bodyContent },
      isAnnex,
    );
    dispatch({
      type: 'UPDATE_BLOCK_FROM_SECTION',
      payload: {
        meta: { sectionId, blockId, sectionKey: isAnnex ? annexes : 'content' },
        block: updateBlock,
      },
    });
  }

  async function addContentSectionBlock({
    methodologyId,
    sectionId,
    sectionKey,
    block,
    isAnnex = false,
  }: {
    methodologyId: string;
    sectionId: string;
    sectionKey: ContentSectionKeys;
    block: ContentBlockPostModel;
    isAnnex?: boolean;
  }) {
    const newBlock = await methodologyService.addContentSectionBlock(
      methodologyId,
      sectionId,
      block,
      isAnnex,
    );
    dispatch({
      type: 'ADD_BLOCK_TO_SECTION',
      payload: { meta: { sectionId, sectionKey }, block: newBlock },
    });
  }

  async function updateSectionBlockOrder({
    methodologyId,
    sectionId,
    order,
    isAnnex = false,
  }: {
    methodologyId: string;
    sectionId: string;
    order: Dictionary<number>;
    isAnnex?: boolean;
  }) {
    const sectionContent = await methodologyService.updateContentSectionBlocksOrder(
      methodologyId,
      sectionId,
      order,
      isAnnex,
    );
    dispatch({
      type: 'UPDATE_SECTION_CONTENT',
      payload: {
        meta: { sectionId, sectionKey: isAnnex ? annexes : 'content' },
        sectionContent,
      },
    });
  }

  async function addContentSection({
    methodologyId,
    order,
    sectionKey,
  }: {
    methodologyId: string;
    order: number;
    sectionKey: ContentSectionKeys;
  }) {
    const newSection = await methodologyService.addContentSection({
      methodologyId,
      order,
      isAnnexes: sectionKey === annexes,
    });
    dispatch({
      type: 'ADD_CONTENT_SECTION',
      payload: {
        sectionKey,
        section: newSection,
      },
    });
  }

  async function updateContentSectionsOrder({
    methodologyId,
    order,
    sectionKey,
  }: {
    methodologyId: string;
    order: Dictionary<number>;
    sectionKey: ContentSectionKeys;
  }) {
    const content = await methodologyService.updateContentSectionsOrder({
      methodologyId,
      order,
      isAnnexes: sectionKey === annexes,
    });
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
        sectionKey,
      },
    });
  }

  async function removeContentSection(
    methodologyId: string,
    sectionId: string,
    sectionKey: ContentSectionKeys,
  ) {
    const content = await methodologyService.removeContentSection(
      methodologyId,
      sectionId,
    );
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
        sectionKey,
      },
    });
  }

  async function updateContentSectionHeading(
    methodologyId: string,
    sectionId: string,
    title: string,
    sectionKey: ContentSectionKeys,
  ) {
    const section = await methodologyService.updateContentSectionHeading(
      methodologyId,
      sectionId,
      title,
    );

    dispatch({
      type: 'UPDATE_CONTENT_SECTION',
      payload: {
        meta: { sectionId, sectionKey },
        section,
      },
    });
  }

  return {
    getMethodologyContent,
    deleteContentSectionBlock,
    updateContentSectionBlock,
    addContentSectionBlock,
    updateSectionBlockOrder,
    addContentSection,
    updateContentSectionsOrder,
    removeContentSection,
    updateContentSectionHeading,
  };
}
