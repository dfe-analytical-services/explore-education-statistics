import client from '@admin/services/utils/service';
import { ContentSection } from '@common/services/publicationService';
import { HtmlBlock } from '@common/services/types/blocks';
import { EditableContentBlock } from '@admin/services/types/content';

// NOTE: EiN content is saved in the EinContentSections and EinContentBlocks db
// tables. But despite that several frontend interfaces/types are shared for content
// across EiN page, releases, and methodologies.

// NOTE: We need to use EditableContentBlock rather than HtmlBlock - despite not
// needing here `comments`, `locked`, etc. - as otherwise we'd need to create
// an alternate version of `EditableContentBlock`. For now, we're avoiding this.
export type EinContentSection = ContentSection<EditableContentBlock>;

// Generic Ein block types
export type EinBlockType = 'HtmlBlock';
type EinContentBlock = HtmlBlock;

export interface EinContentBlockAddRequest {
  type: EinBlockType;
  order?: number;
}

export interface EinHtmlBlockUpdateRequest {
  body: string;
}

export interface EinContent {
  id: string;
  title: string;
  slug: string;
  published?: string;
  content: EinContentSection[];
}

const educationInNumbersContentService = {
  getEducationInNumbersPageContent(
    educationInNumbersPageId: string,
  ): Promise<EinContent> {
    return client.get(
      `/education-in-numbers/${educationInNumbersPageId}/content`,
    );
  },

  addContentSection({
    educationInNumbersPageId,
    order,
  }: {
    educationInNumbersPageId: string;
    order: number;
  }): Promise<EinContentSection> {
    return client.post(
      `/education-in-numbers/${educationInNumbersPageId}/content/sections/add`,
      {
        order,
      },
    );
  },

  updateContentSectionHeading({
    educationInNumbersPageId,
    sectionId,
    heading,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    heading: string;
  }): Promise<EinContentSection> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/heading`,
      { heading },
    );
  },

  updateContentSectionsOrder({
    educationInNumbersPageId,
    order,
  }: {
    educationInNumbersPageId: string;
    order: string[];
  }): Promise<EinContentSection[]> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}/content/sections/order`,
      order,
    );
  },

  removeContentSection({
    educationInNumbersPageId,
    sectionId,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
  }): Promise<EinContentSection[]> {
    return client.delete(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}`,
    );
  },

  addContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    block,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    block: EinContentBlockAddRequest;
  }): Promise<EinContentBlock> {
    return client.post(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/blocks/add`,
      block,
    );
  },

  updateContentSectionHtmlBlock({
    educationInNumbersPageId,
    sectionId,
    blockId,
    block,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    block: EinHtmlBlockUpdateRequest;
  }): Promise<HtmlBlock> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/block/${blockId}/html`,
      block,
    );
  },

  updateContentSectionBlocksOrder({
    educationInNumbersPageId,
    sectionId,
    order,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    order: string[];
  }): Promise<EinContentBlock[]> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/blocks/order`,
      order,
    );
  },

  deleteContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    blockId,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
  }): Promise<void> {
    return client.delete(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/block/${blockId}`,
    );
  },
};

export default educationInNumbersContentService;
