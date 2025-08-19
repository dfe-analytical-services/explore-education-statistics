import {
  ContentBlockPostModel,
  ContentBlockPutModel,
  EditableContentBlock,
} from '@admin/services/types/content';
import client from '@admin/services/utils/service';
import { ContentSection } from '@common/services/publicationService';
import { HtmlBlock } from '@common/services/types/blocks';
import { Dictionary } from '@common/types';

type ContentSectionViewModel = ContentSection<HtmlBlock>;

export interface EducationInNumbersPageContent {
  id: string;
  title: string;
  slug: string;
  published?: string;
  content: ContentSection<HtmlBlock>[];
}

const educationInNumbersContentService = {
  getEducationInNumbersPageContent(
    educationInNumbersPageId: string,
  ): Promise<EducationInNumbersPageContent> {
    // return client.get(`/education-in-numbers/${educationInNumbersPageId}/content`);
    return new Promise(resolve => {
      resolve({
        id: educationInNumbersPageId,
        title: 'Key Statistics',
        slug: 'key-statistics',
        published: '2023-10-01T00:00:00Z',
        content: [
          {
            id: 'section-1',
            heading: 'Section 1',
            order: 1,
            content: [
              {
                id: 'block-1',
                order: 1,
                body: '<p>This is the first block of content in section 1.</p>',
                type: 'HtmlBlock',
              },
            ],
          },
        ],
      });
    });
  },

  addContentSection({
    educationInNumbersPageId,
    order,
  }: {
    educationInNumbersPageId: string;
    order: number;
  }): Promise<ContentSectionViewModel> {
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
  }): Promise<ContentSectionViewModel> {
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
    order: Dictionary<number>;
  }): Promise<ContentSectionViewModel[]> {
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
  }): Promise<ContentSectionViewModel[]> {
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
    block: ContentBlockPostModel;
  }): Promise<EditableContentBlock> {
    return client.post<EditableContentBlock>(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/blocks/add`,
      block,
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
  }): Promise<ContentSectionViewModel[]> {
    return client.delete(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/block/${blockId}`,
    );
  },

  updateContentSectionBlock({
    educationInNumbersPageId,
    sectionId,
    blockId,
    block,
  }: {
    educationInNumbersPageId: string;
    sectionId: string;
    blockId: string;
    block: ContentBlockPutModel;
  }): Promise<EditableContentBlock> {
    return client.put(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/block/${blockId}`,
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
    order: Dictionary<number>;
  }): Promise<EditableContentBlock[]> {
    return client.put<EditableContentBlock[]>(
      `/education-in-numbers/${educationInNumbersPageId}/content/section/${sectionId}/blocks/order`,
      order,
    );
  },
};

export default educationInNumbersContentService;
