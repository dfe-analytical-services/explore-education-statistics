import client from '@admin/services/util/service';
import {
  MethodologyStatus,
  CreateMethodologyRequest,
  MethodologyContent,
} from '@admin/services/methodology/types';
import { IdTitlePair } from '../common/types';

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
};

export default service;
