import {
  CreateMethodologyRequest,
  MethodologyContent,
  MethodologyStatusListItem,
  UpdateMethodologyStatusRequest,
} from '@admin/services/methodology/types';
import client from '@admin/services/util/service';
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
