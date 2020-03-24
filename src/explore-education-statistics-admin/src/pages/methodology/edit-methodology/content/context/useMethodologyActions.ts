import methodologyService from '@admin/services/methodology/methodologyService';
import permissionService from '@admin/services/permissions/permissionService';
import { ContentBlockPostModel } from '@admin/services/release/edit-release/content/types';
import { Dictionary } from '@admin/types';
import { useCallback } from 'react';
import { useMethodologyDispatch } from './MethodologyContext';
import { ContentSectionKeys } from './MethodologyContextActionTypes';

export default function useMethodologyActions() {
  const dispatch = useMethodologyDispatch();

  const getMethodologyContent = useCallback(
    async (methodologyId: string) => {
      dispatch({ type: 'CLEAR_STATE' });
      const methodology = await methodologyService.getMethodologyContent(
        methodologyId,
      );
      const canUpdateMethodology = await permissionService.canUpdateMethodology(
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
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    blockId: string;
    sectionKey: ContentSectionKeys;
  }) {
    await methodologyService.deleteContentSectionBlock({
      methodologyId,
      sectionId,
      blockId,
      sectionKey,
    });
    dispatch({
      type: 'REMOVE_BLOCK_FROM_SECTION',
      payload: {
        meta: {
          sectionId,
          blockId,
          sectionKey,
        },
      },
    });
  }

  async function updateContentSectionBlock({
    methodologyId,
    sectionId,
    blockId,
    bodyContent,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    blockId: string;
    bodyContent: string;
    sectionKey: ContentSectionKeys;
  }) {
    const updateBlock = await methodologyService.updateContentSectionBlock({
      methodologyId,
      sectionId,
      blockId,
      block: { body: bodyContent },
      sectionKey,
    });
    dispatch({
      type: 'UPDATE_BLOCK_FROM_SECTION',
      payload: {
        meta: {
          sectionId,
          blockId,
          sectionKey,
        },
        block: updateBlock,
      },
    });
  }

  async function addContentSectionBlock({
    methodologyId,
    sectionId,
    block,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;

    block: ContentBlockPostModel;
    sectionKey: ContentSectionKeys;
  }) {
    const newBlock = await methodologyService.addContentSectionBlock({
      methodologyId,
      sectionId,
      block,
      sectionKey,
    });
    dispatch({
      type: 'ADD_BLOCK_TO_SECTION',
      payload: {
        meta: { sectionId, sectionKey },
        block: newBlock,
      },
    });
  }

  async function updateSectionBlockOrder({
    methodologyId,
    sectionId,
    order,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    order: Dictionary<number>;
    sectionKey: ContentSectionKeys;
  }) {
    const sectionContent = await methodologyService.updateContentSectionBlocksOrder(
      { methodologyId, sectionId, order, sectionKey },
    );
    dispatch({
      type: 'UPDATE_SECTION_CONTENT',
      payload: {
        meta: { sectionId, sectionKey },
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
      sectionKey,
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
      sectionKey,
    });
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
        sectionKey,
      },
    });
  }

  async function removeContentSection({
    methodologyId,
    sectionId,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    sectionKey: ContentSectionKeys;
  }) {
    const content = await methodologyService.removeContentSection({
      methodologyId,
      sectionId,
      sectionKey,
    });
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
        sectionKey,
      },
    });
  }

  async function updateContentSectionHeading({
    methodologyId,
    sectionId,
    heading,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    heading: string;
    sectionKey: ContentSectionKeys;
  }) {
    const section = await methodologyService.updateContentSectionHeading({
      methodologyId,
      sectionId,
      heading,
      sectionKey,
    });

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
