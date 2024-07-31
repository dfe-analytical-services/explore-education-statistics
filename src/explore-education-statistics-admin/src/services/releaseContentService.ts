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

export interface EditableReleaseVersion
  extends Omit<
    Release<EditableContentBlock, EditableDataBlock, EditableEmbedBlock>,
    'published'
  > {
  approvalStatus: ReleaseApprovalStatus;
  publicationId: string;
  published?: string;
  publishScheduled?: string;
}

export interface ReleaseContent {
  release: EditableReleaseVersion;
  unattachedDataBlocks: DataBlock[];
}

export interface ContentBlockAttachRequest {
  contentBlockId: string;
  order: number;
}

const releaseContentService = {
  getContent(
    releaseVersionId: string,
    isPrerelease = false,
  ): Promise<ReleaseContent> {
    return client.get<ReleaseContent>(`/release/${releaseVersionId}/content`, {
      params: { isPrerelease },
    });
  },
  addContentSection(
    releaseVersionId: string,
    order: number,
  ): Promise<ContentSectionViewModel> {
    return client.post<ContentSectionViewModel>(
      `/release/${releaseVersionId}/content/sections/add`,
      { order },
    );
  },
  updateContentSectionsOrder(
    releaseVersionId: string,
    order: Dictionary<number>,
  ): Promise<ContentSectionViewModel[]> {
    return client.put<ContentSectionViewModel[]>(
      `/release/${releaseVersionId}/content/sections/order`,
      order,
    );
  },
  removeContentSection(
    releaseVersionId: string,
    sectionId: string,
  ): Promise<ContentSectionViewModel[]> {
    return client.delete<ContentSectionViewModel[]>(
      `/release/${releaseVersionId}/content/section/${sectionId}`,
    );
  },

  addContentSectionBlock(
    releaseVersionId: string,
    sectionId: string,
    block: ContentBlockPostModel,
  ): Promise<EditableContentBlock> {
    return client.post<EditableContentBlock>(
      `/release/${releaseVersionId}/content/section/${sectionId}/blocks/add`,
      block,
    );
  },

  addEmbedSectionBlock(
    releaseVersionId: string,
    request: EmbedBlockCreateRequest,
  ): Promise<EditableEmbedBlock> {
    return client.post<EditableEmbedBlock>(
      `/release/${releaseVersionId}/embed-blocks`,
      request,
    );
  },

  updateEmbedSectionBlock(
    releaseVersionId: string,
    contentBlockId: string,
    request: EmbedBlockUpdateRequest,
  ): Promise<EditableEmbedBlock> {
    return client.put<EditableEmbedBlock>(
      `/release/${releaseVersionId}/embed-blocks/${contentBlockId}`,
      request,
    );
  },

  deleteEmbedSectionBlock(
    releaseVersionId: string,
    blockId: string,
  ): Promise<void> {
    return client.delete(
      `/release/${releaseVersionId}/embed-blocks/${blockId}`,
    );
  },

  updateContentSectionHeading(
    releaseVersionId: string,
    sectionId: string,
    heading: string,
  ): Promise<ContentSectionViewModel> {
    return client.put<ContentSectionViewModel>(
      `/release/${releaseVersionId}/content/section/${sectionId}/heading`,
      { heading },
    );
  },

  updateContentSectionBlock(
    releaseVersionId: string,
    sectionId: string,
    blockId: string,
    block: ContentBlockPutModel,
  ): Promise<EditableContentBlock> {
    return client.put<EditableContentBlock>(
      `/release/${releaseVersionId}/content/section/${sectionId}/block/${blockId}`,
      block,
    );
  },

  updateContentSectionBlocksOrder(
    releaseVersionId: string,
    sectionId: string,
    order: Dictionary<number>,
  ): Promise<EditableBlock[]> {
    return client.put<EditableBlock[]>(
      `/release/${releaseVersionId}/content/section/${sectionId}/blocks/order`,
      order,
    );
  },

  deleteContentSectionBlock(
    releaseVersionId: string,
    sectionId: string,
    blockId: string,
  ): Promise<void> {
    return client.delete(
      `/release/${releaseVersionId}/content/section/${sectionId}/block/${blockId}`,
    );
  },

  attachContentSectionBlock(
    releaseVersionId: string,
    sectionId: string,
    block: ContentBlockAttachRequest,
  ): Promise<EditableBlock> {
    return client.post<EditableBlock>(
      `/release/${releaseVersionId}/content/section/${sectionId}/blocks/attach`,
      block,
    );
  },
};

export default releaseContentService;
