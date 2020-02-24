import {
  CreateMethodologyRequest,
  MethodologyContent,
  MethodologyStatus,
} from '@admin/services/methodology/types';
import client from '@admin/services/util/service';
import { Dictionary } from 'src/types';
import { IdTitlePair } from '../common/types';
import { ContentSectionViewModel } from '../release/edit-release/content/types';

const service = {
  getMethodologies(): Promise<MethodologyStatus[]> {
    return client.get<MethodologyStatus[]>('/bau/methodology');
  },

  createMethodology(
    createRequest: CreateMethodologyRequest,
  ): Promise<IdTitlePair> {
    return client.post(`/methodologies/`, createRequest);
  },

  getMethodologyContent(methodologyId: string): Promise<MethodologyContent> {
    return client.get(`methodology/${methodologyId}/content`);
  },

  addContentSection(
    methodologyId: string,
    body: { order: number },
    isAnnexes = false,
  ): Promise<ContentSectionViewModel[]> {
    return client.post(
      `methodology/${methodologyId}/content/sections/add${
        isAnnexes ? '?type=annexes' : ''
      }`,
      body,
    );
  },

  updateContentSectionHeading(
    methodologyId: string,
    sectionId: string,
    heading: string,
    isAnnexes = false,
  ): Promise<ContentSectionViewModel[]> {
    return client.put(
      `methodology/${methodologyId}/content/section/${sectionId}/heading${
        isAnnexes ? '?type=annexes' : ''
      }`,
      { heading },
    );
  },

  updateContentSectionsOrder(
    methodologyId: string,
    ids: Dictionary<number>,
    isAnnexes = false,
  ): Promise<ContentSectionViewModel[]> {
    return client.put(
      `methodology/${methodologyId}/content/sections/order${
        isAnnexes ? '?type=annexes' : ''
      }`,
      ids,
    );
  },

  removeContentSection(
    methodologyId: string,
    sectionId: string,
    isAnnexes = false,
  ): Promise<ContentSectionViewModel[]> {
    return client.delete(
      `methodology/${methodologyId}/content/section/${sectionId}${
        isAnnexes ? '?type=annexes' : ''
      }`,
    );
  },
};

export default service;
