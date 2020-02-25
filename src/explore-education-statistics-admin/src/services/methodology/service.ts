import {
  CreateMethodologyRequest,
  MethodologyContent,
  MethodologyStatusListItem,
  UpdateMethodologyStatusRequest,
} from '@admin/services/methodology/types';
import { ContentSectionViewModel } from '@admin/services/release/edit-release/content/types';
import client from '@admin/services/util/service';
import { Dictionary } from '@common/types';
import {
  BasicMethodology,
  IdTitlePair,
  MethodologyStatus,
} from '../common/types';

const service = {
  getMethodologies(): Promise<MethodologyStatusListItem[]> {
    return client.get<MethodologyStatusListItem[]>('/bau/methodology');
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

  getMethodologyStatus: (methodologyId: string): Promise<MethodologyStatus> =>
    client
      .get<BasicMethodology>(`/methodology/${methodologyId}/summary`)
      .then(methodology => methodology.status),

  updateMethodologyStatus(
    methodologyId: string,
    updateRequest: UpdateMethodologyStatusRequest,
  ): Promise<BasicMethodology> {
    return client.put(`/methodology/${methodologyId}/status`, updateRequest);
  },
};

export default service;
