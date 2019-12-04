import client from '@admin/services/util/service';
import {
  MethodologyStatus,
  CreateMethodologyRequest,
} from '@admin/services/methodology/types';
import { IdTitlePair } from '../common/types';

export interface MethodologyService {
  getMethodologies(): Promise<MethodologyStatus[]>;
  createMethodology: (
    createRequest: CreateMethodologyRequest,
  ) => Promise<IdTitlePair>;
}

const service: MethodologyService = {
  getMethodologies(): Promise<MethodologyStatus[]> {
    return client.get<MethodologyStatus[]>('/bau/methodology');
  },

  createMethodology(
    createRequest: CreateMethodologyRequest,
  ): Promise<IdTitlePair> {
    return client.post(`/methodologies/`, createRequest);
  },
};

export default service;
