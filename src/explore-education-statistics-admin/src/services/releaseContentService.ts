import {
  ContentBlockPostModel,
  ContentBlockPutModel,
  EditableBlock,
  EditableContentBlock,
  EditableDataBlock,
  EditableEmbedBlock,
  EmbedBlockCreateRequest,
  EmbedBlockUpdateRequest,
} from '@admin/services/types/content';
import client from '@admin/services/utils/service';
import {
  ContentSection,
  Release,
  ReleaseApprovalStatus,
} from '@common/services/publicationService';
import { DataBlock } from '@common/services/types/blocks';
import { Dictionary } from '@common/types';

type ContentSectionViewModel = ContentSection<EditableBlock>;

export interface EditableRelease
  extends Release<EditableContentBlock, EditableDataBlock, EditableEmbedBlock> {
  approvalStatus: ReleaseApprovalStatus;
  publishScheduled?: string;
  publicationId: string;
}

export interface ReleaseContent {
  release: EditableRelease;
  unattachedDataBlocks: DataBlock[];
}

export interface ContentBlockAttachRequest {
  contentBlockId: string;
  order: number;
}

const releaseContentService = {
  getContent(releaseId: string): Promise<ReleaseContent> {
    return client.get<ReleaseContent>(`/release/${releaseId}/content`);
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

  addEmbedSectionBlock(
    releaseId: string,
    request: EmbedBlockCreateRequest,
  ): Promise<EditableEmbedBlock> {
    return client.post<EditableEmbedBlock>(
      `/release/${releaseId}/embed-blocks`,
      request,
    );
  },

  updateEmbedSectionBlock(
    releaseId: string,
    contentBlockId: string,
    request: EmbedBlockUpdateRequest,
  ): Promise<EditableEmbedBlock> {
    return client.put<EditableEmbedBlock>(
      `/release/${releaseId}/embed-blocks/${contentBlockId}`,
      request,
    );
  },

  deleteEmbedSectionBlock(releaseId: string, blockId: string): Promise<void> {
    return client.delete(`/release/${releaseId}/embed-blocks/${blockId}`);
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
