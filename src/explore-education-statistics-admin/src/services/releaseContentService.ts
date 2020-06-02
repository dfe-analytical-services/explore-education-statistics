import { KeyStatsFormValues } from '@admin/components/editable/EditableKeyStatTile';
import {
  ContentBlockPostModel,
  ContentBlockPutModel,
  EditableBlock,
  EditableContentBlock,
  EditableDataBlock,
} from '@admin/services/types/content';
import client from '@admin/services/utils/service';
import { ContentSection, Release } from '@common/services/publicationService';
import { DataBlock } from '@common/services/types/blocks';
import { Dictionary } from '@common/types';

type ContentSectionViewModel = ContentSection<EditableBlock>;

export type EditableRelease = Release<EditableContentBlock, EditableDataBlock>;

export interface ReleaseContent {
  release: EditableRelease;
  availableDataBlocks: DataBlock[];
}

export interface ContentBlockAttachRequest {
  contentBlockId: string;
  order: number;
}

const releaseContentService = {
  getContent(releaseId: string): Promise<ReleaseContent> {
    return client.get<ReleaseContent>(`/release/${releaseId}/content`);
  },
  getContentSections(releaseId: string): Promise<ContentSectionViewModel[]> {
    return client.get<ContentSectionViewModel[]>(
      `/release/${releaseId}/content/sections`,
    );
  },
  addContentSection(
    releaseId: string,
    order: number,
  ): Promise<ContentSectionViewModel> {
    return client.post<ContentSectionViewModel>(
      `/release/${releaseId}/content/sections/add`,
      { order },
    );
  },
  updateContentSectionsOrder(
    releaseId: string,
    order: Dictionary<number>,
  ): Promise<ContentSectionViewModel[]> {
    return client.put<ContentSectionViewModel[]>(
      `/release/${releaseId}/content/sections/order`,
      order,
    );
  },
  removeContentSection(
    releaseId: string,
    sectionId: string,
  ): Promise<ContentSectionViewModel[]> {
    return client.delete<ContentSectionViewModel[]>(
      `/release/${releaseId}/content/section/${sectionId}`,
    );
  },

  getContentSection(
    releaseId: string,
    sectionId: string,
  ): Promise<ContentSectionViewModel> {
    return client.get<ContentSectionViewModel>(
      `/release/${releaseId}/content/section/${sectionId}`,
    );
  },

  addContentSectionBlock(
    releaseId: string,
    sectionId: string,
    block: ContentBlockPostModel,
  ): Promise<EditableContentBlock> {
    return client.post<EditableContentBlock>(
      `/release/${releaseId}/content/section/${sectionId}/blocks/add`,
      block,
    );
  },

  updateContentSectionHeading(
    releaseId: string,
    sectionId: string,
    heading: string,
  ): Promise<ContentSectionViewModel> {
    return client.put<ContentSectionViewModel>(
      `/release/${releaseId}/content/section/${sectionId}/heading`,
      { heading },
    );
  },

  updateContentSectionBlock(
    releaseId: string,
    sectionId: string,
    blockId: string,
    block: ContentBlockPutModel,
  ): Promise<EditableContentBlock> {
    return client.put<EditableContentBlock>(
      `/release/${releaseId}/content/section/${sectionId}/block/${blockId}`,
      block,
    );
  },

  updateContentSectionDataBlock(
    releaseId: string,
    contentSectionId: string,
    contentBlockId: string,
    newSummary: KeyStatsFormValues,
  ): Promise<EditableDataBlock> {
    return client.put<EditableDataBlock>(
      `/release/${releaseId}/content/section/${contentSectionId}/data-block/${contentBlockId}`,
      newSummary,
    );
  },

  updateContentSectionBlocksOrder(
    releaseId: string,
    sectionId: string,
    order: Dictionary<number>,
  ): Promise<EditableBlock[]> {
    return client.put<EditableBlock[]>(
      `/release/${releaseId}/content/section/${sectionId}/blocks/order`,
      order,
    );
  },

  deleteContentSectionBlock(
    releaseId: string,
    sectionId: string,
    blockId: string,
  ): Promise<void> {
    return client.delete(
      `/release/${releaseId}/content/section/${sectionId}/block/${blockId}`,
    );
  },

  getAvailableDataBlocks(releaseId: string): Promise<DataBlock[]> {
    return client.get(`/release/${releaseId}/content/available-datablocks`);
  },

  attachContentSectionBlock(
    releaseId: string,
    sectionId: string,
    block: ContentBlockAttachRequest,
  ): Promise<EditableBlock> {
    return client.post<EditableBlock>(
      `/release/${releaseId}/content/section/${sectionId}/blocks/attach`,
      block,
    );
  },
};

export default releaseContentService;
