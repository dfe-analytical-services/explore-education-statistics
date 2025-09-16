import client from '@admin/services/utils/service';
import { ContentSection } from '@common/services/publicationService';
import {
  EinBlockType,
  EinContentBlock,
  EinFreeTextStatTile,
  EinHtmlBlock,
  EinTileGroupBlock,
} from '@common/services/types/einBlocks';

// ContentSection is shared with release/methodology too
export type EinEditableContentSection = ContentSection<EinContentBlock>;

export interface EinContentBlockAddRequest {
  type: EinBlockType;
  order?: number;
}

export interface EinHtmlBlockUpdateRequest {
  body: string;
}

export interface EinGroupBlockUpdateRequest {
  title?: string;
}

export interface EinContent {
  id: string;
  title: string;
  slug: string;
  published?: string;
  content: EinEditableContentSection[];
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
  }): Promise<EinEditableContentSection> {
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
  }): Promise<EinEditableContentSection> {
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
  }): Promise<EinEditableContentSection[]> {
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
  }): Promise<EinEditableContentSection[]> {
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

  addFreeTextStatTile({
    educationInNumbersPageId,
    blockId,
  }: {
    educationInNumbersPageId: string;
    blockId: string;
  }): Promise<EinFreeTextStatTile> {
    return client.post(
      `/education-in-numbers/${educationInNumbersPageId}/content/block/${blockId}/tiles/add`,
      { type: 'FreeTextStatTile' },
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

  updateContentSectionGroupBlock({
    educationInNumbersPageId,
    sectionId,
    blockId,
    block,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    block: EinGroupBlockUpdateRequest;
  }): Promise<EinTileGroupBlock> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/block/${blockId}/tile-group`,
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
