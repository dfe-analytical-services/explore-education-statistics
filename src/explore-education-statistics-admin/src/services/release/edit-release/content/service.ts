import client from '@admin/services/util/service';
import { Dictionary } from '@common/types/util';
import {
  ContentBlockPostModel,
  ContentBlockPutModel,
  ContentBlockViewModel,
  ContentSectionViewModel,
  ManageContentPageViewModel,
} from './types';

export interface ReleaseContentService {
  getContent: (releaseId: string) => Promise<ManageContentPageViewModel>;
  getContentSections: (releaseId: string) => Promise<ContentSectionViewModel[]>;
  updateContentSectionsOrder: (
    releaseId: string,
    order: Dictionary<number>,
  ) => Promise<ContentSectionViewModel[]>;

  getContentSection: (
    releaseId: string,
    sectionId: string,
  ) => Promise<ContentSectionViewModel>;

  addContentSectionBlock: (
    releaseId: string,
    sectionId: string,
    block: ContentBlockPostModel,
  ) => Promise<ContentBlockViewModel>;

  updateContentSectionBlock: (
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    block: ContentBlockPutModel,
  ) => Promise<ContentBlockViewModel>;
}

const service: ReleaseContentService = {
  getContent(releaseId: string): Promise<ManageContentPageViewModel> {
    return client.get<ManageContentPageViewModel>(
      `/release/${releaseId}/content`,
    );
  },
  getContentSections(releaseId: string): Promise<ContentSectionViewModel[]> {
    return client.get<ContentSectionViewModel[]>(
      `/release/${releaseId}/content/sections`,
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
  ): Promise<ContentBlockViewModel> {
    return client.post<ContentBlockViewModel>(
      `/release/${releaseId}/content/section/${sectionId}/blocks/add`,
      block,
    );
  },

  updateContentSectionBlock(
    releaseId: string,
    sectionId: string,
    blockId: string,
    block: ContentBlockPutModel,
  ): Promise<ContentBlockViewModel> {
    return client.put<ContentBlockViewModel>(
      `/release/${releaseId}/content/section/${sectionId}/block/${blockId}`,
      block,
    );
  },
};

export default service;
