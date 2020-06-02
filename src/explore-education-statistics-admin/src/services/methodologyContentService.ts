import { ContentSectionKeys } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContextActionTypes';
import { MethodologyStatus } from '@admin/services/methodologyService';
import {
  ContentBlockPostModel,
  ContentBlockPutModel,
  EditableContentBlock,
} from '@admin/services/types/content';
import client from '@admin/services/utils/service';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';

type ContentSectionViewModel = ContentSection<EditableContentBlock>;

export interface MethodologyContent {
  id: string;
  title: string;
  slug: string;
  status: MethodologyStatus;
  published?: string;
  publishScheduled: string;
  content: ContentSection<EditableContentBlock>[];
  annexes: ContentSection<EditableContentBlock>[];
}

const methodologyContentService = {
  getMethodologyContent(methodologyId: string): Promise<MethodologyContent> {
    return client.get(`/methodology/${methodologyId}/content`);
  },

  addContentSection({
    methodologyId,
    order,
    sectionKey,
  }: {
    methodologyId: string;
    order: number;
    sectionKey: ContentSectionKeys;
  }): Promise<ContentSectionViewModel> {
    if (sectionKey === 'content')
      return client.post(`/methodology/${methodologyId}/content/sections/add`, {
        order,
      });
    return client.post(
      `/methodology/${methodologyId}/content/sections/add?type=${sectionKey}`,
      { order },
    );
  },

  updateContentSectionHeading({
    methodologyId,
    sectionId,
    heading,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    heading: string;
    sectionKey: ContentSectionKeys;
  }): Promise<ContentSectionViewModel> {
    if (sectionKey === 'content')
      return client.put(
        `/methodology/${methodologyId}/content/section/${sectionId}/heading`,
        { heading },
      );

    return client.put(
      `/methodology/${methodologyId}/content/section/${sectionId}/heading?type=${sectionKey}`,
      { heading },
    );
  },

  updateContentSectionsOrder({
    methodologyId,
    order,
    sectionKey,
  }: {
    methodologyId: string;
    order: Dictionary<number>;
    sectionKey: ContentSectionKeys;
  }): Promise<ContentSectionViewModel[]> {
    if (sectionKey === 'content')
      return client.put(
        `/methodology/${methodologyId}/content/sections/order`,
        order,
      );

    return client.put(
      `/methodology/${methodologyId}/content/sections/order?type=${sectionKey}`,
      order,
    );
  },

  removeContentSection({
    methodologyId,
    sectionId,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    sectionKey: ContentSectionKeys;
  }): Promise<ContentSectionViewModel[]> {
    if (sectionKey === 'content')
      return client.delete(
        `/methodology/${methodologyId}/content/section/${sectionId}`,
      );

    return client.delete(
      `/methodology/${methodologyId}/content/section/${sectionId}?type=${sectionKey}`,
    );
  },

  addContentSectionBlock({
    methodologyId,
    sectionId,
    block,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    block: ContentBlockPostModel;
    sectionKey: ContentSectionKeys;
  }): Promise<EditableContentBlock> {
    if (sectionKey === 'content')
      return client.post<EditableContentBlock>(
        `/methodology/${methodologyId}/content/section/${sectionId}/blocks/add`,
        block,
      );

    return client.post<EditableContentBlock>(
      `/methodology/${methodologyId}/content/section/${sectionId}/blocks/add?type=${sectionKey}`,
      block,
    );
  },

  deleteContentSectionBlock({
    methodologyId,
    sectionId,
    blockId,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    blockId: string;
    sectionKey: ContentSectionKeys;
  }): Promise<ContentSectionViewModel[]> {
    if (sectionKey === 'content')
      return client.delete(
        `/methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}`,
      );

    return client.delete(
      `/methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}?type=${sectionKey}`,
    );
  },

  updateContentSectionBlock({
    methodologyId,
    sectionId,
    blockId,
    block,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    blockId: string;
    block: ContentBlockPutModel;
    sectionKey: ContentSectionKeys;
  }): Promise<EditableContentBlock> {
    if (sectionKey === 'content')
      return client.put(
        `/methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}`,
        block,
      );

    return client.put(
      `/methodology/${methodologyId}/content/section/${sectionId}/block/${blockId}?type=${sectionKey}`,
      block,
    );
  },

  updateContentSectionBlocksOrder({
    methodologyId,
    sectionId,
    order,
    sectionKey,
  }: {
    methodologyId: string;
    sectionId: string;
    order: Dictionary<number>;
    sectionKey: ContentSectionKeys;
  }): Promise<EditableContentBlock[]> {
    if (sectionKey === 'content')
      return client.put<EditableContentBlock[]>(
        `/methodology/${methodologyId}/content/section/${sectionId}/blocks/order`,
        order,
      );

    return client.put<EditableContentBlock[]>(
      `/methodology/${methodologyId}/content/section/${sectionId}/blocks/order?type=${sectionKey}`,
      order,
    );
  },
};

export default methodologyContentService;
