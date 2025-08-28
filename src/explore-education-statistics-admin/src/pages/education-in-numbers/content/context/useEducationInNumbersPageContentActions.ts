import { ContentBlockPostModel } from '@admin/services/types/content';
import educationInNumbersContentService from '@admin/services/educationInNumbersContentService';
import { useEducationInNumbersPageContentDispatch } from './EducationInNumbersPageContentContext';

export default function useEducationInNumbersPageContentActions() {
  const dispatch = useEducationInNumbersPageContentDispatch();

  async function deleteContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    blockId,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
  }) {
    await educationInNumbersContentService.deleteContentSectionBlock({
      educationInNumbersPageId,
      sectionId,
      blockId,
    });
    dispatch({
      type: 'REMOVE_BLOCK_FROM_SECTION',
      payload: {
        meta: {
          sectionId,
          blockId,
        },
      },
    });
  }

  async function updateContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    blockId,
    bodyContent,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    bodyContent: string;
  }) {
    const updateBlock =
      await educationInNumbersContentService.updateContentSectionHtmlBlock({
        educationInNumbersPageId,
        sectionId,
        blockId,
        block: { body: bodyContent },
      });
    dispatch({
      type: 'UPDATE_BLOCK_FROM_SECTION',
      payload: {
        meta: {
          sectionId,
          blockId,
        },
        block: updateBlock,
      },
    });
  }

  async function addContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    block,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    block: ContentBlockPostModel;
  }) {
    const newBlock =
      await educationInNumbersContentService.addContentSectionBlock({
        educationInNumbersPageId,
        sectionId,
        block,
      });
    dispatch({
      type: 'ADD_BLOCK_TO_SECTION',
      payload: {
        meta: { sectionId },
        block: newBlock,
      },
    });
    return newBlock;
  }

  async function updateSectionBlockOrder({
    educationInNumbersPageId,
    sectionId,
    order,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    order: string[];
  }) {
    const sectionContent =
      await educationInNumbersContentService.updateContentSectionBlocksOrder({
        educationInNumbersPageId,
        sectionId,
        order,
      });
    dispatch({
      type: 'UPDATE_SECTION_CONTENT',
      payload: {
        meta: { sectionId },
        sectionContent,
      },
    });
  }

  async function addContentSection({
    educationInNumbersPageId,
    order,
  }: {
    educationInNumbersPageId: string;
    order: number;
  }) {
    const newSection = await educationInNumbersContentService.addContentSection(
      {
        educationInNumbersPageId,
        order,
      },
    );
    dispatch({
      type: 'ADD_CONTENT_SECTION',
      payload: {
        section: newSection,
      },
    });
    return newSection;
  }

  async function updateContentSectionsOrder({
    educationInNumbersPageId,
    order,
  }: {
    educationInNumbersPageId: string;
    order: string[];
  }) {
    const content =
      await educationInNumbersContentService.updateContentSectionsOrder({
        educationInNumbersPageId,
        order,
      });
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
      },
    });
  }

  async function removeContentSection({
    educationInNumbersPageId,
    sectionId,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
  }) {
    const content = await educationInNumbersContentService.removeContentSection(
      {
        educationInNumbersPageId,
        sectionId,
      },
    );
    dispatch({
      type: 'SET_CONTENT',
      payload: {
        content,
      },
    });

    return content;
  }

  async function updateContentSectionHeading({
    educationInNumbersPageId,
    sectionId,
    heading,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    heading: string;
  }) {
    const section =
      await educationInNumbersContentService.updateContentSectionHeading({
        educationInNumbersPageId,
        sectionId,
        heading,
      });

    dispatch({
      type: 'UPDATE_CONTENT_SECTION',
      payload: {
        meta: { sectionId },
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
