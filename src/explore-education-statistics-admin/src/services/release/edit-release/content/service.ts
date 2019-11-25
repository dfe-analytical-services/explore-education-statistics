import client from '@admin/services/util/service';
import { Release } from '@common/services/publicationService';
import { Dictionary } from '@common/types/util';
import {
  ContentBlockPutModel,
  ContentBlockViewModel,
  ContentSectionViewModel,
  ManageContentPageViewModel,
} from './types';

export interface ReleaseContentService {
  getRelease: (releaseId: string) => Promise<Release>;
  getContent: (releaseId: string) => Promise<ManageContentPageViewModel>;
  getContentSections: (releaseId: string) => Promise<ContentSectionViewModel[]>;
  updateContentSectionsOrder: (
    releaseId: string,
    order: Dictionary<number>,
  ) => Promise<ContentSectionViewModel[]>;
  updateContentSectionBlock: (
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    block: ContentBlockPutModel,
  ) => Promise<ContentBlockViewModel>;
}

const service: ReleaseContentService = {
  getRelease(releaseId: string): Promise<Release> {
    return client.get<Release>(`/releases/${releaseId}`);
  },
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
