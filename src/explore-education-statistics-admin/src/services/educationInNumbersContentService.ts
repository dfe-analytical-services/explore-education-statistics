import client from '@admin/services/utils/service';
import { ContentSection } from '@common/services/publicationService';
import { HtmlBlock } from '@common/services/types/blocks';
import { EditableContentBlock } from '@admin/services/types/content';

// Generic Ein block types
export type EinBlockType = 'HtmlBlock';
export type EinContentBlock = EinHtmlBlock;

// This is a hack to make the EiN stuff with preexisting block/section content
// code used for release/methodology pages, while keeping a separate EiN type.
// We need to use EditableContentBlock rather than just EinContentBlock -
// despite not needing `comments`, `locked`, etc. - as otherwise we'd need to
// create an alternate version of `EditableContentBlock`. For now, we're
// avoiding this.
export type EinEditableContentBlock = EinContentBlock & EditableContentBlock;
// ContentSection is shared with release/methodology too
export type EinContentSection = ContentSection<EinEditableContentBlock>;
// HtmlBlock is shared with release/methodology as well
type EinHtmlBlock = HtmlBlock;

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
  }): Promise<EinHtmlBlock> {
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
