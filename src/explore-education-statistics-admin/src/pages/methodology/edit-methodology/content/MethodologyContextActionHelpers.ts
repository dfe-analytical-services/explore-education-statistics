import permissionsService from '@admin/services/permissions/service';
import methodologyService from '@admin/services/methodology/service';
import {
  ContentBlockAttachRequest,
  ContentBlockPostModel,
} from '@admin/services/methodology/edit-methodology/content/types';
import { Dictionary } from '@admin/types';
import { useMethodologyDispatch } from './MethodologyContext';
import { ContentSectionKeys } from './MethodologyContextActionTypes';

export default function useMethodologyActions() {
  const dispatch = useMethodologyDispatch();

  async function getMethodologyContent(methodologyId: string) {
    dispatch({ type: 'CLEAR_STATE' });
    const { methodology } = await methodologyService.getContent(methodologyId);
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
  }

  async function deleteContentSectionBlock(
    methodologyId: string,
    sectionId: string,
    blockId: string,
    sectionKey: ContentSectionKeys,
  ) {
    await methodologyService.deleteContentSectionBlock(
      methodologyId,
      sectionId,
      blockId,
    );
    dispatch({
      type: 'REMOVE_BLOCK_FROM_SECTION',
      payload: { meta: { sectionId, blockId, sectionKey } },
    });
  }

  async function updateContentSectionBlock(
    methodologyId: string,
    sectionId: string,
    blockId: string,
    sectionKey: ContentSectionKeys,
    bodyContent: string,
  ) {
    const updateBlock = await methodologyService.updateContentSectionBlock(
      methodologyId,
      sectionId,
      blockId,
      { body: bodyContent },
    );
    dispatch({
      type: 'UPDATE_BLOCK_FROM_SECTION',
      payload: { meta: { sectionId, blockId, sectionKey }, block: updateBlock },
    });
  }

  async function addContentSectionBlock(
    methodologyId: string,
    sectionId: string,
    sectionKey: ContentSectionKeys,
    block: ContentBlockPostModel,
  ) {
    const newBlock = await methodologyService.addContentSectionBlock(
      methodologyId,
      sectionId,
      block,
    );
    dispatch({
      type: 'ADD_BLOCK_TO_SECTION',
      payload: { meta: { sectionId, sectionKey }, block: newBlock },
    });
  }

  async function attachContentSectionBlock(
    methodologyId: string,
    sectionId: string,
    sectionKey: ContentSectionKeys,
    block: ContentBlockAttachRequest,
  ) {
    const newBlock = await methodologyService.attachContentSectionBlock(
      methodologyId,
      sectionId,
      block,
    );
    dispatch({
      type: 'ADD_BLOCK_TO_SECTION',
      payload: { meta: { sectionId, sectionKey }, block: newBlock },
    });
  }

  async function updateSectionBlockOrder(
    methodologyId: string,
    sectionId: string,
    sectionKey: ContentSectionKeys,
    order: Dictionary<number>,
  ) {
    const sectionContent = await methodologyService.updateContentSectionBlocksOrder(
      methodologyId,
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
  }

  async function addContentSection(methodologyId: string, order: number) {
    const newSection = await methodologyService.addContentSection(
      methodologyId,
      order,
    );
    dispatch({
      type: 'ADD_CONTENT_SECTION',
      payload: {
        section: newSection,
      },
    });
  }

  async function updateContentSectionsOrder(
    methodologyId: string,
    order: Dictionary<number>,
  ) {
    const content = await methodologyService.updateContentSectionsOrder(
      methodologyId,
      order,
    );
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
      },
    });
  }

  async function removeContentSection(
    methodologyId: string,
    sectionId: string,
  ) {
    const content = await methodologyService.removeContentSection(
      methodologyId,
      sectionId,
    );
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
      },
    });
  }

  async function updateContentSectionHeading(
    methodologyId: string,
    sectionId: string,
    title: string,
  ) {
    const section = await methodologyService.updateContentSectionHeading(
      methodologyId,
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
  }

  return {
    getMethodologyContent,
    deleteContentSectionBlock,
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
