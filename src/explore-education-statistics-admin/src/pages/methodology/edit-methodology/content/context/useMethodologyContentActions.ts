import methodologyContentService from '@admin/services/methodologyContentService';
import { ContentBlockPostModel } from '@admin/services/types/content';
import { Dictionary } from '@admin/types';
import { useMethodologyContentDispatch } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContext';
import { ContentSectionKeys } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContextActionTypes';

export default function useMethodologyContentActions() {
  const dispatch = useMethodologyContentDispatch();

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
    await methodologyContentService.deleteContentSectionBlock({
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
    const updateBlock = await methodologyContentService.updateContentSectionBlock(
      {
        methodologyId,
        sectionId,
        blockId,
        block: { body: bodyContent },
        sectionKey,
      },
    );
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
    const newBlock = await methodologyContentService.addContentSectionBlock({
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
    const sectionContent = await methodologyContentService.updateContentSectionBlocksOrder(
      {
        methodologyId,
        sectionId,
        order,
        sectionKey,
      },
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
    const newSection = await methodologyContentService.addContentSection({
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
    const content = await methodologyContentService.updateContentSectionsOrder({
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
    const content = await methodologyContentService.removeContentSection({
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
    const section = await methodologyContentService.updateContentSectionHeading(
      {
        methodologyId,
        sectionId,
        heading,
        sectionKey,
      },
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
